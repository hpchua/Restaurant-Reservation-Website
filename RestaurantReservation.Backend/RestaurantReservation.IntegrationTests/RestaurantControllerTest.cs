using FluentAssertions;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.IntegrationTests.Configurations.Api.Services;
using RestaurantReservation.IntegrationTests.Configurations.Priority;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservation.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RestaurantControllerTest : RestaurantServices
    {
        #region Get All
        [Fact]
        public async void Task_1_Get_Restaurant_All_Return_Ok()
        {
            //Arrange
            var count = await GetCount();

            //Act
            var restaurants = await GetRestaurants();

            //Assert
            restaurants.Count.Should().Be(count);
        }

        private async Task<List<RestaurantCategoryVM>> GetRestaurants()
        {
            return (List<RestaurantCategoryVM>)await GetAll();
        }
        #endregion

        #region Get By ID
        [Fact]
        public async void Task_1_Get_Restaurant_ById_Return_Ok()
        {
            await AuthenticateMemberAsync();

            //Arrange
            long existingRestaurantID = RestaurantInputs.panMeeRestaurantID;
            string existingRestaurantName = RestaurantInputs.panMeeRestaurantName;

            //Act
            var restaurantToFound = await GetByID(existingRestaurantID, "all", "all");

            //Assert
            restaurantToFound.Should().NotBeNull();
            restaurantToFound.Restaurant.Name.Should().Be(existingRestaurantName);
        }

        [Fact]
        public async void Task_1_Get_Restaurant_ById_Return_NotFound()
        {
            await AuthenticateMemberAsync();

            //Arrange
            long fakeRestaurantID = RestaurantInputs.fakeRestaurantID;

            //Act
            var restaurantToFound = await GetByID(fakeRestaurantID, "all", "all");

            //Assert
            restaurantToFound.Should().BeNull();
        }
        #endregion

        #region Get Edit Info By ID
        [Fact]
        public async void Task_1_Get_Restaurant_EditInfo_ById_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long existingRestaurantID = RestaurantInputs.panMeeRestaurantID;
            string existingRestaurantName = RestaurantInputs.panMeeRestaurantName;

            //Act
            var restaurantToFound = await GetEditInfoById(existingRestaurantID);

            //Assert
            restaurantToFound.Should().NotBeNull();
            restaurantToFound.Restaurant.Name.Should().Be(existingRestaurantName);
        }

        [Fact]
        public async void Task_1_Get_Restaurant_EditInfo_ById_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakeRestaurantID = RestaurantInputs.fakeRestaurantID;

            //Act
            var restaurantToFound = await GetEditInfoById(fakeRestaurantID);

            //Assert
            restaurantToFound.Should().BeNull();
        }
        #endregion

        #region Add
        [Fact]
        public async void Task_2_Add_Restaurant_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime currentTime = RestaurantInputs.currentTime;
            string newRestaurantName = RestaurantInputs.newRestaurantName;
            List<long> categoryIDs = RestaurantInputs.CategoryIDsForCreateRestaurant;

            RestaurantCategoryVM restaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = new Restaurant
                {
                    Name = newRestaurantName,
                    WorkingDay = SD.WorkingDays.Daily.ToString(),
                    StartWorkingTime = currentTime,
                    EndWorkingTime = currentTime.AddHours(9),
                    ImageUrl = "~/images/restaurants/testing.jpg",
                    Address = "99, Jalan Testing, Kuala Lumpur",
                    IsAvailable = true,
                },
                CategoryIds = categoryIDs,
            };

            //Act
            var result = await Create(restaurantCategoryVM);

            //Assert
            result.Should().BeTrue();

            var newRestaurant = await RetrieveRestaurantByName(newRestaurantName);
            newRestaurant.Restaurant.Name.Should().Be(newRestaurantName);
            newRestaurant.Restaurant.ImageUrl.Should().Be("~/images/restaurants/testing.jpg");
            newRestaurant.Restaurant.Address.Should().Be("99, Jalan Testing, Kuala Lumpur");
        }

        private async Task<RestaurantCategoryVM> RetrieveRestaurantByName(string restaurantName)
        {
            var restaurantCategories = await GetRestaurants();
            RestaurantCategoryVM restaurantCategoryToFound = new RestaurantCategoryVM();

            foreach (var restaurantCategory in restaurantCategories)
            {
                if (restaurantCategory.Restaurant.Name.ToLower().Equals(restaurantName.ToLower()))
                {
                    restaurantCategoryToFound.Restaurant = restaurantCategory.Restaurant;
                    restaurantCategoryToFound.CategoryIds = restaurantCategory.CategoryIds;
                }
            }
            return restaurantCategoryToFound;
        }

        [Fact]
        public async void Task_2_Add_Restaurant_With_Duplicate_Name_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime currentTime = RestaurantInputs.currentTime;
            string newRestaurantName = RestaurantInputs.newRestaurantName;
            List<long> categoryIDs = RestaurantInputs.CategoryIDsForCreateRestaurant;

            RestaurantCategoryVM restaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = new Restaurant
                {
                    Name = newRestaurantName,
                    WorkingDay = SD.WorkingDays.Daily.ToString(),
                    StartWorkingTime = currentTime,
                    EndWorkingTime = currentTime.AddHours(9),
                    ImageUrl = "~/images/restaurants/testing.jpg",
                    Address = "99, Jalan Testing, Kuala Lumpur",
                    IsAvailable = true,
                },
                CategoryIds = categoryIDs,
            };

            //Act
            var result = await Create(restaurantCategoryVM);

            //Assert
            result.Should().BeFalse();
        }
        #endregion

        #region Update
        [Fact]
        public async void Task_3_Update_Restaurant_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string newRestaurantName = RestaurantInputs.newRestaurantName;
            string editRestaurantName = RestaurantInputs.editRestaurantName;
            List<long> updatedCategoryIDs = RestaurantInputs.CategoryIDsForUpdateRestaurant;

            var existingRestaurant = await RetrieveRestaurantByName(newRestaurantName);
            existingRestaurant.Restaurant.Name = editRestaurantName;
            existingRestaurant.Restaurant.VersionNo = Convert.ToBase64String(existingRestaurant.Restaurant.RowVersion);

            RestaurantCategoryVM restaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = existingRestaurant.Restaurant,
                CategoryIds = updatedCategoryIDs,
            };

            //Act
            var response = await Update(restaurantCategoryVM.Restaurant.RestaurantID, restaurantCategoryVM);

            //Assert
            response.Should().Be(HttpStatusCode.OK);

            var updatedRestaurant = await RetrieveRestaurantByName(editRestaurantName);

            var updatedRestaurantToFound = await GetByID(updatedRestaurant.Restaurant.RestaurantID, "all", "all");
            updatedRestaurantToFound.Restaurant.Name.Should().Be(editRestaurantName);
            updatedRestaurantToFound.CategoryIds.Should().BeEquivalentTo(updatedCategoryIDs);
        }
        
        [Fact]
        public async void Task_3_Update_Restaurant_With_Duplicate_Name_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string existingRestaurantName = RestaurantInputs.panMeeRestaurantName;
            string editRestaurantName = RestaurantInputs.editRestaurantName;
            List<long> updatedCategoryIDs = RestaurantInputs.CategoryIDsForUpdateRestaurant;

            var existingRestaurant = await RetrieveRestaurantByName(existingRestaurantName);
            existingRestaurant.Restaurant.Name = editRestaurantName;                // SAME NAME DIFFERENT Restaurant
            existingRestaurant.Restaurant.VersionNo = Convert.ToBase64String(existingRestaurant.Restaurant.RowVersion);

            RestaurantCategoryVM restaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = existingRestaurant.Restaurant,
                CategoryIds = updatedCategoryIDs,
            };

            //Act
            var response = await Update(restaurantCategoryVM.Restaurant.RestaurantID, restaurantCategoryVM);

            //Assert
            response.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void Task_3_Update_Restaurant_With_Invalid_Version_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string newRestaurantName = RestaurantInputs.newRestaurantName;
            string editRestaurantName = RestaurantInputs.editRestaurantName;
            List<long> updatedCategoryIDs = RestaurantInputs.CategoryIDsForUpdateRestaurant;

            var existingRestaurant = await RetrieveRestaurantByName(editRestaurantName);
            existingRestaurant.Restaurant.Name = newRestaurantName;
            existingRestaurant.Restaurant.VersionNo = "WW12345";

            RestaurantCategoryVM restaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = existingRestaurant.Restaurant,
                CategoryIds = updatedCategoryIDs,
            };

            //Act
            var response = await Update(restaurantCategoryVM.Restaurant.RestaurantID, restaurantCategoryVM);

            //Assert
            response.Should().Be(HttpStatusCode.Conflict);
        }
        #endregion

        #region Delete
        [Fact]
        public async void Task_4_Delete_Restaurant_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string editRestaurantName = RestaurantInputs.editRestaurantName;
            var existingRestaurantCategory = await RetrieveRestaurantByName(editRestaurantName);

            //Act
            var response = await Delete(existingRestaurantCategory.Restaurant.RestaurantID);

            //Assert
            response.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async void Task_4_Delete_Restaurant_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakeRestaurantID = RestaurantInputs.fakeRestaurantID;

            //Act
            var response = await Delete(fakeRestaurantID);

            //Assert
            response.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async void Task_5_Delete_Restaurant_Completely_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string editRestaurantName = RestaurantInputs.editRestaurantName;
            var existingRestaurantCategory = await RetrieveRestaurantByName(editRestaurantName);

            //Act
            var response = await DeleteCompletely(existingRestaurantCategory.Restaurant.RestaurantID);

            //Assert
            response.Should().Be(HttpStatusCode.NoContent);
        }
        #endregion
    }
}
