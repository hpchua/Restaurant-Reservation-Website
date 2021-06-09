namespace RestaurantReservation.Api.EmailService.MemoryStorage
{
    public class MemoryResultStorage : IMemoryResultStorage
    {
        private string Result;
        public void Add(bool isSuccess)
        {
            if (isSuccess)
                Result = "Email have been sent successfully!";
            else
                Result = "Email unable to send out";
        }

        public string Get()
        {
            return Result;
        }
    }
}
