using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RestaurantReservationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> logger;
        private readonly IUserService userService;

        public UserController(ILogger<UserController> logger,
                              IUserService userService)
        {
            this.logger = logger;
            this.userService = userService;
        }

        #region GetAll - HttpGet
        /// <summary>
        /// Get all existing users info from the database based on the role given.
        /// </summary>
        /// <param name="role">User Role string, e.g. Admin and Member </param>
        /// <returns>
        /// A list of users results will be returned.
        /// </returns>
        /// <response code="200">A list of users</response>
        /// <response code="500">Error Retrieving users info from database</response>
        [HttpGet("{role}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(GetAll))]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<List<ApplicationUser>> GetAll(string role)
        {
            var users = await userService.GetAll();
            var userRoles = await userService.GetAllUserRole();
            var roles = await userService.GetAllRole();

            foreach (var user in users)
            {
                var roleID = userRoles.FirstOrDefault(record => record.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(role => role.Id == roleID).Name;
            }

            switch (role)
            {
                case SD.ROLE_ADMIN:
                    users = users.Where(user => user.Role == SD.ROLE_ADMIN).ToList();
                    break;
                case SD.ROLE_MEMBER:
                    users = users.Where(user => user.Role == SD.ROLE_MEMBER).ToList();
                    break;
            }
            logger.LogInformation($"Users have been retrieved {HttpStatusCode.OK}");
            return users;
        }
        #endregion

        #region Get - HttpGet
        /// <summary>
        /// Retrieve a specific user details based on the user ID given.
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <returns>
        /// A user details will be returned
        /// </returns>
        /// <response code="200">A specific user</response>
        /// <response code="500">Error Retrieving a user info from database</response> 
        [HttpGet("Info/{userID}")]
        [SwaggerOperation(OperationId = nameof(Get))]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationUser>> Get(string userID)
        {
            var userToFound = await userService.GetByID(userID);

            if (userToFound == null)
            {
                logger.LogWarning($"Couldn't find user with ID {userID} {HttpStatusCode.NotFound}");
                return NotFound();
            }

            var userRoles = await userService.GetAllUserRole();
            var roles = await userService.GetAllRole();

            var roleID = userRoles.FirstOrDefault(record => record.UserId == userToFound.Id).RoleId;
            userToFound.Role = roles.FirstOrDefault(role => role.Id == roleID).Name;

            logger.LogInformation($"User with ID {userID} have been retrieved {HttpStatusCode.OK}");
            return userToFound;
        }
        #endregion

        #region CheckExistingEmail - HttpGet
        /// <summary>
        /// A validation of User's Email Address Exists in the database
        /// </summary>
        /// <param name="email"> User's Email Address </param>
        /// <returns>
        /// A Boolean response that contains true or false. 
        /// If the result if true, the user's email address is existing in the database.
        /// Otherwise, the email address is free to enter.
        /// </returns>
        /// <response code="200">An Application User result</response>
        /// <response code="500">Error retrieving user's email address</response>   
        [HttpGet("Check/{email}")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(CheckExistingEmail))]
        [ProducesResponseType(typeof(ApplicationUser), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationUser>> CheckExistingEmail(string email)
        {
            try
            {
                ApplicationUser userToFound = await userService.GetUserByEmail(email);

                if (userToFound == null)
                    return NotFound();

                return userToFound;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving email address with {nameof(email)} {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region ChangePassword - HttpPost
        /// <summary>
        /// Accepts a combination of user ID, current password and new password for password change.
        /// </summary>
        /// <param name="changePasswordVM"> 
        /// ChangePasswordVM object
        /// </param>
        /// <returns>
        /// Returns boolean to indicate success or failure.
        /// </returns>
        /// <response code="200">Returns boolean object</response>
        /// <response code="500">Error Changing User's Password</response> 
        [HttpPost(nameof(ChangePassword))]
        [SwaggerOperation(OperationId = nameof(ChangePassword))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Boolean>> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            var userToFound = await userService.GetByID(changePasswordVM.UserID);

            if (userToFound == null)
                return NotFound();

            try
            {
                var result = await userService.ChangePassword(userToFound, changePasswordVM.CurrentPassword, changePasswordVM.NewPassword);
                if (result.Succeeded)
                    return true;

                logger.LogWarning($"User with {userToFound.Id} change password unsuccessfully {HttpStatusCode.BadRequest}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Password change by User {changePasswordVM.UserID} failed inputs {changePasswordVM} with exception");
            }
            return false;
        }
        #endregion

        #region Update - HttpPut
        /// <summary>
        /// Update an existing user information in the database.
        /// </summary>
        /// <param name="input">ApplicationUser Object</param>
        /// <returns>
        /// A Boolean response that contains true or false. 
        /// If the result if true, the latest user profile details will be updated successfully.
        /// Otherwise, update user profile process unable to be conducted.
        /// </returns>
        /// <response code="200">A Boolean response</response>
        /// <response code="404">Invalid user ID</response>
        /// <response code="500">Error sending email wiht URL to the user</response> 
        [HttpPut]
        [SwaggerOperation(OperationId = nameof(Update))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Boolean>> Update(ProfileVM input)
        {
            var userToFound = await userService.GetByID(input.UserID);

            if (userToFound == null)
                return NotFound();

            try
            {
                ApplicationUser userToUpdate = ProfileViewModel.FetchData(input);
                await userService.Update(userToUpdate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred when updating user {input} in database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            logger.LogInformation($"User with ID {userToFound.Id} update profile successfully {HttpStatusCode.OK}");
            return true;
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing member registered details from database.
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, deleting member record process unable to be conducted due to User ID not found.
        /// </returns>
        /// <response code="204">Deleting user information successfully</response>
        /// <response code="404">User record not found</response>
        /// <response code="500">Error Deleting a user object information from database</response>   
        [HttpDelete("{userId}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string userID)
        {
            var userToDelete = await userService.GetByID(userID);

            if (userToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing user with ID {userID} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await userService.Delete(userToDelete);
                logger.LogInformation($"User {userToDelete.UserName} with ID {userToDelete.Id} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting {userToDelete} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }
        #endregion
    }
}
