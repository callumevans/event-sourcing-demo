using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI
{
    public class ToDoListContext : DbContext
    {
        public DbSet<ToDoItem> ToDoItems { get; set; }
        
        public ToDoListContext(DbContextOptions<ToDoListContext> options)
            : base(options)
        {
        }
    }
}