using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
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
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> logger;
        private readonly ICategoryService categoryService;

        public CategoryController(ILogger<CategoryController> logger,
                                  ICategoryService categoryService)
        {
            this.logger = logger;
            this.categoryService = categoryService;
        }

        #region GetCategoryCount - GET
        /// <summary>
        /// Retrive the total number of category in database. 
        /// </summary>
        /// <returns>
        /// Number of category will be returned.       
        /// </returns>
        /// <response code="200">Number of category record</response>
        /// <response code="500">Error Retrieving number of category record</response>   
        [HttpGet("Count")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(GetCategoryCount))]
        [ProducesResponseType(typeof(Int32), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Int32>> GetCategoryCount()
        {
            return await categoryService.GetCount();
        }
        #endregion

        #region GetAll - HttpGet
        /// <summary>
        /// Get all existing categories from the database.
        /// </summary>
        /// <returns>
        /// A list of category results will be returned.
        /// </returns>
        /// <response code="200">A list of category</response>
        /// <response code="500">Error Retrieving categories info from database</response>  
        [HttpGet]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(GetAll))]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<List<Category>> GetAll()
        {
            var categories = await categoryService.GetAll();
            logger.LogInformation($"Categories have been retrieved {HttpStatusCode.OK}");
            return categories;
        }
        #endregion

        #region Get - HttpGet
        /// <summary>
        /// Retrieve a specific category information based on the Category ID given.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>
        /// A category information will be returned
        /// </returns>
        /// <response code="200">A category</response>
        /// <response code="500">Error Retrieving a category info from database</response>   
        [HttpGet("{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Get))]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> Get(long id)
        {
            var categoryToFound = await categoryService.GetCategoryByID(id);

            if (categoryToFound == null)
            {
                logger.LogWarning($"Category of ID {id} not found {HttpStatusCode.NotFound}");
                return NotFound();
            }
            logger.LogInformation($"Category of ID {id} have been retrieved {HttpStatusCode.OK}");
            return categoryToFound;
        }
        #endregion

        #region Add - HttpPost
        /// <summary>
        /// Add a new category to the database. This checks for duplicate category name.
        /// </summary>
        /// <param name="category">Category object</param>
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
        public async Task<Boolean> Add(Category category)
        {
            var existingCategory = await categoryService.GetCategoryByName(category.Name);

            try
            {
                if (existingCategory == null)
                {
                    await categoryService.Add(category);
                    logger.LogInformation($"Category {category.Name} has been created with ID {category.CategoryID} {SD.HTTPLogging.OK}");
                    return true;
                }
                else
                {
                    logger.LogWarning($"Attempt to add duplicate category {category} {SD.HTTPLogging.BAD_REQUEST}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while adding new category {category} to database {SD.HTTPLogging.INTERNAL}");
                return false;
            }
        }
        #endregion

        #region Update - HttpPut
        /// <summary>
        /// Update an existing category in the database. This checks for duplicate category name.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="category">Category Object</param>
        /// <returns>
        /// An updated Category will be returned. 
        /// Otherwise, here are the following why the updating existing category info process unable to be conducted :
        /// If the category name change to existing data and
        /// there is another conflict happens when 2 same processes conducting simultaneously.
        /// </returns>
        /// <response code="200">Updated Category object</response>
        /// <response code="400">Invalid Category ID or Category name exists message</response>
        /// <response code="409">Category Version Conflict message</response>
        /// <response code="500">Error Updating a Category object information from database</response>   
        [HttpPut("{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Update))]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> Update(long id, Category category)
        {
            if (id != category.CategoryID)
            {
                logger.LogWarning($"ID Requested {id} is not matched with Category of ID {category.CategoryID} {SD.HTTPLogging.BAD_REQUEST}");
                return BadRequest();
            }

            var oldCategoryValues = await categoryService.GetCategoryByID(category.CategoryID);
            try
            {
                if (Convert.ToBase64String(oldCategoryValues.RowVersion).Equals(category.VersionNo))
                {
                    var existingCategory = await categoryService.GetCategoryByName(category.Name);

                    if (existingCategory != null && (existingCategory.CategoryID != category.CategoryID))
                    {
                        logger.LogWarning($"Attempt to update category {nameof(category)} failed due to duplicate category {nameof(existingCategory)} {SD.HTTPLogging.BAD_REQUEST}");
                        return BadRequest("Duplicate Category Name input");
                    }

                    await categoryService.Update(category);
                    logger.LogInformation($"Category {category.Name} with ID {category.CategoryID} Data has been updated {SD.HTTPLogging.OK}");

                    return category;
                }
                else
                {
                    logger.LogWarning($"Category {category.Name} with ID {category.CategoryID} Data have some conflicts {SD.HTTPLogging.CONFLICT}");
                    return Conflict();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while updating existing category {category} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing category information from database.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="userID">User ID</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, deleting category record process unable to be conducted due to category ID not found.
        /// </returns>
        /// <response code="204">Deleting category successfully</response>
        /// <response code="404">Category record not found</response>
        /// <response code="500">Error Deleting a category object information from database</response>   
        [HttpDelete("{userId}/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id, string userID)
        {
            var categoryToDelete = await categoryService.GetCategoryByID(id);

            if (categoryToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing category of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await categoryService.Delete(categoryToDelete);
                logger.LogInformation($"Category {categoryToDelete.Name} with ID {categoryToDelete.CategoryID} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting {categoryToDelete} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }

            return NoContent();
        }
        #endregion
    }
}
