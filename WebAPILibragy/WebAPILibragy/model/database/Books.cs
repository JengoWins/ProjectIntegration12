namespace WebAPILibragy.model.database;

[Serializable]
public class Books
{
    public Books() { id = new Guid(); }
    public Guid id { get; set; }
    public Guid id_author { get; set; }
    public Guid id_name { get; set; }
    public Guid id_genres { get; set; }
    public Guid id_publish { get; set; }
    public int pages {  get; set; }
    public string description {  get; set; }
    public DateTime years {  get; set; }
}
