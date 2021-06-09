using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Core.ViewModels.Members;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RestaurantReservation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BookingController : Controller
    {
        private readonly ILogger<BookingController> logger;

        private readonly IBookingService bookingService;
        private readonly IRestaurantService restaurantService;
        private readonly IRestaurantScheduleService restaurantScheduleService;
        private readonly IUserService userService;
        private readonly IPublisher publisher;

        public BookingController(ILogger<BookingController> logger,
                                 IBookingService bookingService,
                                 IRestaurantService restaurantService,
                                 IRestaurantScheduleService restaurantScheduleService,
                                 IUserService userService,
                                 IPublisher publisher)
        {
            this.logger = logger;
            this.bookingService = bookingService;
            this.restaurantService = restaurantService;
            this.restaurantScheduleService = restaurantScheduleService;
            this.userService = userService;
            this.publisher = publisher;
        }

        #region GetBookingListCount - GET
        /// <summary>
        /// Retrive the total number of members' pending reservation in database. 
        /// </summary>
        /// <returns>
        /// Number of Members' Pending Reservation will be returned.       
        /// </returns>
        /// <response code="200">Number of members' pending reservation record</response>
        /// <response code="500">Error Retrieving number of members' pending reservation record</response>   
        [HttpGet("Admin/ListCount")]
        [AllowAnonymous]
        [SwaggerOperation(OperationId = nameof(AdminGetCount))]
        [ProducesResponseType(typeof(Int32), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Int32>> AdminGetCount()
        {
            return await bookingService.AdminGetPendingBookingCount();
        }
        #endregion

        #region GetAllBookingsOfStatus - GET
        /// <summary>
        /// Get all members' bookings of a specific status.
        /// </summary>
        /// <param name="status">Booking status, e.g. Pending, Expired, Complete</param>
        /// <returns>
        /// A list of members' booking records will be returned.
        /// </returns>
        /// <response code="200">All members' booking records</response>
        /// <response code="500">Error Retrieving members' booking records from database</response>  
        [HttpGet("Admin/{status}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(GetAllBookingsOfStatus))]
        [ProducesResponseType(typeof(IEnumerable<Booking>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<Booking>> GetAllBookingsOfStatus(string status)
        {
            var bookings = await bookingService.GetAll();
            bookings = FilterBookingStatus(bookings, status);

            logger.LogInformation($"All the Members' Bookings {bookings} has been retrieved successfully {SD.HTTPLogging.OK}");
            return bookings;
        }

        private IEnumerable<Booking> FilterBookingStatus(IEnumerable<Booking> bookings, string status)
        {
            switch (status)
            {
                case SD.BookingStatus.PENDING:
                    bookings = bookings.Where(b => b.BookingStatus.Equals(SD.BookingStatus.PENDING));
                    break;
                case SD.BookingStatus.EXPIRED:
                    bookings = bookings.Where(b => b.BookingStatus.Equals(SD.BookingStatus.EXPIRED));
                    break;
                case SD.BookingStatus.COMPLETE:
                    bookings = bookings.Where(b => b.BookingStatus.Equals(SD.BookingStatus.COMPLETE));
                    break;
            }
            return bookings;
        }
        #endregion

        #region Get - HttpGet
        /// <summary>
        /// Retrieve a specific booking details based on the Booking Number given.
        /// </summary>
        /// <param name="bookingNo">Booking Number</param>
        /// <returns>
        /// Relevant booking details will be returned
        /// </returns>
        /// <response code="200">BookingDetailVM object that contains booking details</response>
        /// <response code="400">Booking Details Not Found</response>
        /// <response code="500">Error Retrieving booking detail info from database</response>   
        [HttpGet("Details/BookingNo/{bookingNo}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(GetByBookingNo))]
        [ProducesResponseType(typeof(BookingDetailVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingDetailVM>> GetByBookingNo(string bookingNo)
        {
            var bookingToFound = await bookingService.GetByNumber(bookingNo);

            if (bookingToFound == null)
            {
                logger.LogWarning($"Booking of Number {bookingNo} not found {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            var bookingDetailToFound = await bookingService.GetDetailsByID(bookingToFound.BookingID);

            BookingDetailVM bookingDetailVM = new BookingDetailVM
            {
                Booking = bookingToFound,
                BookingDetail = bookingDetailToFound,
            };

            logger.LogInformation($"Booking of Number {bookingNo} have been retrieved {SD.HTTPLogging.OK}");
            return bookingDetailVM;
        }
        #endregion

        #region Get - HttpGet
        /// <summary>
        /// Get all bookings of a specific status for a specific user.
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <param name="status">Order Status, eg. Pending, Complete, Expired</param>
        /// <returns>
        /// Relevant booking details will be returned based on the user id and status given
        /// </returns>
        /// <response code="200">A list of BookingHistoryVM object that contains specific member's booking details</response>
        /// <response code="500">Error Retrieving booking detail info from database</response>  
        [HttpGet("Member/{userID}/{status}")]
        [Authorize(Roles = SD.ROLE_MEMBER)]
        [SwaggerOperation(OperationId = nameof(GetAllBookingsByUserID))]
        [ProducesResponseType(typeof(BookingHistoryVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingHistoryVM>> GetAllBookingsByUserID(string userID, string status)
        {
            var bookingList = await bookingService.GetByUserID(userID, status);
            BookingDetail bookingDetail = null;

            BookingHistoryVM memberBookingDetailVM = new BookingHistoryVM
            {
                Bookings = bookingList,
                BookingDetails = new List<BookingDetail>(),
            };

            foreach (var booking in bookingList)
            {
                bookingDetail = await bookingService.GetDetailsByID(booking.BookingID);
                memberBookingDetailVM.BookingDetails.Add(bookingDetail);
            }

            logger.LogInformation($"Member {userID}'s Booking History have been retrieved {SD.HTTPLogging.OK}");
            return memberBookingDetailVM;
        }
        #endregion


        #region Add - POST
        /// <summary>
        /// Add a new member booking to the database. This checks for number of seat avaiable for each session also.
        /// </summary>
        /// <param name="bookingVM">MakeBookingVM object</param>
        /// <returns>
        /// New Member Booking and the number of schedule avaialble seat will be updated from the database
        /// </returns>
        /// <response code="200">New Booking Object</response>
        /// <response code="500">Error Adding a member booking information into database</response>   
        [HttpPost]
        [Authorize(Roles = SD.ROLE_MEMBER)]
        [SwaggerOperation(OperationId = nameof(Add))]
        [ProducesResponseType(typeof(Booking), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Booking>> Add(MakeBookingVM bookingVM)
        {
            var scheduleToFound = await restaurantScheduleService.GetScheduleByID(bookingVM.ScheduleID);
            int pax = scheduleToFound.AvailableSeat - bookingVM.Pax;

            if (pax < 0)
            {
                logger.LogWarning($"Unable to add member new booking {bookingVM.Booking} due to insufficient seat {SD.HTTPLogging.BAD_REQUEST}");
                return null;
            }

            try
            {
                var newBooking = await bookingService.Add(bookingVM.Booking);
                if (newBooking == null)
                {
                    logger.LogWarning($"Unable to add member new booking {bookingVM.Booking} {SD.HTTPLogging.BAD_REQUEST}");
                    return null;
                }

                BookingDetail bookingDetail = new BookingDetail
                {
                    BookingID = newBooking.BookingID,
                    ScheduleID = bookingVM.ScheduleID,
                    Pax = bookingVM.Pax,
                };

                await bookingService.CreateBookingDetail(bookingDetail);
                await GenerateBookingReceipt(bookingVM, scheduleToFound);
                logger.LogInformation($"Member {bookingVM} new booking has been made and sent email has been created with booking ID {newBooking.BookingID} {SD.HTTPLogging.OK}");
                return newBooking;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while adding new category {bookingVM.Booking} to database {SD.HTTPLogging.INTERNAL}");
                return null;
            }
        }

        private async Task GenerateBookingReceipt(MakeBookingVM bookingVM, RestaurantSchedule scheduleToFound)
        {
            var userToFound = await userService.GetByID(bookingVM.Booking.UserID);
            var restaurantToFound = await restaurantService.GetRestaurantByID(scheduleToFound.RestaurantID);

            string content = $"<h2>{restaurantToFound.Name} Receipt</h2>" +
                "<p> Dear " + userToFound.Name + ", </p> <br />" +
                "We are here to inform you have place a reservation for a restaurant schedule, please show this receipt on the booking day <br /><br />" +
                "<table>" +
                    "<tr>" +
                        "<td style='width: 200px'><b>Booking Number</b></td>" +
                        "<td>: <b>" + bookingVM.Booking.BookingNo + "</b></td>" +
                        "<td style='width: 50px; border: 0'> &nbsp; </td>" +
                        "<td style='width: 200px'> Restaurant Name</td>" +
                        "<td>: " + restaurantToFound.Name + "</td>" +
                    "</tr>" +
                    "<tr>" +
                        "<td style='width: 200px'> Booking Date </td>" +
                        "<td>: " + bookingVM.Booking.BookingDate + "</td>" +
                        "<td style='width: 50px; border: 0'> &nbsp; </td>" +
                        "<td style='width: 200px'> Restaurant Schedule Session </td>" +
                        "<td>: " + scheduleToFound.ScheduleDate.ToString("dd/MM/yyyy") + " " + scheduleToFound.StartTime.ToString("hh:mm tt") + " - " + scheduleToFound.EndTime.ToString("hh:mm tt") + "</td>" +
                    "</tr>" +
                "</table>" +
                "<hr />" +
                "<table>" +
                    "<tr>" +
                        "<td style='width: 200px'> Number of Person </td>" +
                        "<td>: " + bookingVM.Pax + "</td>" +
                    "</tr>" +
                    "<tr>" +
                        "<td style='width: 200px'> Full Name </td>" +
                        "<td>: " + bookingVM.Booking.FullName + "</td>" +
                    "</tr>" +
                    "<tr>" +
                        "<td style='width: 200px'> Phone Number </td>" +
                        "<td>: " + bookingVM.Booking.PhoneNo + "</td>" +
                    "</tr>" +
                "</table>";

            BookingReceipt bookingReceipt = new BookingReceipt
            {
                Email = userToFound.Email,
                Title = "Booking Receipt",
                Content = content,
            };

            publisher.Publish(
                message: JsonConvert.SerializeObject(bookingReceipt),
                routingKey: "email.booking",
                messageAttributes: null
            );
        }
        #endregion

        #region Update - HttpPut
        /// <summary>
        /// Update an existing member's booking in the database. 
        /// </summary>
        /// <param name="bookingID">Booking ID</param>
        /// <param name="booking">Booking Object Information</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, update member's booking status process unable to be conducted due to booking ID not found.
        /// </returns>
        /// <response code="204">Deleting category successfully</response>
        /// <response code="400">Invalid Booking ID message</response>
        /// <response code="404">No record found based on Booking ID given</response>
        /// <response code="500">Error Updating a Booking object information from database</response>  
        [HttpPut("{bookingID}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Update))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> Update(long bookingID, Booking booking)
        {
            if (bookingID != booking.BookingID)
            {
                logger.LogWarning($"ID Requested {bookingID} is not matched with Booking of ID {booking.BookingID} {SD.HTTPLogging.BAD_REQUEST}");
                return BadRequest();
            }

            var bookingFromDB = await bookingService.GetByID(bookingID);

            if (bookingFromDB == null)
            {
                logger.LogWarning($"ID Requested {bookingID} is not found from the database {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await bookingService.Update(booking);
                logger.LogInformation($"Booking {booking} with ID {booking.BookingID} Data has been updated {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while updating existing booking {booking} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion

        #region Delete - HttpDelete
        /// <summary>
        /// Remove an existing booking information from database, for testing
        /// </summary>
        /// <param name="id">booking ID</param>
        /// <returns>
        /// No response return back.
        /// Otherwise, deleting booking process unable to be conducted due to schedule ID not found.
        /// </returns>
        /// <response code="204">Updating booking successfully Result</response>
        /// <response code="404">Booking record not found</response>
        /// <response code="500">Error Deleting a booking object information from database</response>   
        [HttpDelete("Testing/{id}")]
        [Authorize(Roles = SD.ROLE_ADMIN)]
        [SwaggerOperation(OperationId = nameof(Delete))]
        [ProducesResponseType(typeof(object), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            var bookingToDelete = await bookingService.GetByID(id);
            if (bookingToDelete == null)
            {
                logger.LogWarning($"Attempt to delete non-existing booking of ID {id} {SD.HTTPLogging.NOT_FOUND}");
                return NotFound();
            }

            try
            {
                await bookingService.Delete(bookingToDelete);
                logger.LogInformation($"Booking with ID {id} Data has been deleted {SD.HTTPLogging.OK}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting Booking with ID {id} from database {SD.HTTPLogging.INTERNAL}");
                return StatusCode(Convert.ToInt32(HttpStatusCode.InternalServerError));
            }
            return NoContent();
        }
        #endregion
    }
}
