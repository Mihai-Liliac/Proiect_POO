using System.IO;
using System.Text.Json;

namespace HotelSelfCheck
{
    public class DataStore
    {
        private static readonly string filePath = "hotel.Data.json";

        public static void Save(HotelData data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
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
            catch (Exception)
            {
                var fallbackData = new HotelData();
                fallbackData.InitializeDefaults();
                return fallbackData;
            }
        }
    }
}