﻿using Binance.Net.Interfaces.Clients;
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
    internal sealed class RsiSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        IOptions<RsiWorkerOptions> options,
        ILogger<RsiSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly RsiWorkerOptions _options = options.Value;

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
                signal.SignalType = RSISignal(closePricesLongList.ToList());
                signal.Symbol = ticker;
                signal.DateTime = latestCloseTime;
                signal.StrategyType = StrategyTypes.Rsi;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.Rsi).ReceiveSignalUpdate(signal);

                logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
            }
        }

        // 2. RSI (Relative Strength Index)
        public static SignalTypes RSISignal(List<decimal> prices, int period = 14, decimal overbought = 70, decimal oversold = 30)
        {
            if (prices.Count < period + 1) return SignalTypes.Hold;

            var gains = new List<decimal>();
            var losses = new List<decimal>();

            for (int i = 1; i < prices.Count; i++)
            {
                var difference = prices[i] - prices[i - 1];
                gains.Add(difference > 0 ? difference : 0);
                losses.Add(difference < 0 ? -difference : 0);
            }

            var avgGain = gains.TakeLast(period).Average();
            var avgLoss = losses.TakeLast(period).Average();

            if (avgLoss == 0) return SignalTypes.Hold;

            var rs = avgGain / avgLoss;
            var rsi = 100 - (100 / (1 + rs));

            if (rsi < oversold) return SignalTypes.Buy;
            if (rsi > overbought) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}