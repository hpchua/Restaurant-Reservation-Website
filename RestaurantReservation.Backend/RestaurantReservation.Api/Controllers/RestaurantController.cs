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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RestaurantReservationSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class RestaurantController : Controller
    {
        private readonly ILogger<RestaurantController> logger;
        private readonly IRestaurantService restaurantService;
        private readonly IRestaurantCategoryService restaurantCategoryService;
        private readonly IRestaurantScheduleService restaurantScheduleService;
        private readonly IPromotionService promotionService;

        public RestaurantController(ILogger<RestaurantController> logger,
                                    IRestaurantService restaurantService,
                                    IRestaurantCategoryService restaurantCategoryService,
                                    IRestaurantScheduleService restaurantScheduleService,
                                    IPromotionService promotionService)
        {
            this.logger = logger;
            this.restaurantService = restaurantService;
            this.restaurantCategoryService = restaurantCategoryService;
            this.restaurantScheduleService = restaurantScheduleService;
            this.promotionService = promotionService;
        }

        #region GetRestaurantCount - GET
        /// <summary>
        /// Retrive the total number of restaurant in database. 
        /// </summary>
        /// <returns>
        /// Number of restaurant will be returned.       
        /// </returns>
        /// <response code="200">Number of restaurant record</response>
        /// <response code="500">Error Retrieving number of restaurant record</response>   
        [HttpGet("Count")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(GetRestaurantCount))]
        [ProducesResponseType(typeof(Int32), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Int32>> GetRestaurantCount()
        {
            return await restaurantService.GetCount();
        }
        #endregion

        #region GetAll - HttpGet
        /// <summary>
        /// Get all existing restaurants from the database.
        /// </summary>
        /// <returns>
        /// A list of restaurant with category results will be returned.
        /// </returns>
        /// <response code="200">A list of restaurant with category</response>
        /// <response code="500">Error Retrieving restaurants info from database</response>  
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(GetAll))]
        [ProducesResponseType(typeof(IEnumerable<RestaurantCategoryVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<RestaurantCategoryVM>> GetAll()
        {
            List<Restaurant> restaurants = await restaurantService.GetAll();
            List<RestaurantCategory> restaurantCategories = await restaurantCategoryService.GetAll();

            List<RestaurantCategoryVM> restaurantList = new List<RestaurantCategoryVM>();
            foreach (var restaurant in restaurants)
            {
                RestaurantCategoryVM restaurantVM = new RestaurantCategoryVM
                {
                    Restaurant = restaurant,
                    CategoryIds = restaurantCategories.Where(rc => rc.RestaurantID == restaurant.RestaurantID).Select(rc => rc.CategoryID).ToList(),
                    Categories = new List<string>()
                };

                for (int i = 0; i < restaurantVM.CategoryIds.Count(); ++i)
                {
                    restaurantVM.Categories.Add(restaurantCategories.First(x => x.CategoryID == restaurantVM.CategoryIds[i]).Category.Name);
                }
                restaurantList.Add(restaurantVM);
            }
            return restaurantList;
        }
        #endregion

        #region Get - HttpGet
        /// <summary>
        /// Retrieve a specific restaurant with the categories, schedules and promotion information based on the Restaurant ID given.
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <param name="ScheduleStatus">Schedule Status Filter eg: All, Full, Available</param>
        /// <param name="PromotionStatus">Promotion Filter eg: All, Available, Unavailable</param>
        /// <returns>
        /// A restaurant with categories, schedules and promotion information will be returned
        /// </returns>
        /// <response code="200">A restaurant with categories, schedules and promotions</response>
        /// <response code="500">Error Retrieving a restaurant info from database</response>   
        [HttpGet("{id}/{ScheduleStatus}/{PromotionStatus}")]
        [SwaggerOperation(OperationId = nameof(Get))]
        [ProducesResponseType(typeof(RestaurantCategorySchedulePromtionVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantCategorySchedulePromtionVM>> Get(long id, string ScheduleStatus, string PromotionStatus)
        {
            var restaurant = await restaurantService.GetRestaurantByID(id);

            if (restaurant == null)
            {
                logger.LogWarning($"Restaurant of ID {id} not found {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            List<RestaurantCategory> restaurantCategories = await restaurantCategoryService.GetRestaurantCategory(restaurant.RestaurantID);
            List<RestaurantSchedule> restaurantSchedules = await restaurantScheduleService.GetRestaurantSchedules(restaurant.RestaurantID);
            List<RestaurantSchedule> memberRestaurantSchedules = await restaurantScheduleService.GetMemberRestaurantSchedules(restaurant.RestaurantID);
            List<Promotion> promotions = await promotionService.GetRestaurantPromotion(restaurant.RestaurantID);

            restaurantSchedules = await FilterScheduleList(restaurantSchedules, ScheduleStatus, restaurant.RestaurantID);
            memberRestaurantSchedules = FilterMemberScheduleList(memberRestaurantSchedules, ScheduleStatus);
            promotions = FilterPromotionList(promotions, PromotionStatus);

            RestaurantCategorySchedulePromtionVM productVM = new RestaurantCategorySchedulePromtionVM
            {
                Restaurant = restaurant,
                CategoryIds = restaurantCategories.Select(rc => rc.CategoryID).ToList(),
                Categories = new List<string>(),
                RestaurantSchedules = restaurantSchedules,
                MemberRestaurantSchedules = memberRestaurantSchedules,
                Promotions = promotions,
            };

            for (int i = 0; i < productVM.CategoryIds.Count(); ++i)
            {
                productVM.Categories.Add(restaurantCategories[i].Category.Name);
            }
            return productVM;
        }

        private List<RestaurantSchedule> FilterMemberScheduleList(List<RestaurantSchedule> memberRestaurantSchedules, string ScheduleStatus)
        {
            switch (ScheduleStatus)
            {
                case "Available":
                    memberRestaurantSchedules = memberRestaurantSchedules.Where(rs => rs.Status == (int)SD.ScheduleStatus.Available).ToList();
                    break;
                case "Full":
                    memberRestaurantSchedules = memberRestaurantSchedules.Where(rs => rs.Status == (int)SD.ScheduleStatus.Full).ToList();
                    break;
                case "Unavailable":
                    memberRestaurantSchedules = memberRestaurantSchedules.Where(rs => rs.Status == (int)SD.ScheduleStatus.Unavailable).ToList();
                    break;
            }
            return memberRestaurantSchedules;
        }

        private List<Promotion> FilterPromotionList(List<Promotion> promotions, string PromotionStatus)
        {
            switch (PromotionStatus)
            {
                case "Available":
                    promotions = promotions.Where(rs => rs.isAvailable == true).ToList();
                    break;
                case "Unavailable":
                    promotions = promotions.Where(rs => rs.isAvailable == false).ToList();
                    break;
            }
            return promotions;
        }

        private async Task<List<RestaurantSchedule>> FilterScheduleList(List<RestaurantSchedule> restaurantSchedules, string ScheduleStatus, long RestaurantID)
        {
            restaurantSchedules = await restaurantScheduleService.GetRestaurantSchedules(RestaurantID);

            switch (ScheduleStatus)
            {
                case "Available":
                    restaurantSchedules = restaurantSchedules.Where(rs => rs.Status == (int)SD.ScheduleStatus.Available).ToList();
                    break;
                case "Full":
                    restaurantSchedules = restaurantSchedules.Where(rs => rs.Status == (int)SD.ScheduleStatus.Full).ToList();
                    break;
                case "Unavailable":
                    restaurantSchedules = restaurantSchedules.Where(rs => rs.Status == (int)SD.ScheduleStatus.Unavailable).ToList();
                    break;
            }
            return restaurantSchedules;
        }
        #endregion

        #region Get - HttpGet
        /// <summary>
        /// Retrieve a specific restaurant with the categories and schedules information based on the Restaurant ID given.
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <returns>
        /// A restaurant with categories selected information will be returned
        /// </returns>
        /// <response code="200">A restaurant with categories</response>
        /// <response code="500">Error Retrieving a restaurant info from database</response>   
        [HttpGet("EditInfo/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Get))]
        [ProducesResponseType(typeof(RestaurantCategorySchedulePromtionVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantCategoryVM>> GetEditInfoByID(long id)
        {
            var restaurant = await restaurantService.GetRestaurantByID(id);

            if (restaurant == null)
            {
                logger.LogWarning($"Restaurant of ID {id} not found {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            List<RestaurantCategory> restaurantCategories = await restaurantCategoryService.GetRestaurantCategory(restaurant.RestaurantID);

            RestaurantCategoryVM productVM = new RestaurantCategoryVM
            {
                Restaurant = restaurant,
                CategoryIds = restaurantCategories.Select(rc => rc.CategoryID).ToList(),
                Categories = new List<string>(),
            };

            for (int i = 0; i < productVM.CategoryIds.Count(); ++i)
            {
                productVM.Categories.Add(restaurantCategories[i].Category.Name);
            }
            return productVM;
        }
        #endregion

        #region Add - HttpPost
        /// <summary>
        /// Add a new restaurant (under which category) to the database. This checks for duplicate category name.
        /// </summary>
        /// <param name="restaurantCategoryVM">RestaurantCategoryVM object</param>
        /// <returns>
        /// A Boolean response that contains true or false. 
        /// If the result if true, a new category object will be added to the database.
        /// Otherwise, adding new category process unable to be conducted.
        /// </returns>
        /// <response code="200">A Boolean response</response>
        /// <response code="500">Error Adding a category object information into database</response>   
        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Add))]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<Boolean> Add(RestaurantCategoryVM restaurantCategoryVM)
        {
            var existingRestaurant = await restaurantService.GetRestaurantByName(restaurantCategoryVM.Restaurant.Name);

            try
            {
                if (existingRestaurant == null)
                {
                    await restaurantService.Add(restaurantCategoryVM.Restaurant);

                    foreach (var categoryId in restaurantCategoryVM.CategoryIds)
                    {
                        await restaurantCategoryService.Add(new RestaurantCategory
                        {
                            RestaurantID = restaurantCategoryVM.Restaurant.RestaurantID,
                            CategoryID = categoryId
                        });
                    }
                    logger.LogInformation($"Restaurant {restaurantCategoryVM.Restaurant.Name} has been created with category successfully {SD.HTTPLogging.OK}");
                    return true;
                }
                else
                {
                    logger.LogWarning($"Attempt to add duplicate restaurant name {restaurantCategoryVM.Restaurant.Name} {SD.HTTPLogging.BAD_REQUEST}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while adding new restaurant {restaurantCategoryVM} to database {SD.HTTPLogging.INTERNAL}");
                return false;
            }
        }
        #endregion

        #region Update - HttpPut
        /// <summary>
        /// Update an existing restaurant in the database. This checks for duplicate restaurant name.
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <param name="restaurantCategoryVM">RestaurantCategoryVM Object</param>
        /// <returns>
        /// An updated Restaurant will be returned. 
        /// Otherwise, here are the following why the updating existing restaurant info process unable to be conducted :
        /// If the restaurant name change to existing data and
        /// there is another conflict happens when 2 same processes conducting simultaneously.
        /// </returns>
        /// <response code="200">Updated Restaurant object</response>
        /// <response code="400">Invalid Restaurant ID or Restaurant name exists message</response>
        /// <response code="409">Category Version Conflict message</response>
        /// <response code="500">Error Updating a Restaurant object information from database</response>   
        [HttpPut("{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Update))]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestaurantCategoryVM>> Update(long id, RestaurantCategoryVM restaurantCategoryVM)
        {
            if (id != restaurantCategoryVM.Restaurant.RestaurantID)
            {
                logger.LogWarning($"ID Requested {id} is not matched with Category of ID {restaurantCategoryVM.Restaurant.RestaurantID} {SD.HTTPLogging.BAD_REQUEST}");
                return BadRequest();
            }

            var oldRestaurantValues = await restaurantService.GetRestaurantByID(restaurantCategoryVM.Restaurant.RestaurantID);
            try
            {
                if (Convert.ToBase64String(oldRestaurantValues.RowVersion).Equals(restaurantCategoryVM.Restaurant.VersionNo))
                {
                    var existingRestaurant = await restaurantService.GetRestaurantByName(restaurantCategoryVM.Restaurant.Name);
                    if (existingRestaurant != null && (existingRestaurant.RestaurantID != restaurantCategoryVM.Restaurant.RestaurantID))
                    {
                        logger.LogWarning($"Attempt to update restaurant {nameof(restaurantCategoryVM.Restaurant)} failed due to duplicate name {nameof(restaurantCategoryVM.Restaurant.Name)} {SD.HTTPLogging.BAD_REQUEST}");
                        return BadRequest("Duplicate Restaurant Name input");
                    }
                    restaurantCategoryVM.Restaurant.ImageUrl = (restaurantCategoryVM.Restaurant.ImageUrl == null) ? oldRestaurantValues.ImageUrl : restaurantCategoryVM.Restaurant.ImageUrl;
                    await restaurantService.Update(restaurantCategoryVM.Restaurant);

                    var currentRestaurantCategoryMapping = await restaurantCategoryService.GetCategories(restaurantCategoryVM.Restaurant.RestaurantID);
                    await restaurantCategoryService.Delete(currentRestaurantCategoryMapping);

                    foreach (var categoryId in restaurantCategoryVM.CategoryIds)
                    {
                        await restaurantCategoryService.Add(new RestaurantCategory
                        {
                            RestaurantID = restaurantCategoryVM.Restaurant.RestaurantID,
                            CategoryID = categoryId
                        });
                    }
                    logger.LogInformation($"Restaurant {restaurantCategoryVM.Restaurant.Name} with ID {restaurantCategoryVM.Restaurant.RestaurantID} Data has been updated {SD.HTTPLogging.OK}");
                    return restaurantCategoryVM;
                }
                else
                {
                    logger.LogWarning($"Restaurant {restaurantCategoryVM.Restaurant.Name} with ID {restaurantCategoryVM.Restaurant.RestaurantID} Data have some conflicts {SD.HTTPLogging.CONFLICT}");
                    return Conflict();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while updating existing restaurant {restaurantCategoryVM.Restaurant} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing restaurant information from database.
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <param name="userID">User ID</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, deactivating Restaurant process unable to be conducted due to Restaurant ID not found.
        /// </returns>
        /// <response code="204">Updating Restaurant Status Result</response>
        /// <response code="404">Restaurant record not found</response>
        /// <response code="500">Error Deleting a Restaurant object information from database</response>   
        [HttpDelete("{userId}/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id, string userID)
        {
            var restaurantToDelete = await restaurantService.GetRestaurantByID(id);
            if (restaurantToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing restaurant of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                restaurantToDelete.IsAvailable = !restaurantToDelete.IsAvailable;   // change to inactive instead of deleting it
                await restaurantService.Update(restaurantToDelete);
                logger.LogInformation($"Restaurant {restaurantToDelete.Name} with ID {restaurantToDelete.RestaurantID} Data has been set to inactive {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting {restaurantToDelete} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing restaurant information from database, for testing
        /// </summary>
        /// <param name="id">Restaurant ID</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, deleting Restaurant process unable to be conducted due to Restaurant ID not found.
        /// </returns>
        /// <response code="204">Deleting Restaurant Success Result</response>
        /// <response code="404">Restaurant record not found</response>
        /// <response code="500">Error Deleting a Restaurant object information from database</response>   
        [HttpDelete("Testing/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var restaurantToDelete = await restaurantService.GetRestaurantByID(id);
            if (restaurantToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing restaurant of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await restaurantService.Delete(restaurantToDelete);
                logger.LogInformation($"Restaurant {restaurantToDelete.Name} with ID {restaurantToDelete.RestaurantID} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting {restaurantToDelete} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion
    }
}
