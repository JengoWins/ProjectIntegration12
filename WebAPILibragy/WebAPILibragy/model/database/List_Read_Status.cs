namespace WebAPILibragy.model.database;

[Serializable]
public class List_Read_Status
{
    public List_Read_Status() { id = new Guid(); }
    public Guid id { get; set; }
    public string status { get; set; }
}
