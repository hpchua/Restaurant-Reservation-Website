using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthenticateController : Controller
    {
        private readonly ILogger<AuthenticateController> logger;
        private readonly IConfiguration configuration;
        private readonly IRefreshTokenService refreshTokenService;
        private readonly IUserService userService;
        private readonly IPublisher publisher;

        public AuthenticateController(ILogger<AuthenticateController> logger,
                                      IConfiguration configuration,
                                      IRefreshTokenService refreshTokenService,
                                      IUserService userService,
                                      IPublisher publisher)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.refreshTokenService = refreshTokenService;
            this.userService = userService;
            this.publisher = publisher;
        }

        #region Login - HttpPost
        /// <summary>
        /// Verify the combination of username and password to grant user access to access Restaurant Reservation System.
        /// </summary>
        /// <param name="loginInput">
        /// Provides the necessary username and password combination for authentication
        /// </param>
        /// <returns>
        /// An authentication result object that contains a status code. If authentication is
        /// successful, a user is included for creation of claims. Otherwise, an error message
        /// is included to indicate the failure reason.
        /// </returns>
        /// <response code="200">Returns the user account successfully login message</response>
        /// <response code="500">Error Retrieving user info from database</response>   
        [HttpPost(nameof(Login))]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(Login))]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<AuthenticationResult> Login(LoginVM loginInput)
        {
            var user = await userService.GetUserByUsername(loginInput.Username);
            int statusCode = SD.StatusCode.OK;
            var messages = new List<string>();
            var token = String.Empty;
            RefreshToken refreshToken = new RefreshToken();

            if (user != null)
            {
                bool isPasswordCorrect = await userService.CheckPassword(user, loginInput.Password);

                if (isPasswordCorrect)
                {
                    var userRole = await userService.GetRoleNameByUserID(user.Id);
                    user.Role = userRole;
                    token = await GenerateJwtToken(user);

                    refreshToken = GenerateRefreshToken(user.Id);
                    await refreshTokenService.AddRefreshToken(refreshToken);
                    logger.LogInformation($"User refresh token has been created {SD.HTTPLogging.OK}");
                }
                else
                {
                    logger.LogWarning($"User {user.Id} provided the wrong password {SD.HTTPLogging.UNAUTHORIZED}");
                    user = null;
                    statusCode = SD.StatusCode.UNAUTHORIZED;
                    messages.Add("Invalid credentials.");
                }
            }
            else
            {
                logger.LogWarning($"Attempt to login with {loginInput.Username} which doesn't exist {SD.HTTPLogging.NOT_FOUND}");
                user = null;
                statusCode = SD.StatusCode.NOT_FOUND;
                messages.Add("User not found.");
            }

            return new AuthenticationResult
            {
                ApplicationUser = user,
                StatusCode = statusCode,
                Message = messages,
                Token = token,
                RefreshToken = refreshToken.Token,
            };
        }
        #endregion

        #region Refresh Token - HttpPost
        /// <summary>
        /// Refresh User JWT Access Token When accessing the resources everytime.
        /// </summary>
        /// <param name="refreshRequest">
        /// Provides the user's current JWT access token and refresh token to compare database one
        /// </param>
        /// <returns>
        /// An authentication result object that will contains a status code. If authentication is
        /// successful, a user is included for creation of claims. Otherwise, an error message
        /// is included to indicate the failure reason.
        /// </returns>
        /// <response code="200">Returns the user latest JWT access token</response>
        /// <response code="500">Error Retrieving user refresh token from database</response>  
        [HttpPost(nameof(RefreshToken))]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(RefreshToken))]
        [ProducesResponseType(typeof(UserToken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<UserToken> RefreshToken(RefreshRequest refreshRequest)
        {
            var user = GetUserByAccessToken(refreshRequest.AccessToken).Result;

            if (user != null && ValidateRefreshToken(user, refreshRequest.RefreshToken))
            {
                var userRole = await userService.GetRoleNameByUserID(user.Id);
                user.Role = userRole;

                var token = await GenerateJwtToken(user);
                UserToken userToken = new UserToken(user, token);

                logger.LogInformation($"User JWT Current access token has been refreshed {SD.HTTPLogging.OK}");
                return userToken;
            }

            logger.LogWarning($"Failed to refresh user JWT Current access token due to invalid access token given {SD.HTTPLogging.BAD_REQUEST}");
            return null;
        }
        #endregion

        private bool ValidateRefreshToken(ApplicationUser user, string refreshToken)
        {
            RefreshToken userRefreshToken = refreshTokenService.Get(refreshToken);

            if (userRefreshToken != null && userRefreshToken.UserID == user.Id && userRefreshToken.ExpiryDate > DateTime.UtcNow)
                return true;

            return false;
        }

        private async Task<ApplicationUser> GetUserByAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["JwtConfig:Secret"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidAudience = configuration["JWTConfig:ValidAudience"],
                ValidIssuer = configuration["JWTConfig:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };

            SecurityToken securityToken;
            var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken != null & jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var userID = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                logger.LogInformation($"User found from the valid access token given {SD.HTTPLogging.OK}");
                return await userService.GetUserByID(userID);
            }

            logger.LogWarning($"No user found from the access token given {SD.HTTPLogging.BAD_REQUEST}");
            return null;
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await userService.GetUserRole(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                user.Role = userRole;
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtConfig:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JwtConfig:ValidIssuer"],
                audience: configuration["JwtConfig:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string userID)
        {
            RefreshToken refreshToken = new RefreshToken();

            refreshToken.UserID = userID;

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddDays(1);

            return refreshToken;
        }

        #region Register - HttpPost
        /// <summary>
        /// Take user registration info input to register a new account
        /// </summary>
        /// <param name="registerInput">
        /// Provide necessary information for registering a new user in the database
        /// </param>
        /// <returns>
        /// Similarly return an authentication result object that contains a status 
        /// code to indicate success or failure.
        /// </returns>
        /// <response code="200">Returns the user account successfully created message</response>
        /// <response code="500">Error Adding user registration info into database</response>   
        [HttpPost(nameof(Register))]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(Register))]
        [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthenticationResult>> Register(RegisterVM registerInput)
        {
            try
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = registerInput.Username,
                    Email = registerInput.Email,
                    Name = registerInput.Name,
                    PhoneNumber = registerInput.PhoneNumber,
                    Role = registerInput.Role,
                    JoinedDate = DateTime.Now,
                };

                var result = await userService.Add(user, registerInput.Password);
                if (!result.Succeeded)
                {
                    logger.LogWarning($"User {user.UserName} Failed to create {SD.HTTPLogging.BAD_REQUEST}");
                    return new AuthenticationResult
                    {
                        StatusCode = SD.StatusCode.BAD_REQUEST,
                        Message = new List<string>(result.Errors.Select(e => e.Description))
                    };
                }
                else
                {
                    await userService.AddUserRole(user, user.Role);

                    logger.LogInformation($"User {user.UserName} has been created with role as {user.Role} {SD.HTTPLogging.OK}");

                    return new AuthenticationResult
                    {
                        StatusCode = SD.StatusCode.OK,
                        Message = null,
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Register failed with register inputs {nameof(registerInput)} with exception {SD.HTTPLogging.INTERNAL}");
            }

            var errorMessage = new List<string>
            {
                "Registration cannot be processed. Please try again later or contact Staff for further information."
            };

            return new AuthenticationResult
            {
                StatusCode = Convert.ToInt32(HttpStatusCode.InternalServerError),
                Message = errorMessage
            };
        }
        #endregion

        #region ForgotPassword - HttpGet
        /// <summary>
        /// Accepts an email input from user to send email with URL link for change password purposes.
        /// </summary>
        /// <param name="email"> Email Address </param>
        /// <returns>
        /// A Boolean response that contains true or false. 
        /// If the result if true, a reset password email with URL will be sent to the user.
        /// Otherwise, sending email process unable to be conducted.
        /// </returns>
        /// <response code="200">A Boolean response</response>
        /// <response code="500">Error sending email wiht URL to the user</response>   
        [HttpGet("ForgotPassword/{email}")]
        [SwaggerOperation(OperationId = nameof(ForgotPassword))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<Boolean> ForgotPassword(string email)
        {
            ApplicationUser userToFound = null;

            try
            {
                userToFound = await userService.GetUserByEmail(email);

                if (userToFound != null)
                {
                    var token = await userService.GenerateResetPasswordToken(userToFound);
                    var encodedToken = Encoding.UTF8.GetBytes(token);
                    var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                    ForgotPasswordMessage forgotPasswordMessage = new ForgotPasswordMessage
                    {
                        Email = email,
                        Title = "Forgot Password",
                        Token = validToken,
                    };

                    publisher.Publish(
                        message: JsonConvert.SerializeObject(forgotPasswordMessage),
                        routingKey: "email.ForgotPassword",
                        messageAttributes: null
                    );

                    logger.LogInformation($"User's Email {nameof(email)} have been sent {SD.HTTPLogging.OK}");
                    return true;
                }

                logger.LogWarning($"User's Email {email} can't found {SD.HTTPLogging.NOT_FOUND}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error send email to the User {userToFound.UserName} with {nameof(email)} {SD.HTTPLogging.INTERNAL}");
            }
            return false;
        }
        #endregion

        #region ResetPassword - HttpPost
        /// <summary>
        /// A combination of Reset Password Token + User's Email Address and Updated Password
        /// </summary>
        /// <param name="input"> ResetPasswordVM </param>
        /// <returns>
        /// A Boolean response that contains true or false. 
        /// If the result if true, the user's account password has been reset successfully.
        /// Otherwise, reset password process unable to be conducted due to invalid input data.
        /// </returns>
        /// <response code="200">A Boolean response</response>
        /// <response code="500">Error updating user's account password</response>   
        [HttpPost(nameof(ResetPassword))]
        [SwaggerOperation(OperationId = nameof(ResetPassword))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<Boolean> ResetPassword(ResetPasswordVM input)
        {
            ApplicationUser userToFound = null;

            try
            {
                userToFound = await userService.GetUserByEmail(input.Email);

                if (userToFound != null)
                {
                    var decodedToken = WebEncoders.Base64UrlDecode(input.Token);
                    var validToken = Encoding.UTF8.GetString(decodedToken);

                    var result = await userService.ResetPassword(userToFound, validToken, input.Password);
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"User's Password with Email {input.Email} have been reset successfully {SD.HTTPLogging.OK}");
                        return true;
                    }

                    logger.LogWarning($"Invalid token to reset password for the {input.Email} {SD.HTTPLogging.BAD_REQUEST}");
                }
                else
                    logger.LogWarning($"User's Email {input.Email} can't found {SD.HTTPLogging.NOT_FOUND}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error reset password for {userToFound.UserName} with {nameof(input.Email)} {SD.HTTPLogging.INTERNAL}");
            }
            return false;
        }
        #endregion
    }
}
