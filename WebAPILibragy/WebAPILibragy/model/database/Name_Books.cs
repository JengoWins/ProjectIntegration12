namespace WebAPILibragy.model.database;

[Serializable]
public class Name_Books
{
    public Name_Books() { id = new Guid(); }
    public Guid id { get; set; }
    public string name { get; set; }
}
