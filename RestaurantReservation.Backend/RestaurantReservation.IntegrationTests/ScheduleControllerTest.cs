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
    public class ScheduleControllerTest : ScheduleServices
    {
        #region Get All
        [Fact]
        public async void Task_1_Get_Schedule_All_Return_Ok()
        {
            await AuthenticateMemberAsync();

            //Arrange
            long existingRestaurantID = ScheduleInputs.murniRestaurantID;
            var count = await GetCount(existingRestaurantID);

            //Act
            var restaurants = await GetSchedules(existingRestaurantID);

            //Assert
            restaurants.Count.Should().Be(count);
        }

        private async Task<List<RestaurantSchedule>> GetSchedules(long restaurantID)
        {
            return await GetAll(restaurantID);
        }
        #endregion

        #region Get By ID
        [Fact]
        public async void Task_1_Get_Schedule_ById_Return_Ok()
        {
            await AuthenticateMemberAsync();

            //Arrange
            long existingScheduleID = ScheduleInputs.murniScheduleID;
            long existingRestaurantID = ScheduleInputs.murniRestaurantID;
            string existingRestaurantName = ScheduleInputs.murniRestaurantName;

            //Act
            var scheduleToFound = await GetByID(existingScheduleID);

            //Assert
            scheduleToFound.Should().NotBeNull();

            scheduleToFound.RestaurantID.Should().Be(existingRestaurantID);
            scheduleToFound.RestaurantName.Should().Be(existingRestaurantName);
        }

        [Fact]
        public async void Task_1_Get_Schedule_ById_Return_NotFound()
        {
            await AuthenticateMemberAsync();

            //Arrange
            long fakeScheduleID = ScheduleInputs.fakeScheduleID;

            //Act
            var scheduleToFound = await GetByID(fakeScheduleID);

            //Assert
            scheduleToFound.Should().BeNull();
        }
        #endregion

        #region Add
        [Fact]
        public async void Task_2_Add_Schedule_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime newSchedule = ScheduleInputs.today.AddDays(1);
            long restaurantID = ScheduleInputs.murniRestaurantID;
            string fakeName = ScheduleInputs.fakeAuthorName;

            RestaurantSchedule restaurantSchedule = new RestaurantSchedule
            {
                RestaurantID = restaurantID,
                ScheduleDate = newSchedule,
                StartTime = newSchedule.AddHours(1),
                EndTime = newSchedule.AddHours(2),
                Capacity = 20,
                CreatedBy = fakeName,
                AvailableSeat = 20,
                Status = (int)SD.ScheduleStatus.Available,
            };

            //Act
            var result = await Create(restaurantSchedule);

            //Assert
            result.Should().BeTrue();

            var newRestaurantSchedule = await RetrieveScheduleByRestaurantID(restaurantID);
            newRestaurantSchedule.CreatedBy.Should().Be(fakeName);
            newRestaurantSchedule.ScheduleDate.Should().Be(newSchedule);
            newRestaurantSchedule.RestaurantID.Should().Be(restaurantID);
        }

        private async Task<RestaurantSchedule> RetrieveScheduleByRestaurantID(long restaurantID)
        {
            var schedules = await GetSchedules(restaurantID);
            RestaurantSchedule restaurantSchedule = new RestaurantSchedule();

            foreach (var schedule in schedules)
            {
                if (schedule.CreatedBy.ToLower().Equals(ScheduleInputs.fakeAuthorName.ToLower()) && schedule.RestaurantID == restaurantID)
                {
                    restaurantSchedule = schedule;
                }
            }
            return restaurantSchedule;
        }

        [Fact]
        public async void Task_2_Add_Schedule_With_Duplicate_Info_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            DateTime newSchedule = ScheduleInputs.today.AddDays(1);
            long restaurantID = ScheduleInputs.murniRestaurantID;
            string fakeName = ScheduleInputs.fakeAuthorName;

            RestaurantSchedule restaurantSchedule = new RestaurantSchedule
            {
                RestaurantID = restaurantID,
                ScheduleDate = newSchedule,
                StartTime = newSchedule.AddHours(1),
                EndTime = newSchedule.AddHours(2),
                Capacity = 20,
                AvailableSeat = 20,
                CreatedBy = fakeName,
                Status = (int)SD.ScheduleStatus.Available,
            };

            //Act
            var result = await Create(restaurantSchedule);

            //Assert
            result.Should().BeFalse();
        }
        #endregion

        #region Update
        [Fact]
        public async void Task_3_Update_Schedule_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long restaurantID = ScheduleInputs.murniRestaurantID;

            var existingSchedule = await RetrieveScheduleByRestaurantID(restaurantID);
            existingSchedule.ScheduleDate = existingSchedule.ScheduleDate.AddDays(1);
            existingSchedule.VersionNo = Convert.ToBase64String(existingSchedule.RowVersion);

            //Act
            var response = await Update(existingSchedule.ScheduleID, existingSchedule);

            //Assert
            response.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async void Task_3_Update_Schedule_With_Different_Schedule_ID_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long restaurantID = ScheduleInputs.murniRestaurantID;
            long fakeScheduleID = ScheduleInputs.fakeScheduleID;

            var existingSchedule = await RetrieveScheduleByRestaurantID(restaurantID);
            existingSchedule.ScheduleDate = existingSchedule.ScheduleDate.AddDays(1);
            existingSchedule.VersionNo = Convert.ToBase64String(existingSchedule.RowVersion);

            //Act
            var response = await Update(fakeScheduleID, existingSchedule);

            //Assert
            response.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void Task_3_Update_Schedule_With_Invalid_Version_Return_Conflict()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long restaurantID = ScheduleInputs.murniRestaurantID;

            var existingSchedule = await RetrieveScheduleByRestaurantID(restaurantID);
            existingSchedule.ScheduleDate = existingSchedule.ScheduleDate.AddDays(1);
            existingSchedule.VersionNo = "WW12345";

            //Act
            var response = await Update(existingSchedule.ScheduleID, existingSchedule);

            //Assert
            response.Should().Be(HttpStatusCode.Conflict);
        }
        #endregion

        #region Delete
        [Fact]
        public async void Task_4_Delete_Schedule_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long restaurantID = ScheduleInputs.murniRestaurantID;
            var existingSchedule = await RetrieveScheduleByRestaurantID(restaurantID);

            //Act
            var response = await Delete(existingSchedule.ScheduleID, (int)SD.ScheduleStatus.Unavailable);

            //Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async void Task_4_Delete_Schedule_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakeRestaurantID = ScheduleInputs.fakeRestaurantID;

            //Act
            var response = await Delete(fakeRestaurantID, (int)SD.ScheduleStatus.Unavailable);

            //Assert
            response.Should().BeFalse();
        }

        [Fact]
        public async void Task_5_Delete_Schedule_Completely_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long restaurantID = ScheduleInputs.murniRestaurantID;
            var existingRestaurantDetails = await GetDeleteScheduleInfoByRestaurantID(restaurantID);
            var scheduleToDelete = GetDeleteScheduleInfo(existingRestaurantDetails, restaurantID);

            //Act
            var response = await DeleteCompletely(scheduleToDelete.ScheduleID);

            //Assert
            response.Should().BeTrue();
        }

        private RestaurantSchedule GetDeleteScheduleInfo(RestaurantCategorySchedulePromtionVM restaurantCategorySchedulePromtionVM, long restaurantID)
        {
            RestaurantSchedule scheduleToFound = new RestaurantSchedule();

            foreach (var schedule in restaurantCategorySchedulePromtionVM.RestaurantSchedules)
            {
                if (schedule.CreatedBy.ToLower().Equals(ScheduleInputs.fakeAuthorName.ToLower()) && schedule.RestaurantID == restaurantID)
                    scheduleToFound = schedule;
            }
            return scheduleToFound;
        }
        #endregion
    }
}
