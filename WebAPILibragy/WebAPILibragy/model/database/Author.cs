using System.ComponentModel.DataAnnotations;

namespace WebAPILibragy.model.database;

[Serializable]
public class Author
{
    public Author()
    {
        id = new Guid();
    }

    public Guid id { get; set; }
    public string last_name { get; set; }
    public string first_name { get; set; }
    public string patronymic { get; set; }
}

