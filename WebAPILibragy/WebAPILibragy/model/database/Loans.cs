namespace WebAPILibragy.model.database;

[Serializable]
public class Loans
{
    public Loans() { id = new Guid(); }
    public Guid id { get; set; }
    public Guid id_books { get; set; }
    public Guid id_readers { get; set; }
    public Guid id_status  { get; set; }
    public DateTime time_of_issue { get; set; }
}

