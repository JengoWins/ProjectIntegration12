namespace WebAPILibragy.model.database;

public class Account
{
    public Account()
    {
        id = new Guid();
    }

    public Guid id { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public Guid id_role { get; set; }
}
