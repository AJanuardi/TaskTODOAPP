using Microsoft.EntityFrameworkCore;
using TODODATABASE.Models;

namespace TODODATABASE.Data
{
    public class AppDbContext : DbContext
    {
         public DbSet<ToDo> Todos {get; set;}
         public DbSet<User> Users {get; set;}

    public AppDbContext(DbContextOptions options) : base (options) 
    {

    }
    }
}