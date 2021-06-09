using FluentAssertions;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.IntegrationTests.Configurations.Api.Services;
using RestaurantReservation.IntegrationTests.Configurations.Priority;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservation.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class AuthenticateControllerTest : AuthenticateService
    {
        #region Login
        [Fact]
        public async void Task_1_Login_Valid_Credentials_Return_Ok()
        {
            //Arrange
            LoginVM loginVM = new LoginVM
            {
                Username = "admin",
                Password = "testing123",
                RememberMe = false,
            };

            //Act
            var response = await Login(loginVM);

            //Assert
            response.StatusCode.Should().Be(SD.StatusCode.OK);
        }

        [Fact]
        public async void Task_1_Login_Wrong_Username_Return_NotFound()
        {
            //Arrange
            LoginVM loginVM = new LoginVM
            {
                Username = "adminn",
                Password = "testing123",
                RememberMe = false,
            };

            //Act
            var response = await Login(loginVM);

            //Assert
            response.StatusCode.Should().Be(SD.StatusCode.NOT_FOUND);
        }

        [Fact]
        public async void Task_1_Login_Wrong_Password_Return_Unauthorized()
        {
            //Arrange
            LoginVM loginVM = new LoginVM
            {
                Username = "admin",
                Password = "testingg1234567",
                RememberMe = false,
            };

            //Act
            var response = await Login(loginVM);

            //Assert
            response.StatusCode.Should().Be(SD.StatusCode.UNAUTHORIZED);
        }
        #endregion

        #region Register
        [Fact]
        public async void Task_2_Register_Member_Valid_Details_Return_Ok()
        {
            //Arrange
            RegisterVM registerInput = new RegisterVM
            {
                Name = "Member 99",
                PhoneNumber = "019-9146612",
                Email = "member99@gmail.com",
                Username = AuthenticateInputs.newMockMemberUsername,
                Password = "testing123",
                ConfirmPassword = "testing123",
                Role = SD.ROLE_MEMBER,
            };

            //Act
            var response = await RegisterMember(registerInput);

            //Assert
            response.StatusCode.Should().Be(SD.StatusCode.OK);
            response.Message.Should().BeNull();
        }

        [Fact]
        public async void Task_2_Register_With_Duplicate_Member_Details_Return_BadRequest()
        {
            //Arrange
            RegisterVM registerInput = new RegisterVM
            {
                Name = "Member 1",
                PhoneNumber = "0123456789",
                Email = AuthenticateInputs.existingMemberEmail,
                Username = AuthenticateInputs.existingMemberUsername,
                Password = "testing123",
                ConfirmPassword = "testing123",
                Role = SD.ROLE_MEMBER,
            };

            //Act
            var response = await RegisterMember(registerInput);

            //Assert
            response.StatusCode.Should().Be(SD.StatusCode.BAD_REQUEST);
            response.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void Task_3_Delete_Registered_Member_Details_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            ApplicationUser newMockMember = await RetrieveMemberDetailsByUsername(AuthenticateInputs.newMockMemberUsername); 

            //Act
            var response = await DeleteMemberUser(newMockMember.Id);

            //Assert
            response.Should().Be(true);
        }

        private async Task<ApplicationUser> RetrieveMemberDetailsByUsername(string username)
        {
            var users = await GetAllMemberUsers();
            ApplicationUser memberToFound = new ApplicationUser();

            foreach(var user in users)
            {
                if (user.UserName.ToLower().Equals(username.ToLower()))
                {
                    memberToFound = user;
                }
            }
            return memberToFound;
        }

        #endregion

        #region Forgot Password
        [Fact]
        public async void Task_4_ForgotPassword_With_Exist_Email_Return_Ok()
        {
            //Arrange
            string memberEmail = AuthenticateInputs.existingMemberEmail;

            //Act
            var response = await ForgotPassword(memberEmail);

            //Assert
            response.Should().Be(true);
        }

        [Fact]
        public async void Task_4_ForgotPassword_With_Fake_Email_Return_NotFound()
        {
            //Arrange
            string fakeEmail = AuthenticateInputs.fakeMemberEmail;

            //Act
            var response = await ForgotPassword(fakeEmail);

            //Assert
            response.Should().Be(false);
        }
        #endregion
    }
}
