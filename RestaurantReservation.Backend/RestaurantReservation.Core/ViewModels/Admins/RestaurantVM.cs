using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Core.ViewModels.Admins
{
    public static class RestaurantViewModel
    {
        //Schedule Details
        public static RestaurantScheduleVM FillScheduleDetails(RestaurantSchedule restaurantSchedule, Restaurant restaurant)
        {
            return new RestaurantScheduleVM
            {
                RestaurantID = restaurant.RestaurantID,
                RestaurantName = restaurant.Name,
                RestaurantWorkingDay = restaurant.WorkingDay,
                StartWorkingTime = restaurant.StartWorkingTime,
                EndWorkingTime = restaurant.EndWorkingTime,
                Schedule = restaurantSchedule,
            };
        }

        //Promotion Details
        public static RestaurantPromotionVM FillPromotionDetails(Promotion promotion, Restaurant restaurant)
        {
            return new RestaurantPromotionVM
            {
                RestaurantID = restaurant.RestaurantID,
                RestaurantName = restaurant.Name,
                RestaurantWorkingDay = restaurant.WorkingDay,
                StartWorkingTime = restaurant.StartWorkingTime,
                EndWorkingTime = restaurant.EndWorkingTime,
                Promotion = promotion,
            };
        }
    }

    // List Restaurant Details under which Categories, Schedules and Promotions 
    public class RestaurantCategorySchedulePromtionVM
    {
        public Restaurant Restaurant { get; set; }

        public List<string> Categories { get; set; }
        public List<long> CategoryIds { get; set; }

        public IEnumerable<RestaurantSchedule> RestaurantSchedules { get; set; }
        public IEnumerable<RestaurantSchedule> MemberRestaurantSchedules { get; set; }
        public IEnumerable<Promotion> Promotions { get; set; }
    }


    public class RestaurantCategoryVM
    {
        public Restaurant Restaurant { get; set; }

        public List<string> Categories { get; set; }

        [Required(ErrorMessage = "Must set at least one category for the product.")]
        [MinLength(1)]
        public List<long> CategoryIds { get; set; }
    }

    
    // Restaurant with Schedule
    public class RestaurantScheduleVM
    {
        public long RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantWorkingDay { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime EndWorkingTime { get; set; }
        public RestaurantSchedule Schedule { get; set; }
        public int Duration { get; set; }
    }

    // Restaurant with Promotions
    public class RestaurantPromotionVM
    {
        public long RestaurantID { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantWorkingDay { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime EndWorkingTime { get; set; }
        public Promotion Promotion { get; set; }
        public int SelectedType { get; set; }
    }
}
