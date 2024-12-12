using Binance.Net.Interfaces.Clients;
using Zcrypta.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Zcrypta.Entities.Interfaces;
using Zcrypta.Entities.Strategies.Options;
using Zcrypta.Entities.Enums;
using Zcrypta.Context;
using Microsoft.EntityFrameworkCore;

namespace Zcrypta.BackgroundServices
{
    internal sealed class BollingerBandsSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<BollingerBandsWorkerOptions> options,
        ILogger<BollingerBandsSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly BollingerBandsWorkerOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SendSignal();

                await Task.Delay(_options.WorkInterval, stoppingToken);
            }
        }

        private async Task SendSignal()
        {
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.BollingerBands).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<BollingerBandsStrategyOptions>(strategy.Properties);
                    var ticker = props.Ticker;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.Period);
                    var closePricesLongList = kLines.Data.TakeLast(props.Period).Select(x => x.ClosePrice);
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)BollingerBandsSignal(closePricesLongList.ToList(), props.Period, props.StandardDeviations);
                    dbSignal.Symbol = ticker;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyType = (int)StrategyTypes.BollingerBands;
                    dbSignal.Interval = strategy.Interval;
                    context.TradingSignals.Add(dbSignal);
                    await context.SaveChangesAsync();

                    logger.LogInformation($"Saved {ticker} signal to {dbSignal}");
                }
                catch (Exception e)
                {
                    logger.LogError($"Error : {e}");
                }
            }
        }

        // 4. Bollinger Bands
        public static SignalTypes BollingerBandsSignal(List<decimal> prices, int period = 20, decimal standardDeviations = 2)
        {
            if (prices.Count < period) return SignalTypes.Hold;

            var sma = prices.TakeLast(period).Average();
            var std = CalculateStandardDeviation(prices.TakeLast(period).ToList());

            var upperBand = sma + (standardDeviations * std);
            var lowerBand = sma - (standardDeviations * std);
            var currentPrice = prices.Last();

            if (currentPrice < lowerBand) return SignalTypes.Buy;
            if (currentPrice > upperBand) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }

        private static decimal CalculateStandardDeviation(List<decimal> values)
        {
            var avg = values.Average();
            var sumOfSquaresOfDifferences = values.Select(val => (val - avg) * (val - avg)).Sum();
            return (decimal)Math.Sqrt((double)(sumOfSquaresOfDifferences / values.Count));
        }
    }
}