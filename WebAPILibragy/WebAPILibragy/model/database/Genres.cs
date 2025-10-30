namespace WebAPILibragy.model.database;

[Serializable]
public class Genres
{
    public Genres() { id = new Guid(); }
    public Guid id { get; set; }
    public string name { get; set; }
}
