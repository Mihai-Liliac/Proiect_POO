namespace HotelSelfCheck;

public class Client : User
{
    public List<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public Client(int id, string username, string password) : base (id, username, password, UserType.Client) {}
    private List<Room> availableRooms = new List<Room>();
    
    public List<Room> SearchAvailableRooms(List<Room> rooms, DateTime from, DateTime to, string type = null, List<string> facilities = null)
    {
        foreach (var room in rooms)
        {
            if (room.Status != RoomStatus.Libera)
                continue;
            
            if (!string.IsNullOrEmpty(type) && room.Type != type)
                continue;

            if (facilities != null && facilities.Count > 0)
            {
                bool allFacilitiesExist = true;
                foreach (var f in facilities)
                {
                    if (!room.Facilities.Contains(f))
                    {
                        allFacilitiesExist = false;
                        break;
                    }
                }
                
                if (!allFacilitiesExist)
                    continue;
            }

            availableRooms.Add(room);
        }

        return availableRooms;
    }
    
    public void CreateReservation(Room room, DateTime from, DateTime to)
    {
        var reservation = new Reservation(room.Number, Username, from, to, ReservationStatus.Activa);

        Reservations.Add(reservation);
        room.SetStatus(RoomStatus.Ocupata);

        Console.WriteLine($"Rezervarea camerei {room.Number} a fost realizata.");
    }

    public void CancelReservation(Reservation reservation, Room room)
    {
        reservation.SetStatus(ReservationStatus.Anulata);
        room.SetStatus(RoomStatus.Libera);

        Console.WriteLine($"Rezervarea camerei {room.Number} a fost anulata.");
    }

    public void CheckIn(Reservation reservation)
    {
        Console.WriteLine($"Check-in realizat pentru camera {reservation.RoomNumber}.");
    }

    public void CheckOut(Reservation reservation, Room room)
    {
        reservation.SetStatus(ReservationStatus.Finalizata);
        room.SetStatus(RoomStatus.Libera);

        Console.WriteLine($"check-out realizat pentru camera {reservation.RoomNumber}.");
    }

    public void ShowReservations()
    {
        Console.WriteLine("Rezervarile tale: ");
        foreach (var r in Reservations)
        {
            Console.WriteLine($"Camera {r.RoomNumber}, Status: {r.Status}, {r.From:dd/MM/yyyy} - {r.To:dd/MM/yyyy}");
        }
    }

    public void ModifyReservationDates(Reservation reservation, DateTime newFrom, DateTime newTo)
    {
        reservation.UpdateDates(newFrom, newTo);
        Console.WriteLine(
            $"Rezervarea camerei {reservation.RoomNumber} a fost modificata: {newFrom:dd/MM/yyyy} - {newTo:dd/MM/yyyy}");
    }
    
    public void ShowHistory()
    {
        Console.WriteLine("Istoric sejururi finalizate: ");
        foreach (var r in Reservations)
        {
            if (r.Status == ReservationStatus.Finalizata)
            {
                Console.WriteLine($"Camera {r.RoomNumber}, {r.From:dd/MM/yyyy} - {r.To:dd/MM/yyyy}");
            }
        }
    }
}