namespace RestaurantReservation.Core.Utils
{
    public class SD
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_MEMBER = "Member";

        public const string CONTENT_JSON = "application/json";

        public static class BookingStatus
        {
            public const string PENDING = "Pending";
            public const string EXPIRED = "Expired";
            public const string COMPLETE = "Completed";
        }

        /// <summary>
        /// Types of Promotion
        /// </summary>
        public enum PromotionType
        {
            Coupon = 1,
            Discount = 2,
            Giveaways = 3,
        }

        /// <summary>
        /// Audit Trail
        /// </summary>
        public enum ActionType
        {
            Update = 1,
            Delete = 2
        }

        /// <summary>
        /// Working Days for Operating Hour
        /// </summary>
        public enum WorkingDays : int
        {
            Daily = 1,
            Weekend = 2,
        }

        /// <summary>
        /// Schedule Status
        /// </summary>
        public enum ScheduleStatus : int
        {
            Available = 1,
            Full = 2,
            Expired = 3,
            Unavailable = 4,
        }

        /// <summary>
        /// Logging Message
        /// </summary>
        public static class HTTPLogging
        {
            public const string OK = "> HTTP 200 - OK";
            public const string BAD_REQUEST = "> HTTP 400 - Bad Request";
            public const string UNAUTHORIZED = "> HTTP 401 - Unauthorized";
            public const string NOT_FOUND = "> HTTP 404 - Not Found";
            public const string CONFLICT = "> HTTP 409 - Conflict";
            public const string INTERNAL = "> HTTP 500 - Internal Server Error";
        }

        /// <summary>
        /// API HTTP response status codes
        /// </summary>
        public static class StatusCode
        {
            public const int UNAUTHORIZED = 401;
            public const int NOT_FOUND = 404;
            public const int OK = 200;
            public const int BAD_REQUEST = 400;
            public const int INTERNAL = 500;
        }

        /// <summary>
        /// [UI] - Authentication Policy and Claim
        /// </summary>
        public static class Policy
        {
            public const string ADMIN_ONLY = "AdminOnly";
            public const string MEMBER_ONLY = "MemberOnly";
            public const string AUTHENTICATED_ONLY = "AuthenticatedOnly";
        }
    }
}
