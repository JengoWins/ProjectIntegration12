namespace WebAPILibragy.model.database;

[Serializable]
public class Readers
{
    public Readers() { id = new Guid(); }
    public Readers(custom.Readers read)
    {
        id = new Guid();
        last_name = read.last_name;
        first_name = read.first_name;
        patronymic = read.patronymic;
        email = read.email;
        phone = read.phone;
        address = read.address;
    }
    public Guid id { get; set; }
    public string last_name { get; set; }
    public string first_name { get; set; }
    public string patronymic { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address { get; set; }

}
