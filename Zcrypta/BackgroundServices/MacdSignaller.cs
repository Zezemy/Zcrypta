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
    internal sealed class MacdSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<MacdWorkerOptions> options,
        ILogger<MacdSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly MacdWorkerOptions _options = options.Value;

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
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.Macd).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<MacdStrategyOptions>(strategy.Properties);
                    var ticker = props.Ticker;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.ShortPeriod);
                    var closePricesLongList = kLines.Data.TakeLast(props.ShortPeriod).Select(x => x.ClosePrice);
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)MACDSignal(closePricesLongList.ToList(), props.LongPeriod, props.ShortPeriod, props.Period);
                    dbSignal.Symbol = ticker;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyType = (int)StrategyTypes.Macd;
                    dbSignal.Interval = (int)KLineIntervals.OneMinute;
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

        // 3. MACD (Moving Average Convergence Divergence)
        public static SignalTypes MACDSignal(List<decimal> prices, int longPeriod, int shortPeriod, int period)
        {
            if (prices.Count < shortPeriod) return SignalTypes.Hold;

            var fastEMA = CalculateEMA(prices, longPeriod);
            var slowEMA = CalculateEMA(prices, shortPeriod);
            var macd = fastEMA - slowEMA;
            var signal = CalculateEMA(new List<decimal> { macd }, period);

            if (macd > signal) return SignalTypes.Buy;
            if (macd < signal) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }

        private static decimal CalculateEMA(List<decimal> prices, int period)
        {
            var multiplier = 2.0m / (period + 1);
            var ema = prices.Take(period).Average();

            foreach (var price in prices.Skip(period))
            {
                ema = (price - ema) * multiplier + ema;
            }
            return ema;
        }
    }
}