using FluentAssertions;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.IntegrationTests.Configurations.Api.Services;
using RestaurantReservation.IntegrationTests.Configurations.Priority;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservation.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class UserControllerTest : UserServices
    {
        #region Get By ID
        [Fact]
        public async void Task_1_Get_User_By_Id_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            ApplicationUser adminToFound = await GetUserDetailsByUsername(UserInputs.existingAdminUsername);

            //Act
            var response = await GetByID(adminToFound.Id);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<ApplicationUser> GetUserDetailsByUsername(string username)
        {
            var users = await GetAll();
            ApplicationUser userToFound = null;

            foreach (var user in users)
            {
                if (user.UserName.ToLower().Equals(username.ToLower()))
                    userToFound = user;
            }
            return userToFound;
        }

        [Fact]
        public async void Task_1_Get_User_By_Id_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string fakeUserID = UserInputs.fakeUserID;

            //Act
            var response = await GetByID(fakeUserID);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        #endregion

        #region Check Existing Email
        [Fact]
        public async void Task_2_Check_Existing_Email_Return_Ok()
        {
            //Arrange
            string existingEmail = UserInputs.existingAdminEmail;
            string username = UserInputs.existingAdminUsername;

            //Act
            var response = await CheckExistingUserEmail(existingEmail);

            //Assert
            response.Should().NotBeNull();
            response.UserName.Should().Be(username);
            response.Email.Should().Be(existingEmail);
        }

        [Fact]
        public async void Task_2_Check_Non_Existing_Email_Return_NotFound()
        {
            //Arrange
            string existingEmail = UserInputs.fakeEmail;

            //Act
            var response = await CheckExistingUserEmail(existingEmail);

            //Assert
            response.Should().BeNull();
        }
        #endregion

        #region Change Password
        [Fact]
        public async void Task_3_ChangePassword_With_Correct_Current_Password_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            ApplicationUser adminToFound = await GetUserDetailsByUsername(UserInputs.existingAdminUsername);
            ChangePasswordVM changePasswordVM = new ChangePasswordVM
            {
                UserID = adminToFound.Id,
                CurrentPassword = "testing123",
                NewPassword = "testing1234",
                ConfirmNewPassword = "testing1234"
            };

            //Act
            var response = await ChangePassword(changePasswordVM);

            //Assert
            response.Should().Be(true);

            SetToOriginPassword(changePasswordVM);
        }

        private async void SetToOriginPassword(ChangePasswordVM changePasswordVM)
        {
            changePasswordVM.CurrentPassword = "testing1234";
            changePasswordVM.NewPassword = "testing123";
            changePasswordVM.ConfirmNewPassword = "testing123";
            await ChangePassword(changePasswordVM);
        }

        [Fact]
        public async void Task_3_ChangePassword_With_Incorrect_Current_Password_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            ApplicationUser adminToFound = await GetUserDetailsByUsername(UserInputs.existingAdminUsername);
            ChangePasswordVM changePasswordVM = new ChangePasswordVM
            {
                UserID = adminToFound.Id,
                CurrentPassword = "testing1233",
                NewPassword = "testing1234",
                ConfirmNewPassword = "testing1234"
            };

            //Act
            var response = await ChangePassword(changePasswordVM);

            //Assert
            response.Should().Be(false);
        }
        #endregion

        #region Update Profile
        [Fact]
        public async void Task_4_Update_Profile_Return_Ok()
        {
            await AuthenticateAdminAsync();
            
            //Arrange
            ApplicationUser adminToFound = await GetUserDetailsByUsername(UserInputs.existingAdminUsername);
            ProfileVM profileVM = new ProfileVM
            {
                UserID = adminToFound.Id,
                Username = adminToFound.UserName,
                Name = "Admin User 11",
                PhoneNumber = "010-0000000",
                Email = "admin1@gmail.com",
            };

            //Act
            var response = await UpdateProfile(profileVM);

            //Assert
            response.Should().Be(true);
            SetToOriginProfile(profileVM);
        }

        private async void SetToOriginProfile(ProfileVM profileVM)
        {
            profileVM.Name = "Admin User";
            profileVM.PhoneNumber = "012-3456789";
            profileVM.Email = UserInputs.existingAdminEmail;

            await UpdateProfile(profileVM);
        }

        [Fact]
        public async void Task_4_Update_Profile_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            ApplicationUser adminToFound = await GetUserDetailsByUsername(UserInputs.existingAdminUsername);
            ProfileVM profileVM = new ProfileVM
            {
                Username = adminToFound.UserName,
                Name = "Admin User",
                PhoneNumber = "012-3456789",
                Email = UserInputs.existingAdminEmail,
            };

            //Act
            var response = await UpdateProfile(profileVM);

            //Assert
            response.Should().Be(false);
        }
        #endregion
    }
}
