using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using Zcrypta.Models;
using Zcrypta.Entities.Strategies.Options;
using Zcrypta.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

        public static readonly IEnumerable<TradingPair> tradingPairs =
        [
            new TradingPair()
        {
            Base = "BTC",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "ETH",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "TRY",
            Quote = "USDT"
        }
        ];

        public static readonly IEnumerable<SignalStrategy> signalStrategies =
        [
            new SignalStrategy()
        {
            StrategyType = 0,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 1,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new MaCrossoverStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, LongPeriod=20, ShortPeriod =10, Ticker= "BTCUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 1,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 2,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new MacdStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, FastPeriod=12, SlowPeriod =26, SignalPeriod= 9, Ticker= "ETHUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 2,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 3,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new RsiStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=14, Overbought =70, Oversold= 30, Ticker= "TRYUSDT"}),
            IsPredefined = true,
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
                var hashed = password.HashPassword(user, "admin");
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

            foreach (var tradingPair in tradingPairs)
            {
                context.TradingPairs.Add(tradingPair);
                await context.SaveChangesAsync();
            }

            foreach (var signalStrategy in signalStrategies)
            {
                context.SignalStrategies.Add(signalStrategy);
                await context.SaveChangesAsync();
            }

            var user2 = await userManager.FindByEmailAsync("zy@zcrypta.com");

            Random random = new Random(tradingPairs.Count());
            var index = random.Next();

            foreach (var strategy in signalStrategies)
            {
                context.UserSignalStrategies.Add(new UserSignalStrategy { StrategyId = strategy.Id, UserId = user2.Id });
            }
            await context.SaveChangesAsync();
        }

        private class SeedUser : User
        {
            public string[]? RoleList { get; set; }
        }
    }
}
