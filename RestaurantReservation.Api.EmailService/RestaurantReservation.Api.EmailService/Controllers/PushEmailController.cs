using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Api.EmailService.DB;
using RestaurantReservation.Api.EmailService.MemoryStorage;
using RestaurantReservation.Api.EmailService.Models;
using System.Collections.Generic;

namespace RestaurantReservation.Api.EmailService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushEmailController : Controller
    {
        private readonly IDataProvider dataProvider;
        private readonly IMemoryResultStorage memoryResultStorage;

        public PushEmailController(IDataProvider dataProvider,
                                   IMemoryResultStorage memoryResultStorage)
        {
            this.dataProvider = dataProvider;
            this.memoryResultStorage = memoryResultStorage;
        }

        [HttpGet]
        public ActionResult<List<string>> GetAllEmails()
        {
            return dataProvider.GetEmails();
        }

        [HttpGet("PromotionID/{ID}")]
        public ActionResult<Promotion> GetPromotionByID(long ID)
        {
            return dataProvider.GetPromotionByID(ID);
        }

        [HttpGet("Result")]
        public string GetEmailResult()
        {
            return memoryResultStorage.Get();
        }
    }
}
