using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository refreshTokenRepository;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            this.refreshTokenRepository = refreshTokenRepository;
        }

        public RefreshToken Get(string refreshToken)
        {
            return refreshTokenRepository.Get(refreshToken);
        }

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            await refreshTokenRepository.AddRefreshToken(refreshToken);
        }
    }
}
