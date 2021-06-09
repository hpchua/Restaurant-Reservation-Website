using FluentAssertions;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.IntegrationTests.Configurations.Api.Services;
using RestaurantReservation.IntegrationTests.Configurations.Priority;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservation.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class BookingControllerTest : BookingServices
    {
        #region Get All Pending Bookings
        [Fact]
        public async void Task_1_Get_Booking_All_Pending_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            var count = await GetCount();

            //Act
            var bookings = await GetAll(SD.BookingStatus.PENDING);

            //Assert
            bookings.Count.Should().Be(count);
        }
        #endregion

        #region Get By BookingNo
        [Fact]
        public async void Task_1_Get_ByBookingNo_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string bookingNo = BookingInputs.existingBookingNo;

            //Act
            var bookingDetailVM = await GetByBookingNo(bookingNo);

            //Assert
            bookingDetailVM.Booking.Should().NotBeNull();
        }

        [Fact]
        public async void Task_1_Get_ByBookingNo_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string fakeBookingNo = BookingInputs.fakeBookingNo;

            //Act
            var bookingDetailVM = await GetByBookingNo(fakeBookingNo);

            //Assert
            bookingDetailVM.Booking.Should().BeNull();
        }
        #endregion

        #region GetAllBookingsByUserID
        [Fact]
        public async void Task_1_Get_All_ByUserID_Return_Ok()
        {
            await AuthenticateMemberAsync();

            //Arrange
            ApplicationUser userToFound = await GetUserDetails(BookingInputs.existingMemberUsername);

            //Act
            var bookingHistoryVM = await GetAllBookingsByUserID(userToFound.Id, "all");

            //Assert
            bookingHistoryVM.Bookings.Should().NotBeNull();
        }

        private async Task<ApplicationUser> GetUserDetails(string username)
        {
            var users = await GetAllUser();
            ApplicationUser userToFound = new ApplicationUser();

            foreach (var user in users)
            {
                if (user.UserName.ToLower().Equals(username.ToLower()))
                    userToFound = user;
            }
            return userToFound;
        }
        #endregion
    }
}
