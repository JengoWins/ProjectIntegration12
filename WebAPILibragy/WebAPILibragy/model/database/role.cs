namespace WebAPILibragy.model.database;

public class role
{
    public role()
    {
        id = new Guid();
    }

    public Guid id { get; set; }
    public string roles { get; set; }
}
