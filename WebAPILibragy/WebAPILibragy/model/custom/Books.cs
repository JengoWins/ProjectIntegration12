using System.Xml.Linq;
using WebAPILibragy.model.database;

namespace WebAPILibragy.model.custom;

[Serializable]
public class Books
{
    public Books() { }
    public Books(string Lauthor, string Fauthor, string Pauthor, string name, string genres, string publish, string city, int pages, string description, DateTime years) 
    {
        last_name = Lauthor;
        first_name = Fauthor;
        patronymic = Pauthor;
        this.name = name;
        this.genres = genres;
        publisher = publish; 
        this.city = city;
        this.pages = pages;
        this.description = description;
        this.years = years;
    }
    public string last_name { get; set; }
    public string first_name { get; set; }
    public string? patronymic { get; set; }
    public string name { get; set; }
    public string genres { get; set; }
    public string publisher { get; set; }
    public string city { get; set; }
    public int pages { get; set; }
    public string description { get; set; }
    public DateTime years { get; set; }
}
