namespace WebAPILibragy.model.database;

[Serializable]
public class Publish
{
    public Publish() { id = new Guid(); }
    public Guid id { get; set; }
    public string name { get; set; }
    public string city { get; set; }
}
