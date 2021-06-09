using FluentAssertions;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.IntegrationTests.Configurations.Api.Services;
using RestaurantReservation.IntegrationTests.Configurations.Priority;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantReservation.IntegrationTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class CategoryControllerTest : CategoryServices
    {
        #region Get All
        [Fact]
        public async void Task_1_Get_Category_All_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            var count = await GetCount();

            //Act
            var categories = await GetAll();

            //Assert
            categories.Should().HaveCount(count);
        }
        #endregion

        #region Get By Id
        [Fact]
        public async void Task_2_Get_Category_ById_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long categoryID = CategoryInputs.chickenID;
            string categoryName = CategoryInputs.chickenCategory;

            //Act
            var categoryToFound = await GetByID(categoryID);

            //Assert
            categoryToFound.Should().NotBeNull();
            categoryToFound.CategoryID.Should().Be(categoryID);
            categoryToFound.Name.Should().Be(categoryName);
        }

        [Fact]
        public async void Task_2_Get_Category_ById_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakeCategoryID = CategoryInputs.fakeID;

            //Act
            var categoryToFound = await GetByID(fakeCategoryID);

            //Assert
            categoryToFound.Should().BeNull();
        }
        #endregion

        #region Create
        [Fact]
        public async void Task_1_Create_Category_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string categoryName = CategoryInputs.newCategoryName;

            //Act
            var isCategoryCreated = await Create(new Category { Name = categoryName });

            //Assert
            isCategoryCreated.Should().Be(true);

            var newCategory = await RetrieveCategoryByName(categoryName);
            newCategory.Name.Should().Be(categoryName);
        }

        private async Task<Category> RetrieveCategoryByName(string categoryName)
        {
            var categories = await GetAll();
            Category categoryToFound = null;

            foreach (var category in categories)
            {
                if (category.Name.ToLower().Equals(categoryName.ToLower()))
                {
                    categoryToFound = category;
                }
            }
            return categoryToFound;
        }

        [Fact]
        public async void Task_3_Create_Category_With_Duplicate_Name_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string categoryName = CategoryInputs.newCategoryName;

            //Act
            var isCategoryCreated = await Create(new Category { Name = categoryName });

            //Assert
            isCategoryCreated.Should().Be(false);
        }
        #endregion

        #region Update
        [Fact]
        public async void Task_4_Update_Category_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string newCategoryName = CategoryInputs.newCategoryName;
            string editCategoryName = CategoryInputs.editCategoryName;

            var existingCategory = await RetrieveCategoryByName(newCategoryName);
            existingCategory.Name = editCategoryName;
            existingCategory.VersionNo = Convert.ToBase64String(existingCategory.RowVersion);

            //Act
            var response = await Update(existingCategory.CategoryID, existingCategory);

            //Assert
            response.Should().Be(HttpStatusCode.OK);

            var updatedCategory = await RetrieveCategoryByName(editCategoryName);
            updatedCategory.Name.Should().Be(editCategoryName);
        }

        [Fact]
        public async void Task_4_Update_Category_With_Duplicate_Name_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string existingCategoryName = CategoryInputs.chickenCategory;
            string editCategoryName = CategoryInputs.editCategoryName;

            var existingCategory = await RetrieveCategoryByName(existingCategoryName);
            existingCategory.Name = editCategoryName;
            existingCategory.VersionNo = Convert.ToBase64String(existingCategory.RowVersion);

            //Act
            var response = await Update(existingCategory.CategoryID, existingCategory);

            //Assert
            response.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void Task_4_Update_Category_With_Invalid_Version_Return_BadRequest()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string existingCategoryName = CategoryInputs.chickenCategory;
            string editCategoryName = CategoryInputs.editCategoryName;

            var existingCategory = await RetrieveCategoryByName(existingCategoryName);
            existingCategory.Name = editCategoryName;
            existingCategory.VersionNo = "WW12345";

            //Act
            var response = await Update(existingCategory.CategoryID, existingCategory);

            //Assert
            response.Should().Be(HttpStatusCode.Conflict);
        }
        #endregion

        #region Delete
        [Fact]
        public async void Task_5_Delete_Category_Return_Ok()
        {
            await AuthenticateAdminAsync();

            //Arrange
            string editCategoryName = CategoryInputs.editCategoryName;

            var existingCategory = await RetrieveCategoryByName(editCategoryName);

            //Act
            var response = await Delete(existingCategory.CategoryID);

            //Assert
            response.Should().Be(true);
        }

        [Fact]
        public async void Task_5_Delete_Category_Return_NotFound()
        {
            await AuthenticateAdminAsync();

            //Arrange
            long fakeCategoryID = CategoryInputs.fakeID;

            //Act
            var response = await Delete(fakeCategoryID);

            //Assert
            response.Should().Be(false);
        }
        #endregion
    }
}
