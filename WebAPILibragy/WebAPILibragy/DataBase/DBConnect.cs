using Microsoft.EntityFrameworkCore;
using WebAPILibragy.model.database;

namespace WebAPILibragy.DataBase;

public class DBConnect : DbContext
{
    // Database DbSet Tables
    public DbSet<Author> Author { get; set; } = null!;
    public DbSet<Books> Books { get; set; } = null!;
    public DbSet<Genres> Genres { get; set; } = null!;
    public DbSet<List_Read_Status> List_Read_Status { get; set; } = null!;
    public DbSet<Loans> Loans { get; set; } = null!;
    public DbSet<Name_Books> Name_Books { get; set; } = null!;
    public DbSet<Publish> Publish { get; set; } = null!;
    public DbSet<Readers> Readers { get; set; } = null!;
    public DbSet<Account> Account { get; set; } = null!;
    public DbSet<role> role { get; set; } = null!;

    public DBConnect()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Library;Username=postgres;Password=SaraParker206");
    }
}
