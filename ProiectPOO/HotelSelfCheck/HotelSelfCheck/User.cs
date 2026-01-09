namespace Hotel_Self_Check_in;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }
    
    public UserType Type { get; private set; }
    
    public User(int id, string username, string password, UserType type)
    {
        Id = id;
        Username = username;
        Password = password;
        Type = type;
    }
}