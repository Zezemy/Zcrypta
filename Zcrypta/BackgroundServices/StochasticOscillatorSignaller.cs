using Binance.Net.Interfaces.Clients;
using Zcrypta.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Zcrypta.Entities.Interfaces;
using Zcrypta.Entities.Strategies.Options;
using Zcrypta.Extensions;
using Zcrypta.Entities.Enums;
using Zcrypta.Context;
using Microsoft.EntityFrameworkCore;

namespace Zcrypta.BackgroundServices
{
    internal sealed class StochasticOscillatorSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<StochasticOscillatorWorkerOptions> options,
        ILogger<StochasticOscillatorSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly StochasticOscillatorWorkerOptions _options = options.Value;

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
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.StochasticOscillator).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<StochasticOscillatorStrategyOptions>(strategy.Properties);
                    var ticker = props.Ticker;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.Period);
                    var closePricesLongList = kLines.Data.TakeLast(props.Period).Select(x => x.ConvertToKLine());
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)StochasticSignal(closePricesLongList, props.Period, props.Overbought, props.Oversold);
                    dbSignal.Symbol = ticker;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyType = (int)StrategyTypes.StochasticOscillator;
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

        // 5. Stochastic Oscillator
        public static SignalTypes StochasticSignal(IEnumerable<IKLine> prices, int period = 14, decimal overbought = 80, decimal oversold = 20)
        {
            if (prices.Count() < period) return SignalTypes.Hold;

            var recentPrices = prices.TakeLast(period).ToList();
            var highestHigh = recentPrices.Max(p => p.HighPrice);
            var lowestLow = recentPrices.Min(p => p.LowPrice);

            if (highestHigh - lowestLow == 0) return SignalTypes.Hold;

            var k = ((prices.Last().ClosePrice - lowestLow) / (highestHigh - lowestLow)) * 100;

            if (k < oversold) return SignalTypes.Buy;
            if (k > overbought) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}