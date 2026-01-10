using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HotelUI
{
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
        public Dictionary<string, decimal> Prices { get; set; } = new Dictionary<string, decimal>
        {
            { "Single Room", 50 }, { "Double Deluxe", 85 }, { "Presidential Suite", 150 }
        };

        public void InitializeDefaults()
        {
            if (Rooms == null) Rooms = new List<Room>();
            if (Reservations == null) Reservations = new List<Reservation>();
            if (Prices == null) Prices = new Dictionary<string, decimal>();
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
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public static HotelData Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    var newData = new HotelData();
                    newData.InitializeDefaults();
                    return newData;
                }
                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<HotelData>(json);
                if (data == null) data = new HotelData();
                data.InitializeDefaults();
                return data;
            }
            catch
            {
                var fallback = new HotelData();
                fallback.InitializeDefaults();
                return fallback;
            }
        }
    }

    public class AvailableRoomViewModel
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public decimal PricePerNight { get; set; }
        public string Facilities { get; set; }
    }
}