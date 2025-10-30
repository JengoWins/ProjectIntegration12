namespace WebAPILibragy.model.custom;

[Serializable]
public class Loans_Min
{
    public Loans_Min() { }
    public Loans_Min(string Luser, string Fuser, string phone, string name, string genres, string publish,string status, DateTime years)
    { 
        this.last_name = Luser;
        first_name = Fuser;
        this.phone = phone;
        name_book = name;
        this.genres = genres;
        this.publisher = publish;
        this.time_of_issue = years;
        this.status = status;
    }
    public string last_name { get; set; }
    public string first_name { get; set; }
    public string phone { get; set; }
    public string name_book { get; set; }
    public string genres { get; set; }
    public string publisher { get; set; }
    public string status { get; set; }
    public DateTime time_of_issue { get; set; }
}
