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
	internal sealed class MacdSignaller(
		IServiceScopeFactory serviceScopeFactory,
		IHubContext<MacdFeedHub, ISignallerClientContract> hubContext,
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
				await UpdateStockPrices();

				await Task.Delay(_options.WorkInterval, stoppingToken);
			}
		}

		private async Task UpdateStockPrices()
		{
			var ticker = _options.Ticker;
			var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, Binance.Net.Enums.KlineInterval.OneMinute, limit:20);
			var closePricesLongList = kLines.Data.TakeLast(20).Select(x => x.ClosePrice);
			var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
			//DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
			//DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
			TradingSignal signal = new TradingSignal();
			signal.SignalType = MACDSignal(closePricesLongList.ToList());
			signal.Symbol = ticker;
			signal.DateTime = latestCloseTime;

			//await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

			await hubContext.Clients.Group(ticker).ReceiveSignalUpdate(signal);

			logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
		}

		private decimal CalculateNewPrice(decimal currentPrice)
		{
			double change = 0.02;
			decimal priceFactor = (decimal)(_random.NextDouble() * change * 2 - change);
			decimal priceChange = currentPrice * priceFactor;
			decimal newPrice = Math.Max(0, currentPrice + priceChange);
			newPrice = Math.Round(newPrice, 2);
			return newPrice;
		}

        // 3. MACD (Moving Average Convergence Divergence)
        public static SignalTypes MACDSignal(List<decimal> prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            if (prices.Count < slowPeriod) return SignalTypes.Hold;

            var fastEMA = CalculateEMA(prices, fastPeriod);
            var slowEMA = CalculateEMA(prices, slowPeriod);
            var macd = fastEMA - slowEMA;
            var signal = CalculateEMA(new List<decimal> { macd }, signalPeriod);

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