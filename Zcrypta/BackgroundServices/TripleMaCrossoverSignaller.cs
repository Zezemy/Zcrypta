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

namespace Zcrypta.BackgroundServices
{
    internal sealed class TripleMaCrossoverSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<TripleMaCrossoverWorkerOptions> options,
        ILogger<TripleMaCrossoverSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly TripleMaCrossoverWorkerOptions _options = options.Value;

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
            foreach (string ticker in signalTickerManager.GetAllTickers())
            {
                //var ticker = _options.Ticker;
                var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, Binance.Net.Enums.KlineInterval.OneMinute, limit: 20);
                var closePricesLongList = kLines.Data.TakeLast(20).Select(x => x.ClosePrice);
                var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
                //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
                TradingSignal signal = new TradingSignal();
                signal.SignalType = TripleMovingAverageCrossover(closePricesLongList);
                signal.Symbol = ticker;
                signal.DateTime = latestCloseTime;
                signal.StrategyType = StrategyTypes.TripleMaCrossover;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.TripleMaCrossover).ReceiveSignalUpdate(signal);

                logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
            }
        }

        // 6. Triple Moving Average Crossover
        public static SignalTypes TripleMovingAverageCrossover(IEnumerable<decimal> prices, int shortPeriod = 5, int mediumPeriod = 10, int longPeriod = 20)
        {
            if (prices.Count() < longPeriod) return SignalTypes.Hold;

            var shortMA = prices.TakeLast(shortPeriod).Average();
            var mediumMA = prices.TakeLast(mediumPeriod).Average();
            var longMA = prices.TakeLast(longPeriod).Average();

            if (shortMA > mediumMA && mediumMA > longMA) return SignalTypes.Buy;
            if (shortMA < mediumMA && mediumMA < longMA) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}