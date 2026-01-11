namespace HotelSelfCheck;

public class Administrator : User
{
    public List<Room> Rooms { get; private set; } = new List<Room>();
    public List<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public Administrator(int id, string username, string password) : base(id, username, password, UserType.Admin) {}
    //Mihai  (Administrarea camerelor)

    public void CreateRoom(Room room)
    {
        if (Rooms.Any(r => r.Number == room.Number))
            throw new InvalidOperationException("The room already exists.");
        Rooms.Add(room);
    }

    public void UpdateRoom(int number, string newType, List<string> newFacilities)
    {
        var room = Rooms.FirstOrDefault(r => r.Number == number);
        if (room == null)
            throw new InvalidOperationException("The room doesn't exist.");
        
        room.UpdateType(newType);
        room.UpdateFacilities(newFacilities);
    }

    public void DeleteRoom(int number)
    {
        var room = Rooms.FirstOrDefault(r => r.Number == number);
        if (room == null)
            throw new InvalidOperationException("The room doesn't exist.");

        Rooms.Remove(room);
    }

    //Denis (Gestionarea rezervarilor)
    
    public void ShowReservation()
    {
        Console.WriteLine("Rezervari: ");
        foreach (var r in Reservations)
        {
            Console.WriteLine($"Camera {r.RoomNumber}, Client: {r.ClientName}, Status: {r.Status}, {r.From} - {r.To}");
        }
    }
    
    public void UpdateReservationStatus(int roomNumber, string clientName, ReservationStatus newStatus)
    {
        foreach (var r in Reservations)
        {
            if (r.RoomNumber == roomNumber && r.ClientName == clientName)
            {
               r.SetStatus(newStatus);
               Console.WriteLine($"Statusul rezervarii camerei {roomNumber} a fost schimbat in {newStatus}");
               return;
            }
        }
    }

    //Ionut (Configurarea regulilor generale)

    public class HotelRules
    {    
        //TimeSpan -- Se foloseste pentru intervale de timp
        //TimeSpan == cat timp ; ideal pt reguli generale
        public TimeSpan CheckInStart { get; private set; }
        public TimeSpan CheckInEnd { get; private set; }
        public TimeSpan CheckOutLimit { get; private set; }
        public int LengthOfStay { get; private set; }

        public void SetCheckIn(TimeSpan start, TimeSpan end)
        {
            if (start >= end)
            {
                throw new ArgumentException("Check-in invalid.\n");
            }
            CheckInStart = start;
            CheckInEnd = end;
        }

        public void SetCheckOut(TimeSpan limit)
        {
            CheckOutLimit = limit;
        }
        public void SetLenghtOfStay (int days)
        {
            if (days <= 0)
                throw new ArgumentException("The maximum duration must be positive.\n");
            LengthOfStay = days;
        }

        public void ShowRules()
        {
            Console.WriteLine("--- General Hotel Rules ---");
            Console.WriteLine($"Check-in: {CheckInStart} - {CheckInEnd}");
            Console.WriteLine($"Check-out by : {CheckOutLimit}");
            Console.WriteLine($"Maximum stay duration : {LengthOfStay} days ");
        }
    }
}