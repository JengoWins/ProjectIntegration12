using System.ComponentModel.DataAnnotations;
using WebAPILibragy.model.database;

namespace WebAPILibragy.model.custom;

[Serializable]
public class Account
{
    public Account() { }
    public Account(string name, string pass)
    {
        this.username = name;
        this.password = pass;
    }
    [Required]
    public string username { get; set; }
    [Required]
    public string password { get; set; }
}
