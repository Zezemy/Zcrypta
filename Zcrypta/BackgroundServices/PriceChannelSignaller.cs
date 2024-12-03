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
using Binance.Net.Interfaces;

namespace Zcrypta.BackgroundServices
{
    internal sealed class PriceChannelSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<PriceChannelWorkerOptions> options,
        ILogger<PriceChannelSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly PriceChannelWorkerOptions _options = options.Value;

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
                var binanceKLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, Binance.Net.Enums.KlineInterval.OneMinute, limit: 20);
                var kLines = binanceKLines.Data.TakeLast(20).Select(x => x.ConvertToKLine());
                var latestCloseTime = binanceKLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
                //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
                TradingSignal signal = new TradingSignal();
                signal.SignalType = PriceChannelSignal(kLines);
                signal.Symbol = ticker;
                signal.DateTime = latestCloseTime;
                signal.StrategyType = StrategyTypes.PriceChannel;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.PriceChannel).ReceiveSignalUpdate(signal);

                logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
            }
        }

        // 7. Price Channel Strategy
        public static SignalTypes PriceChannelSignal(IEnumerable<IKLine> prices, int period = 20)
        {
            if (prices.Count() < period) return SignalTypes.Hold;

            var recentPrices = prices.TakeLast(period).ToList();
            var upperChannel = recentPrices.Max(p => p.HighPrice);
            var lowerChannel = recentPrices.Min(p => p.LowPrice);
            var currentPrice = prices.Last().ClosePrice;

            if (currentPrice >= upperChannel) return SignalTypes.Sell;
            if (currentPrice <= lowerChannel) return SignalTypes.Buy;
            return SignalTypes.Hold;
        }
    }
}