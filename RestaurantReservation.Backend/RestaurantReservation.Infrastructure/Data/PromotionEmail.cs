using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;

namespace RestaurantReservation.Infrastructure.Data
{
    public class PromotionEmail : IPromotionEmail
    {
        private readonly ILogger<PromotionEmail> logger;
        private readonly IPromotionService promotionService;
        private readonly IPublisher publisher;

        public PromotionEmail(ILogger<PromotionEmail> logger,
                              IPromotionService promotionService,
                              IPublisher publisher)
        {
            this.logger = logger;
            this.promotionService = promotionService;
            this.publisher = publisher;
        }

        public void CheckSentStatus()
        {
            publisher.Publish(
                message: JsonConvert.SerializeObject("CheckPromotionStatus"),
                routingKey: "email.checkPromotionStatus",
                messageAttributes: null
            );
            logger.LogInformation($"Promotion email status has been checked {SD.HTTPLogging.OK}");
        }
    }
}
