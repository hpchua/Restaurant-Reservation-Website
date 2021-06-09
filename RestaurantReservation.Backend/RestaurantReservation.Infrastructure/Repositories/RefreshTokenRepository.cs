using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantReservation.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DatabaseContext db;

        public RefreshTokenRepository(DatabaseContext context)
        {
            db = context;
        }

        public RefreshToken Get(string refreshToken)
        {
            return db.RefreshTokens.Where(rt => rt.Token == refreshToken).OrderByDescending(rt => rt.ExpiryDate).FirstOrDefault();
        }

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            await db.RefreshTokens.AddAsync(refreshToken);
            await db.SaveChangesAsync();
        }
    }
}
