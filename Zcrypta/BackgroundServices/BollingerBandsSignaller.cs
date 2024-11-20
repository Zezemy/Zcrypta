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
	internal sealed class BollingerBandsSignaller(
		IServiceScopeFactory serviceScopeFactory,
		IHubContext<BollingerBandsFeedHub, ISignallerClientContract> hubContext,
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
			signal.SignalType = BollingerBandsSignal(closePricesLongList.ToList());
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