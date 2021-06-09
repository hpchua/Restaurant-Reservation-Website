using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RestaurantReservationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ScheduleController : Controller
    {
        private readonly ILogger<RestaurantController> logger;
        private readonly IRestaurantService restaurantService;
        private readonly IRestaurantScheduleService restaurantScheduleService;

        public ScheduleController(ILogger<RestaurantController> logger,
                                   IRestaurantService restaurantService,
                                   IRestaurantScheduleService restaurantScheduleService)
        {
            this.logger = logger;
            this.restaurantService = restaurantService;
            this.restaurantScheduleService = restaurantScheduleService;
        }

        #region GetCount - GET
        /// <summary>
        /// Retrive the total number of schedule of a restaurant in database. 
        /// </summary>
        /// <param name="restaurantID">Restaurant ID</param>
        /// <returns>
        /// Number of schedule of a restaurant will be returned.       
        /// </returns>
        /// <response code="200">Number of schedule of a restaurant record</response>
        /// <response code="500">Error Retrieving number of schedule of a restaurant record</response>   

        [HttpGet("Count/{restaurantID}")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(GetCount))]
        [ProducesResponseType(typeof(Int32), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Int32>> GetCount(long restaurantID)
        {
            return await restaurantScheduleService.GetCount(restaurantID);
        }
        #endregion

        #region GetAll - HttpGet
        /// <summary>
        /// Get all existing restaurant schedules from the database for populating drop down list purpose based on Restaurant ID given.
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <returns>
        /// A list of restaurant schedules results will be returned.
        /// </returns>
        /// <response code="200">A list of restaurant schedules</response>
        /// <response code="500">Error Retrieving restaurant schedules from database</response>  
        [HttpGet("All/{id}")]
        [SwaggerOperation(OperationId = nameof(GetAll))]
        [ProducesResponseType(typeof(List<RestaurantSchedule>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<List<RestaurantSchedule>> GetAll(long id)
        {
            var schedules = await restaurantScheduleService.GetAvailableRestaurantSchedules(id);
            logger.LogInformation($"Restaurant schedules have been retrieved {HttpStatusCode.OK}");
            return schedules;
        }
        #endregion

        #region GetByID - HttpGet
        /// <summary>
        /// Retrieve a specific restaurant schedule information based on the Schedule ID given.
        /// </summary>
        /// <param name="id">Schedule ID</param>
        /// <returns>
        /// A restaurant schedule information will be returned
        /// </returns>
        /// <response code="200">A restaurant schedule object</response>
        /// <response code="500">Error Retrieving a restaurant schedule info from database</response>   
        [HttpGet("{id}")]
        [SwaggerOperation(OperationId = nameof(Get))]
        [ProducesResponseType(typeof(RestaurantCategorySchedulePromtionVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantScheduleVM>> Get(long id)
        {
            RestaurantSchedule scheduleToFound = await restaurantScheduleService.GetScheduleByID(id);

            if (scheduleToFound == null)
            {
                logger.LogWarning($"Schedule of ID {id} not found {HttpStatusCode.NotFound}");
                return NotFound();
            }

            Restaurant restaurantToFound = await restaurantService.GetRestaurantByID(scheduleToFound.RestaurantID);
            RestaurantScheduleVM restaurantSchedule = RestaurantViewModel.FillScheduleDetails(scheduleToFound, restaurantToFound);

            logger.LogInformation($"Schedule of ID {id} and relevant restaurant info have been retrieved {HttpStatusCode.OK}");
            return restaurantSchedule;
        }
        #endregion

        #region Add - HttpPost
        /// <summary>
        /// Add a new schedule for a restaurant to the database. This also check for duplicate session start time.
        /// </summary>
        /// <param name="restaurantSchedule">restaurantSchedule object</param>
        /// <returns>
        /// No content response which means that a new schedule object has been added to the database.
        /// Otherwise, adding new schedule for a restaurant process unable to be conducted.
        /// </returns>
        /// <response code="204">No content</response>
        /// <response code="500">Error Adding a schedule object information into database</response>   
        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Add))]
        [ProducesResponseType(typeof(RestaurantSchedule), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantSchedule>> Add(RestaurantSchedule restaurantSchedule)
        {
            try
            {
                var result = await restaurantScheduleService.CheckExistingStartTime(0, restaurantSchedule.ScheduleDate, restaurantSchedule.StartTime, restaurantSchedule.RestaurantID, "add");

                if (result)
                {
                    logger.LogWarning($"Attempt to add the schedule {nameof(restaurantSchedule)} failed due to duplicate start time {nameof(restaurantSchedule.StartTime)} {SD.HTTPLogging.BAD_REQUEST}");
                    return BadRequest("Duplicate Start time");
                }

                await restaurantScheduleService.Add(restaurantSchedule);
                return restaurantSchedule;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while adding new shedule {restaurantSchedule} to database");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region Update - HttpPut
        /// <summary>
        /// Update an existing restaurant schedule info in the database. This checks for duplicate start time also.
        /// </summary>
        /// <param name="id">Schedule ID</param>
        /// <param name="restaurantSchedule">RestaurantCategoryVM Object</param>
        /// <returns>
        /// An updated schedule info will be returned. 
        /// Otherwise, here are the following why the updating existing schedule info process unable to be conducted :
        /// If the schedule name change to existing data and
        /// there is another conflict happens when 2 same processes conducting simultaneously.
        /// </returns>
        /// <response code="200">Updated Schedule object</response>
        /// <response code="400">Invalid Schedule ID or Schedule Start Time exists message</response>
        /// <response code="409">Category Version Conflict message</response>
        /// <response code="500">Error Updating a Schedule object information from database</response>   
        [HttpPut("{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Update))]
        [ProducesResponseType(typeof(RestaurantSchedule), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantSchedule>> Update(long id, RestaurantSchedule restaurantSchedule)
        {
            if (id != restaurantSchedule.ScheduleID)
            {
                logger.LogWarning($"ID Requested {id} is not matched with Schedule of ID {restaurantSchedule.ScheduleID} {SD.HTTPLogging.BAD_REQUEST}");
                return BadRequest();
            }

            var oldScheduleValues = await restaurantScheduleService.GetScheduleByID(restaurantSchedule.ScheduleID);
            try
            {
                if (Convert.ToBase64String(oldScheduleValues.RowVersion).Equals(restaurantSchedule.VersionNo))
                {
                    var result = await restaurantScheduleService.CheckExistingStartTime(id, restaurantSchedule.ScheduleDate, restaurantSchedule.StartTime, restaurantSchedule.RestaurantID, "update");
                    if (result)
                    {
                        logger.LogWarning($"Attempt to update schedule {nameof(restaurantSchedule)} failed due to duplicate start time {nameof(restaurantSchedule.StartTime)} {SD.HTTPLogging.BAD_REQUEST}");
                        return BadRequest("Duplicate Start time");
                    }
                    await restaurantScheduleService.Update(restaurantSchedule);

                    logger.LogInformation($"Schedule {restaurantSchedule} with ID {restaurantSchedule.ScheduleID} Data has been updated {SD.HTTPLogging.OK}");
                    return restaurantSchedule;
                }
                else
                {
                    logger.LogWarning($"Schedule with ID {restaurantSchedule.ScheduleID} Data have some conflicts {SD.HTTPLogging.CONFLICT}");
                    return Conflict();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while updating existing schedule {restaurantSchedule} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing schedule information from database.
        /// </summary>
        /// <param name="id">Schedule ID</param>
        /// <param name="userID">User ID</param>
        /// <param name="status">Status Value Eg: Avaialble(1), Unavailable(4)</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, delete schedule info process unable to be conducted due to other members have been placed reservation on them.
        /// </returns>
        /// <response code="204">Updating Restaurant Status Result</response>
        /// <response code="404">Restaurant record not found</response>
        /// <response code="500">Error Deleting a Schedule object information from database</response>   
        [HttpDelete("{userId}/{status}/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id, string userID, int status)
        {
            var scheduleToDelete = await restaurantScheduleService.GetScheduleByID(id);
            if (scheduleToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing restaurant schedule of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                scheduleToDelete.Status = status;   // change to inactive instead of deleting it
                await restaurantScheduleService.Update(scheduleToDelete);
                logger.LogInformation($"Schedule with ID {id} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting Schedule with ID {id} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing restaurant schedule information from database, for testing
        /// </summary>
        /// <param name="id">Schedule ID</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, deleting Restaurant schedule process unable to be conducted due to schedule ID not found.
        /// </returns>
        /// <response code="204">Updating Restaurant schedule successfully Result</response>
        /// <response code="404">Restaurant schedule record not found</response>
        /// <response code="500">Error Deleting a Restaurant schedule object information from database</response>   
        [HttpDelete("Testing/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var scheduleToDelete = await restaurantScheduleService.GetScheduleByID(id);
            if (scheduleToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing restaurant schedule of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await restaurantScheduleService.Delete(scheduleToDelete);
                logger.LogInformation($"Schedule with ID {id} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting Schedule with ID {id} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion
    }
}
