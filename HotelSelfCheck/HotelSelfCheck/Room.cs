namespace HotelSelfCheck;

public class Room
{
    public int Number { get; private set; }
    public string Type { get; private set; }
    public List<string> Facilities { get; private set; }
    public RoomStatus Status { get; private set; }

    public Room(int number, string type, RoomStatus status, List<string> facilities)
    {
        Number = number;
        Type = type;
        Facilities = facilities;
        Status = status;
    }

    public void SetStatus(RoomStatus status)
    {
        Status = status;
    }

    public void UpdateType(string type)
    {
        Type = type;
    }

    public void UpdateFacilities(List<string> facilities)
    {
        Facilities = facilities;
    }
}