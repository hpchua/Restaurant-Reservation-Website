namespace RestaurantReservation.IntegrationTests.Configurations.Api
{
    public static class ApiRoutes
    {
        private static readonly string _baseUrl = "https://localhost:44320/api/";

        public static class Authenticate
        {
            private static readonly string AuthenticateControllerUrl = string.Concat(_baseUrl, "Authenticate");

            public static readonly string Login = string.Concat(AuthenticateControllerUrl, "/", "Login");
            public static readonly string RefreshToken = string.Concat(AuthenticateControllerUrl, "/", "RefreshToken");
            public static readonly string Register = string.Concat(AuthenticateControllerUrl, "/", "Register");
            public static readonly string ForgotPassword = string.Concat(AuthenticateControllerUrl, "/", "ForgotPassword/{email}");
        }

        public static class Categories
        {
            private static readonly string categoryControllerUrl = string.Concat(_baseUrl, "Category");

            public static readonly string GetCount = string.Concat(categoryControllerUrl, "/", "Count");
            public static readonly string GetAll = categoryControllerUrl;
            public static readonly string GetById = string.Concat(categoryControllerUrl, "/", "{id}");
            public static readonly string GetByName = string.Concat(categoryControllerUrl, "/", "name/{name}");
            public static readonly string Post = categoryControllerUrl;
            public static readonly string Put = string.Concat(categoryControllerUrl, "/", "{id}");
            public static readonly string Delete = string.Concat(categoryControllerUrl, "/", "{userId}/{id}");
        }

        public static class Users
        {
            private static readonly string userControllerUrl = string.Concat(_baseUrl, "User");

            public static readonly string GetAll = string.Concat(userControllerUrl, "/", "{role}");
            public static readonly string GetById = string.Concat(userControllerUrl, "/", "Info/{userID}");
            public static readonly string CheckExistingEmail = string.Concat(userControllerUrl, "/", "Check/{email}");
            public static readonly string ChangePassword = string.Concat(userControllerUrl, "/", "ChangePassword");
            public static readonly string UpdateProfile = userControllerUrl;
            public static readonly string DeleteUser = string.Concat(userControllerUrl, "/", "{userId}");
        }

        public static class Restaurants
        {
            private static readonly string restaurantControllerUrl = string.Concat(_baseUrl, "Restaurant");

            public static readonly string GetCount = string.Concat(restaurantControllerUrl, "/", "Count");
            public static readonly string GetAll = restaurantControllerUrl;
            public static readonly string GetById = string.Concat(restaurantControllerUrl, "/", "{id}/{ScheduleStatus}/{PromotionStatus}");
            public static readonly string GetEditInfoById = string.Concat(restaurantControllerUrl, "/", "EditInfo/{id}");
            public static readonly string Post = restaurantControllerUrl;
            public static readonly string Put = string.Concat(restaurantControllerUrl, "/", "{id}");
            public static readonly string Delete = string.Concat(restaurantControllerUrl, "/", "{userId}/{id}");
            public static readonly string DeleteCompletely = string.Concat(restaurantControllerUrl, "/", "Testing/{id}");
        }

        public static class Schedules
        {
            private static readonly string scheduleControllerUrl = string.Concat(_baseUrl, "Schedule");

            public static readonly string GetCount = string.Concat(scheduleControllerUrl, "/", "Count/{restaurantID}");
            public static readonly string GetAll = string.Concat(scheduleControllerUrl, "/", "All/{id}");
            public static readonly string GetById = string.Concat(scheduleControllerUrl, "/", "{id}");
            public static readonly string Post = scheduleControllerUrl;
            public static readonly string Put = string.Concat(scheduleControllerUrl, "/", "{id}");
            public static readonly string Delete = string.Concat(scheduleControllerUrl, "/", "{userId}/{status}/{id}");
            public static readonly string DeleteCompletely = string.Concat(scheduleControllerUrl, "/", "Testing/{id}");
        }

        public static class Promotions
        {
            private static readonly string promotionControllerUrl = string.Concat(_baseUrl, "Promotion");

            public static readonly string GetCount = string.Concat(promotionControllerUrl, "/", "Count/{restaurantID}");
            public static readonly string GetById = string.Concat(promotionControllerUrl, "/", "{id}");
            public static readonly string Post = promotionControllerUrl;
            public static readonly string Put = string.Concat(promotionControllerUrl, "/", "{id}");
            public static readonly string Delete = string.Concat(promotionControllerUrl, "/", "{userId}/{id}");
        }

        public static class Bookings
        {
            private static readonly string bookingControllerUrl = string.Concat(_baseUrl, "Booking");

            public static readonly string GetCount = string.Concat(bookingControllerUrl, "/", "Admin/ListCount");
            public static readonly string GetAll = string.Concat(bookingControllerUrl, "/", "Admin/{status}");
            public static readonly string GetAllByUserID = string.Concat(bookingControllerUrl, "/", "Member/{userID}/{status}");
            public static readonly string GetByBookingNo = string.Concat(bookingControllerUrl, "/", "Details/BookingNo/{bookingNo}");
            public static readonly string Post = bookingControllerUrl;
            public static readonly string Put = string.Concat(bookingControllerUrl, "/", "{bookingID}");
            public static readonly string Delete = string.Concat(bookingControllerUrl, "/", "Testing/{id}");
        }
    }
}
