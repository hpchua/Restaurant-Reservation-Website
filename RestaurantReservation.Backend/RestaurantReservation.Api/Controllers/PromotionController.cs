using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RestaurantReservationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PromotionController : Controller
    {
        private readonly ILogger<PromotionController> logger;
        private readonly IRestaurantService restaurantService;
        private readonly IPromotionService promotionService;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;
        private readonly IPublisher publisher;

        public PromotionController(ILogger<PromotionController> logger,
                                   IRestaurantService restaurantService,
                                   IPromotionService promotionService,
                                   IUserService userService,
                                   IConfiguration configuration,
                                   IPublisher publisher)
        {
            this.logger = logger;
            this.restaurantService = restaurantService;
            this.promotionService = promotionService;
            this.userService = userService;
            this.configuration = configuration;
            this.publisher = publisher;
        }

        #region GetByID - HttpGet
        /// <summary>
        /// Retrieve a specific restaurant Promotion information based on the Promotion ID given.
        /// </summary>
        /// <param name="id">Promotion ID</param>
        /// <returns>
        /// A restaurant Promotion information will be returned
        /// </returns>
        /// <response code="200">A restaurant Promotion object</response>
        /// <response code="500">Error Retrieving a restaurant Promotion info from database</response>   
        [HttpGet("{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Get))]
        [ProducesResponseType(typeof(RestaurantPromotionVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantPromotionVM>> Get(long id)
        {
            Promotion promotionToFound = await promotionService.GetPromotionByID(id);

            if (promotionToFound == null)
            {
                logger.LogWarning($"Schedule of ID {id} not found {HttpStatusCode.NotFound}");
                return NotFound();
            }

            Restaurant restaurantToFound = await restaurantService.GetRestaurantByID(promotionToFound.RestaurantID);
            RestaurantPromotionVM restaurantPromotion = RestaurantViewModel.FillPromotionDetails(promotionToFound, restaurantToFound);

            logger.LogInformation($"Schedule of ID {id} and relevant restaurant info have been retrieved {HttpStatusCode.OK}");
            return restaurantPromotion;
        }
        #endregion

        #region CheckExistingByID - HttpGet
        /// <summary>
        /// Retrieve a specific restaurant Promotion information based on the Promotion ID given.
        /// </summary>
        /// <param name="id">Promotion ID</param>
        /// <returns>
        /// A restaurant Promotion information will be returned
        /// </returns>
        /// <response code="200">A restaurant Promotion object</response>
        /// <response code="500">Error Retrieving a restaurant Promotion info from database</response>   
        [HttpGet("Check/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(CheckExistingByID))]
        [ProducesResponseType(typeof(RestaurantPromotionVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Promotion>> CheckExistingByID(long id)
        {
            Promotion promotionToFound = await promotionService.GetPromotionByID(id);

            if (promotionToFound == null)
            {
                logger.LogWarning($"Schedule of ID {id} not found {HttpStatusCode.NotFound}");
                return NotFound();
            }

            logger.LogInformation($"Schedule of ID {id} and relevant restaurant info have been retrieved {HttpStatusCode.OK}");
            return promotionToFound;
        }
        #endregion

        #region Add - HttpPost
        /// <summary>
        /// Add a new promotion for a restaurant to the database. This checks for duplicate promotions name.
        /// </summary>
        /// <param name="promotion">Promotion object</param>
        /// <returns>
        /// No content response which means that a new promotion object has been added to the database.
        /// Otherwise, adding new promotion for a restaurant process unable to be conducted.
        /// </returns>
        /// <response code="200">A Boolean response</response>
        /// <response code="500">Error Adding a promotions object information into database</response>   
        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Add))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<Boolean> Add(Promotion promotion)
        {
            var existingPromotion = await promotionService.GetPromotionByName(promotion.Name, promotion.RestaurantID);

            try
            {
                if (existingPromotion == null)
                {
                    var newPromotionInfo = await promotionService.Add(promotion);
                    logger.LogInformation($"Promotion {promotion.Name} has been created with ID {promotion.PromotionID} {SD.HTTPLogging.OK}");

                    if (!promotion.isEmailCreatedSent)
                    {
                        var newPromotion = await promotionService.GetByIDForEmail(newPromotionInfo.PromotionID);
                        NewPromotion newPromotionMessage = new NewPromotion
                        {
                            Title = "NewPromotion",
                            Promotion = newPromotion,
                        };

                        publisher.Publish(
                            message: JsonConvert.SerializeObject(newPromotionMessage),
                            routingKey: "email.newPromotion",
                            messageAttributes: null
                        );

                        newPromotion.isEmailCreatedSent = true;
                        await promotionService.Update(newPromotion);
                        logger.LogInformation($"New Promotion {promotion.Name} with ID has been sent the email and update the email status {promotion.PromotionID} {SD.HTTPLogging.OK}");
                    }

                    return true;
                }
                else
                {
                    logger.LogWarning($"Attempt to add duplicate promotion name {promotion.Name} {SD.HTTPLogging.BAD_REQUEST}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while adding new promotion {promotion} to database {SD.HTTPLogging.INTERNAL}");
                return false;
            }
        }
        #endregion

        #region Update - HttpPut
        /// <summary>
        /// Update an existing restaurant Promotion info in the database. This checks for duplicate name also.
        /// </summary>
        /// <param name="id">Promotion ID</param>
        /// <param name="promotion">Updated Promotion Info</param>
        /// <returns>
        /// An updated Promotion info will be returned. 
        /// Otherwise, here are the following why the updating existing Promotion info process unable to be conducted :
        /// If the Promotion name change to existing data and
        /// there is another conflict happens when 2 same processes conducting simultaneously.
        /// </returns>
        /// <response code="200">Updated Promotion object</response>
        /// <response code="400">Invalid Promotion ID or Promotion Name exists message</response>
        /// <response code="409">Category Version Conflict message</response>
        /// <response code="500">Error Updating a Promotion object information from database</response>   
        [HttpPut("{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Update))]
        [ProducesResponseType(typeof(Promotion), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Promotion>> Update(long id, Promotion promotion)
        {
            if (id != promotion.PromotionID)
            {
                logger.LogWarning($"ID Requested {id} is not matched with Schedule of ID {promotion.PromotionID} {SD.HTTPLogging.BAD_REQUEST}");
                return BadRequest();
            }

            var oldScheduleValues = await promotionService.GetPromotionByID(promotion.PromotionID);
            try
            {
                if (Convert.ToBase64String(oldScheduleValues.RowVersion).Equals(promotion.VersionNo))
                {
                    var result = await promotionService.CheckExistingName(promotion, id);
                    if (result)
                    {
                        logger.LogWarning($"Attempt to update promotion {nameof(promotion)} failed due to duplicate name {nameof(promotion.Name)} {SD.HTTPLogging.BAD_REQUEST}");
                        return BadRequest("Duplicate Announcement & Promotion Name");
                    }
                    await promotionService.Update(promotion);
                    logger.LogInformation($"Promotion {promotion} with ID {promotion.PromotionID} Data has been updated {SD.HTTPLogging.OK}");
                    return promotion;
                }
                else
                {
                    logger.LogWarning($"Schedule with ID {promotion.PromotionID} Data have some conflicts {SD.HTTPLogging.CONFLICT}");
                    return Conflict();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while updating existing promotion {promotion} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing promotion information from database.
        /// </summary>
        /// <param name="id">Promotion ID</param>
        /// <param name="userID">User ID</param>
        /// <returns>
        /// No response return back.
        /// </returns>
        /// <response code="204">Deleting Promotion Result</response>
        /// <response code="404">Promotion record not found</response>
        /// <response code="500">Error Deleting a Promotion object information from database</response>   
        [HttpDelete("{userId}/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id, string userID)
        {
            var promotionToDelete = await promotionService.GetPromotionByID(id);
            if (promotionToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing restaurant promotion of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await promotionService.Delete(promotionToDelete);
                logger.LogInformation($"Promotion with ID {id} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting Schedule with ID {id} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion

        #region PushEmail - HttpGet
        /// <summary>
        /// Push Email to the Member Subscriber based on Promotion ID given.
        /// </summary>
        /// <param name="id">Promotion ID</param>
        /// <returns>
        /// A Boolean response that contains true or false. 
        /// If the result if true, the promotion information will be sent to customer via Email.
        /// Otherwise, the promotion information valid date has been expired.
        /// </returns>
        /// <response code="200">A Boolean response</response>
        /// <response code="404">Promotion record not found</response>
        [HttpGet("PushEmail/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(PushEmail))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<Boolean> PushEmail(long id)
        {
            var promotionToPush = await promotionService.GetByIDForEmail(id);

            if (promotionToPush == null)
            {
                logger.LogWarning($"Attempt to send non-existing restaurant promotion of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return false;
            }

            if (!promotionToPush.isAvailable)
            {
                logger.LogWarning($"Attempt to send non-existing restaurant promotion of ID {id} with invalid date {SD.HTTPLogging.BAD_REQUEST}");
                return false;
            }

            publisher.Publish(
                message: JsonConvert.SerializeObject(promotionToPush),
                routingKey: "email.promotion",
                messageAttributes: null
            );

            if (!promotionToPush.isEmailCreatedSent)
            {
                promotionToPush.isEmailCreatedSent = true;
                await promotionService.Update(promotionToPush);
            }

            logger.LogInformation($"Promotion with ID {id} has sent the email to the member subscriber successfully {SD.HTTPLogging.OK}");
            return true;
        }
        #endregion
    }
}
