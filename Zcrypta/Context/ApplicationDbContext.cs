using Microsoft.EntityFrameworkCore;
using Zcrypta.Entities.Dtos;

namespace Zcrypta.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}

