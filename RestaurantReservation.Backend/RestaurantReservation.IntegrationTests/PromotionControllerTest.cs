using FluentAssertions;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.IntegrationTests.Configurations.Api.Services;
using RestaurantReservation.IntegrationTests.Configurations.Priority;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservation.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class PromotionControllerTest : PromotionServices
    {
        #region Get By Id
        [Fact]
        public async void Task_1_Get_Promotion_ById_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long existingPromotionID = PromotionInputs.murniPromotionID;
            long existingRestaurantID = PromotionInputs.murniRestaurantID;
            string existingRestaurantName = PromotionInputs.murniRestaurantName;

            //Act
            RestaurantPromotionVM promotionToFound = await GetByID(existingPromotionID);

            //Assert
            promotionToFound.Promotion.Should().NotBeNull();

            promotionToFound.RestaurantID.Should().Be(existingRestaurantID);
            promotionToFound.RestaurantName.Should().Be(existingRestaurantName);
        }

        [Fact]
        public async void Task_1_Get_Promotion_ById_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakePromotionID = PromotionInputs.fakePromotionID;

            //Act
            RestaurantPromotionVM promotionToFound = await GetByID(fakePromotionID);

            //Assert
            promotionToFound.Should().BeNull();
        }
        #endregion

        #region Add
        [Fact]
        public async void Task_2_Add_Promotion_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime today = PromotionInputs.today;
            long existingRestaurantID = PromotionInputs.murniRestaurantID;
            string fakeName = PromotionInputs.fakeAuthorName;

            Promotion promotion = new Promotion
            {
                RestaurantID = existingRestaurantID,
                Name = "Testing123",
                Description = "Promotion on x.x.2021",
                Content = "Having promotion testing123 on x.x.2021",
                Type = SD.PromotionType.Coupon.ToString(),
                isAvailable = true,
                StartDate = today,
                EndDate = today.AddDays(4),
                CreatedBy = fakeName,
                isEmailCreatedSent = false,
            };

            //Act
            var result = await Create(promotion);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void Task_2_Add_Promotion_With_Duplicate_Info_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime today = PromotionInputs.today;
            long existingRestaurantID = PromotionInputs.murniRestaurantID;
            string fakeName = PromotionInputs.fakeAuthorName;

            Promotion promotion = new Promotion
            {
                RestaurantID = existingRestaurantID,
                Name = "Testing123",
                Description = "Promotion on x.x.2021",
                Content = "Having promotion testing123 on x.x.2021",
                Type = SD.PromotionType.Coupon.ToString(),
                isAvailable = true,
                StartDate = today,
                EndDate = today.AddDays(4),
                CreatedBy = fakeName,
                isEmailCreatedSent = false,
            };

            //Act
            var result = await Create(promotion);

            //Assert
            result.Should().BeFalse();
        }
        #endregion

        #region Update
        [Fact]
        public async void Task_3_Update_Promotion_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime today = PromotionInputs.today;
            long existingPromotionID = PromotionInputs.murniPromotionID;

            var restaurantPromotion = await GetByID(existingPromotionID);
            restaurantPromotion.Promotion.EndDate = today.AddDays(7);
            restaurantPromotion.Promotion.VersionNo = Convert.ToBase64String(restaurantPromotion.Promotion.RowVersion);

            //Act
            var response = await Update(existingPromotionID, restaurantPromotion.Promotion);

            //Assert
            response.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async void Task_3_Update_Promotion_With_Different_Schedule_ID_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long promotionID = PromotionInputs.existingPromotionID;

            var restaurantPromotion = await GetByID(promotionID);
            restaurantPromotion.Promotion.Name = "Happy June Promotion";
            restaurantPromotion.Promotion.VersionNo = Convert.ToBase64String(restaurantPromotion.Promotion.RowVersion);

            //Act
            var response = await Update(999, restaurantPromotion.Promotion);

            //Assert
            response.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void Task_3_Update_Promotion_With_Invalid_Version_Return_Conflict()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime today = PromotionInputs.today;
            long promotionID = PromotionInputs.murniPromotionID;

            var restaurantPromotion = await GetByID(promotionID);
            restaurantPromotion.Promotion.EndDate = today.AddDays(7);
            restaurantPromotion.Promotion.VersionNo = "WW123";

            //Act
            var response = await Update(promotionID, restaurantPromotion.Promotion);

            //Assert
            response.Should().Be(HttpStatusCode.Conflict);
        }
        #endregion


        #region Delete
        [Fact]
        public async void Task_5_Delete_Promotion_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string fakeAuthorName = PromotionInputs.fakeAuthorName;
            Promotion promotionToDelete = await GetPromotionByName(fakeAuthorName);

            //Act
            var response = await Delete(promotionToDelete.PromotionID);

            //Assert
            response.Should().BeTrue();
        }

        private async Task<Promotion> GetPromotionByName(string name)
        {
            var restaurantDetails = await GetRestaurantByID(PromotionInputs.murniRestaurantID);
            Promotion promotionToFound = new Promotion();

            foreach (var promotion in restaurantDetails.Promotions)
            {
                if (promotion.Name.ToLower().Equals(name.ToLower()))
                {
                    promotionToFound = promotion;
                }
            }
            return promotionToFound;
        }

        [Fact]
        public async void Task_5_Delete_Promotion_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakePromotionID = PromotionInputs.fakePromotionID;

            //Act
            var response = await Delete(fakePromotionID);

            //Assert
            response.Should().BeFalse();
        }
        #endregion
    }
}
