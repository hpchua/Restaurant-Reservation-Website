namespace RestaurantReservation.Core.DTO
{
    public class ForgotPasswordMessage
    {
        public string Email { get; set; }
        public string Title { get; set; }
        public string Token { get; set; }
    }
}
