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
            Base = "USDT",
            Quote = "TRY"
        },
            new TradingPair()
        {
            Base = "SUI",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "AI",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "ACA",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "ZK",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "XAI",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "STRK",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "MAV",
            Quote = "USDT"
        },
            new TradingPair()
        {
            Base = "SAGA",
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
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, LongPeriod=12, ShortPeriod =26, Period= 9, Ticker= "ETHUSDT"}),
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
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=14, Overbought =70, Oversold= 30, Ticker= "USDTTRY"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 3,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 4,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new BollingerBandsStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=20, StandardDeviations= 2, Ticker= "SUIUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 4,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 5,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new StochasticOscillatorStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=20, Overbought= 80, Oversold= 20, Ticker= "AIUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 5,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 6,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new TripleMaCrossoverStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, ShortPeriod=5, MediumPeriod= 10, LongPeriod= 20, Ticker= "ACAUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 6,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 7,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new PriceChannelStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period= 20, Ticker= "ZKUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 7,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 8,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new VolumePriceTrendStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period= 14, Ticker= "XAIUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 8,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 9,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new MomentumStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period= 10, Ticker= "STRKUSDT"}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 9,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 10,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new ExponentialMaCrossoverWithVolumeStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, ShortPeriod= 10, LongPeriod= 20, Ticker= "MAVUSDT"}),
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
