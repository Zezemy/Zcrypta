using Binance.Net.Interfaces.Clients;
using Zcrypta.Hubs;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Zcrypta.Entities.Dtos;
using Zcrypta.Entities.Interfaces;
using Zcrypta.Managers;
using Zcrypta.Entities.BackgroundServices;
using Zcrypta.Entities.Strategies.Options;
using Zcrypta.Extensions;
using Zcrypta.Entities.Enums;
using Zcrypta.Models;
using Zcrypta.Context;
using Microsoft.EntityFrameworkCore;

namespace Zcrypta.BackgroundServices
{
    internal sealed class MaCrossoverSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<MaCrossoverWorkerOptions> options,
        ILogger<MaCrossoverSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly MaCrossoverWorkerOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStockPrices();

                await Task.Delay(_options.WorkInterval, stoppingToken);
            }
        }

        private async Task UpdateStockPrices()
        {
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.MaCrossover).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                var props = Newtonsoft.Json.JsonConvert.DeserializeObject<MaCrossoverStrategyOptions>(strategy.Properties);
                var ticker = props.Ticker;
                var kLineInterval = (Binance.Net.Enums.KlineInterval) Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.LongPeriod);
                var closePricesLongList = kLines.Data.TakeLast(props.LongPeriod).Select(x => x.ClosePrice);
                var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
                //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
                Entities.Dtos.TradingSignal signal = new Entities.Dtos.TradingSignal();
                signal.SignalType = MovingAverageCrossover(closePricesLongList, props.ShortPeriod, props.LongPeriod);
                signal.Symbol = ticker;
                signal.DateTime = latestCloseTime;
                signal.StrategyType = StrategyTypes.MaCrossover;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.MaCrossover).ReceiveSignalUpdate(signal);

                logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);

                Models.TradingSignal dbSignal = new Models.TradingSignal();
                dbSignal.SignalType = (int) signal.SignalType;
                dbSignal.Symbol = ticker;
                dbSignal.DateTime = latestCloseTime;
                dbSignal.StrategyType = (int) StrategyTypes.MaCrossover;
                dbSignal.Interval = (int) KLineIntervals.OneMinute;
                context.TradingSignals.Add(dbSignal);
                await context.SaveChangesAsync();
            }
        }

        // 1. Simple Moving Average Crossover
        public static SignalTypes MovingAverageCrossover(IEnumerable<decimal> prices, int shortPeriod, int longPeriod)
        {
            if (prices.Count() < longPeriod) return SignalTypes.Hold;

            var shortMA = prices.TakeLast(shortPeriod).Average();
            var longMA = prices.TakeLast(longPeriod).Average();

            if (shortMA > longMA) return SignalTypes.Buy;
            if (shortMA < longMA) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}