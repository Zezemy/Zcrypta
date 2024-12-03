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
    internal sealed class MomentumSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<MomentumWorkerOptions> options,
        ILogger<MomentumSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly MomentumWorkerOptions _options = options.Value;

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
                var closePricesLongList = kLines.Data.TakeLast(20).Select(x => x.ClosePrice).ToList();
                var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
                //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
                TradingSignal signal = new TradingSignal();
                signal.SignalType = MomentumSignal(closePricesLongList);
                signal.Symbol = ticker;
                signal.DateTime = latestCloseTime;
                signal.StrategyType = StrategyTypes.Momentum;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.Momentum).ReceiveSignalUpdate(signal);

                logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
            }
        }

        // 9. Momentum Strategy
        public static SignalTypes MomentumSignal(List<decimal> prices, int period = 10)
        {
            if (prices.Count() < period) return SignalTypes.Hold;

            var momentum = prices.Last() - prices[prices.Count() - period];
            var previousMomentum = prices[prices.Count() - 2] - prices[prices.Count() - period - 1];

            if (momentum > previousMomentum && momentum > 0) return SignalTypes.Buy;
            if (momentum < previousMomentum && momentum < 0) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}