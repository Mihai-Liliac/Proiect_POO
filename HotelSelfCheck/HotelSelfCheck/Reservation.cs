namespace HotelSelfCheck;

public class Reservation
{
    public int RoomNumber { get; private set; }
    public string ClientName { get; private set; }
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }
    public ReservationStatus Status { get; private set; }

    public Reservation(int roomNumber, string clientName, DateTime from, DateTime to, ReservationStatus status)
    {
        RoomNumber = roomNumber;
        ClientName = clientName;
        From = from;
        To = to;
        Status = status;
    }
    
    public void SetStatus(ReservationStatus newStatus)
    {
        Status = newStatus;
    }

    public void UpdateDates(DateTime newFrom, DateTime newTo)
    {
        if (newFrom >= newTo)
            throw new ArgumentException("Data de început trebuie să fie înainte de data de sfârșit.");
    
        From = newFrom;
        To = newTo;
    }
}