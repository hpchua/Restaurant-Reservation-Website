using RestaurantReservation.Core.Entities;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IRefreshTokenService
    {
        public RefreshToken Get(string refreshToken);
        public Task AddRefreshToken(RefreshToken refreshToken);
    }
}
