namespace RestaurantReservation.Api.EmailService.MemoryStorage
{
    public interface IMemoryResultStorage
    {
        void Add(bool isSuccess);
        string Get();
    }
}
