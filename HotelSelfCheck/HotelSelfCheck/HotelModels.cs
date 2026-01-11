using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HotelUI
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class Room
    {
        public int RoomNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Facilities { get; set; } = "WiFi, TV, AC";
    }

    public class Reservation
    {
        public string ReservationID { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        public string ClientName { get; set; }
        public string RoomType { get; set; }
        public int RoomNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

        public string DateRange => $"{CheckIn:dd MMM} - {CheckOut:dd MMM}";
        
        [System.Text.Json.Serialization.JsonIgnore]
        public string ShowCheckIn => Status == "Booked" ? "Visible" : "Collapsed";
        
        [System.Text.Json.Serialization.JsonIgnore]
        public string ShowCheckOut => Status == "Checked-in" ? "Visible" : "Collapsed";
    }

    public class HotelData
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public List<User> Users { get; set; } = new List<User>();
        public Dictionary<string, decimal> Prices { get; set; } = new Dictionary<string, decimal>
        {
            { "Single Room", 50 }, { "Double Deluxe", 85 }, { "Presidential Suite", 150 }
        };

        public void InitializeDefaults()
        {
            if (Rooms == null) Rooms = new List<Room>();
            if (Reservations == null) Reservations = new List<Reservation>();
            if (Users == null) Users = new List<User>();
            if (Prices == null) Prices = new Dictionary<string, decimal>();

            if (!Users.Any(u => u.Role == "Admin"))
            {
                Users.Add(new User { Username = "admin", Password = "admin", Role = "Admin" });
            }
        }
    }

    public static class DataStore
    {
        private static readonly string filePath = "hotel_database.json";

        public static void Save(HotelData data)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(data, options));
            }
            catch { }
        }

        public static HotelData Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    var n = new HotelData();
                    n.InitializeDefaults();
                    return n;
                }
                var data = JsonSerializer.Deserialize<HotelData>(File.ReadAllText(filePath));
                data?.InitializeDefaults();
                return data ?? new HotelData();
            }
            catch
            {
                var f = new HotelData();
                f.InitializeDefaults();
                return f;
            }
        }
    }
}