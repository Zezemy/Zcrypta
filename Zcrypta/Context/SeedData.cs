﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Zcrypta.Context
{
    public class SeedData
    {
        private static readonly IEnumerable<SeedUser> seedUsers =
        [
            new SeedUser()
        {
            Email = "zy@zcrypta.com",
            NormalizedEmail = "zy@zcrypta.com",
            NormalizedUserName = "zy@zcrypta.com",
            RoleList = [ "Administrator", "Manager" ],
            UserName = "zy@zcrypta.com"
        },
        new SeedUser()
        {
            Email = "zey@zcrypta.com",
            NormalizedEmail = "zey@zcrypta.com",
            NormalizedUserName = "zey@zcrypta.com",
            RoleList = [ "User" ],
            UserName = "zey@zcrypta.com"
        },
        ];

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            if (context.Users.Any())
            {
                return;
            }

            var userStore = new UserStore<User>(context);
            var password = new PasswordHasher<User>();

            using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = ["Administrator", "Manager", "User"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            using var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            foreach (var user in seedUsers)
            {
                var hashed = password.HashPassword(user, "Passw0rd!");
                user.PasswordHash = hashed;
                await userStore.CreateAsync(user);

                if (user.Email is not null)
                {
                    var User = await userManager.FindByEmailAsync(user.Email);

                    if (User is not null && user.RoleList is not null)
                    {
                        await userManager.AddToRolesAsync(User, user.RoleList);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private class SeedUser : User
        {
            public string[]? RoleList { get; set; }
        }
    }
}
