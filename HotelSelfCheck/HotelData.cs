using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelSelfCheck
{
    public class HotelData
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public List<User> Users { get; set; } = new List<User>();

        public void InitializeDefaults()
        {
            if (Rooms == null) Rooms = new List<Room>();
            if (Reservations == null) Reservations = new List<Reservation>();
            if (Users == null) Users = new List<User>();

            if (!Users.Any(u => u.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)))
                Users.Add(new User(1, "admin", "admin", UserType.Admin));

            if (!Users.Any(u => u.Username.Equals("client", StringComparison.OrdinalIgnoreCase)))
                Users.Add(new Client(2, "client", "1234"));
        }
    }
}