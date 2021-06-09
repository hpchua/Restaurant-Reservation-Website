using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using RestaurantReservation.Api.EmailService.DB;
using RestaurantReservation.Api.EmailService.DTO;
using RestaurantReservation.Api.EmailService.MemoryStorage;
using RestaurantReservation.Api.EmailService.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantReservation.Api.EmailService.BackgroundService
{
    public class DataCollector : IHostedService
    {
        private readonly ISubscriber subscriber;
        private readonly IDataProvider dataProvider;
        private readonly IConfiguration configuration;
        private readonly IMemoryResultStorage memoryResultStorage;

        public DataCollector(ISubscriber subscriber,
                             IDataProvider dataProvider,
                             IConfiguration configuration,
                             IMemoryResultStorage memoryResultStorage)
        {
            this.subscriber = subscriber;
            this.dataProvider = dataProvider;
            this.configuration = configuration;
            this.memoryResultStorage = memoryResultStorage;
        }

        private bool ProcessMessage(string message, IDictionary<string, object> headers)
        {
            bool result = true;

            if (message.Contains("NewPromotion"))
            {
                NewPromotion promotion = JsonConvert.DeserializeObject<NewPromotion>(message);

                if (!promotion.Promotion.isEmailCreatedSent && promotion.Promotion.isAvailable)
                {
                    string restaurantLink = $"{configuration["AppUrl"]}/Restaurants/Details?ID={promotion.Promotion.RestaurantID}";

                    List<string> userEmails = dataProvider.GetEmails();

                    result = SendEmail(
                        userEmails,
                        promotion.Promotion.Name,
                        "<h1>" + promotion.Promotion.Description + "</h1>" +
                        "<h3>" + promotion.Promotion.Restaurant.Name + "</h3>" +
                        "<h2>We have added a new promotion!</h2> <br />" +
                        "<p>" + promotion.Promotion.Content + "</p> <br />" +
                        $"<a href='{restaurantLink}'>Restaurant Link</a> <br />" +
                        $"<small>From <b>{promotion.Promotion.Restaurant.Name}! </b></small>"
                    );
                }
            }
            else if (message.Contains("Booking"))
            {
                BookingReceipt bookingReceipt = JsonConvert.DeserializeObject<BookingReceipt>(message);

                List<string> userEmails = new List<string> { $"{bookingReceipt.Email}" };

                result = SendEmail(
                    userEmails,
                    bookingReceipt.Title,
                    bookingReceipt.Content
                );
            }
            else if (message.Contains("Forgot Password"))
            {
                ForgotPassword forgotPassword = JsonConvert.DeserializeObject<ForgotPassword>(message);
                string resetPasswordUrl = $"{configuration["AppUrl"]}/Account/ResetPassword?email={forgotPassword.Email}&token={forgotPassword.Token}";

                List<string> userEmails = new List<string> { $"{forgotPassword.Email}" };

                result = SendEmail(
                    userEmails,
                    "Reset Password",
                    "<h1>Click the link below to reset your password</h1>" + $"<p><a href='{resetPasswordUrl}'>Reset Password Link</a></p>"
                );
            }
            else if (message.Contains("CheckPromotionStatus"))
            {
                var promotions = dataProvider.GetPendingPromotionEmails();

                foreach(var promotion in promotions)
                {
                    string restaurantLink = $"{configuration["AppUrl"]}/Restaurants/Details?ID={promotion.RestaurantID}";
                    var restaurant = dataProvider.GetRestaurantByID(promotion.RestaurantID);
                    List<string> userEmails = dataProvider.GetEmails();

                    result = SendEmail(
                        userEmails,
                        promotion.Name,
                        "<h1>" + promotion.Description + "</h1>" +
                        "<h3>" + restaurant.Name + "</h3>" +
                        "<p>" + promotion.Content + "</p> <br />" +
                        $"<a href='{restaurantLink}'>Restaurant Link</a> <br />" +
                        $"<small>From <b>{restaurant.Name}! </b></small> <br />"
                    );

                    dataProvider.UpdatePromotionEmailStatus(promotion.PromotionID);
                }
            }
            else
            {
                Promotion promotion = JsonConvert.DeserializeObject<Promotion>(message);

                if (promotion.isEmailCreatedSent && promotion.isAvailable)
                {
                    string restaurantLink = $"{configuration["AppUrl"]}/Restaurants/Details?ID={promotion.RestaurantID}";

                    List<string> userEmails = dataProvider.GetEmails();

                    result = SendEmail(
                        userEmails,
                        promotion.Name,
                        "<h1>" + promotion.Description + "</h1>" +
                        "<h3>" + promotion.Restaurant.Name + "</h3>" +
                        "<p>" + promotion.Content + "</p> <br />" +
                        $"<a href='{restaurantLink}'>Restaurant Link</a> <br />" +
                        $"<small>From <b>{promotion.Restaurant.Name}! </b></small> <br />"
                    );
                }
            }

            memoryResultStorage.Add(result);
            return result;
        }

        private bool SendEmail(List<string> userEmails, string subject, string content)
        {
            foreach (var userEmail in userEmails)
            {
                MailMessage mail = new MailMessage
                {
                    Subject = subject,
                    Body = content,
                    From = new MailAddress(configuration["SMTPConfig:SendEmailAddress"], configuration["SMTPConfig:SenderDisplayName"]),
                    IsBodyHtml = bool.Parse(configuration["SMTPConfig:IsBodyHtml"]),
                };

                mail.To.Add(userEmail); // One to one better in case PDPA (Personal Data Protection Act)

                NetworkCredential networkCredential = new NetworkCredential(configuration["SMTPConfig:SendEmailAddress"], configuration["SMTPConfig:Password"]);
                SmtpClient smtpClient = new SmtpClient()
                {
                    Host = configuration["SMTPConfig:Host"],
                    Port = Convert.ToInt32(configuration["SMTPConfig:Port"]),
                    EnableSsl = bool.Parse(configuration["SMTPConfig:EnableSSL"]),
                    Credentials = networkCredential,
                };
                mail.BodyEncoding = Encoding.Default;

                try
                {
                    smtpClient.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    return false;
                    throw ex;
                }
            }
            return true;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            subscriber.Subscribe(ProcessMessage);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
