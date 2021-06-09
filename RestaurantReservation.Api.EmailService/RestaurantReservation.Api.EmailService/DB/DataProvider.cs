using Dapper;
using RestaurantReservation.Api.EmailService.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RestaurantReservation.Api.EmailService.DB
{
    public class DataProvider : IDataProvider
    {
        private readonly string connectionString;

        public DataProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<string> GetEmails()
        {
            IEnumerable<ApplicationUser> applicationUsers = null;
            using (var connection = new SqlConnection(connectionString))
            {
                applicationUsers = connection.Query<ApplicationUser>("SELECT Email FROM AspNetUsers WHERE isSubscriber = 1");
            }

            List<string> emails = new List<string>();
            foreach (var user in applicationUsers)
            {
                emails.Add(user.Email);
            }
            return emails;
        }

        public Promotion GetPromotionByID(long ID)
        {
            Promotion promotion = null;

            using (var connection = new SqlConnection(connectionString))
            {
                promotion = connection.QueryFirstOrDefault<Promotion>($"SELECT * FROM Promotion WHERE PromotionID = {ID}");
            }
            return promotion;
        }

        public Restaurant GetRestaurantByID(long ID)
        {
            Restaurant restaurant = null;

            using (var connection = new SqlConnection(connectionString))
            {
                restaurant = connection.QueryFirstOrDefault<Restaurant>($"SELECT * FROM Restaurant WHERE RestaurantID = {ID}");
            }
            return restaurant;
        }

        public IEnumerable<Promotion> GetPendingPromotionEmails()
        {
            IEnumerable<Promotion> promotions = null;
            using (var connection = new SqlConnection(connectionString))
            {
                promotions = connection.Query<Promotion>("SELECT * FROM Promotion P WHERE isEmailCreatedSent = 0");
            }
            return promotions;
        }

        public void UpdatePromotionEmailStatus(long promotionID)
        {
            Promotion promotion = null;
            string updateQuery = "UPDATE Promotion " +
                     "SET isEmailCreatedSent = @isEmailCreatedSent " +
                     "WHERE PromotionID = @PromotionID";

            using (var connection = new SqlConnection(connectionString))
            {
                promotion = connection.QueryFirstOrDefault<Promotion>($"SELECT * FROM Promotion WHERE PromotionID = {promotionID}");

                var parameters = new DynamicParameters();
                parameters.Add("@isEmailCreatedSent", 1, DbType.Boolean);
                parameters.Add("@PromotionID", promotionID, DbType.Int64);

                connection.Execute(updateQuery, parameters);
            }
        }
    }
}
