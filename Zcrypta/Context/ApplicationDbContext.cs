using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zcrypta.Entities.Dtos;
using Zcrypta.Entities.Models;

namespace Zcrypta.Context
{
    public class ApplicationDbContext : IdentityDbContext<User> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //public DbSet<ChatMessage> ChatMessages { get; set; }
        //public DbSet<UserModel> Users { get; set; }
        //public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        //public DbSet<UserRoleModel> UserRoles { get; set; }
        //public DbSet<RoleModel> Roles { get; set; }
    }
}

