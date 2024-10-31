using Microsoft.EntityFrameworkCore;
using Zcda.Entities.Dtos;

namespace BlazorWebAppTest1.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}

