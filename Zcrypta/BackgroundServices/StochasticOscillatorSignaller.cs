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
	internal sealed class StochasticOscillatorSignaller(
		IServiceScopeFactory serviceScopeFactory,
		IHubContext<StochasticOscillatorFeedHub, ISignallerClientContract> hubContext,
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
				await UpdateStockPrices();

				await Task.Delay(_options.WorkInterval, stoppingToken);
			}
		}

		private async Task UpdateStockPrices()
		{
			var ticker = _options.Ticker;
			var binanceKLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, Binance.Net.Enums.KlineInterval.OneMinute, limit:20);
			var kLines = binanceKLines.Data.TakeLast(20).Select(x => x.ConvertToKLine());
			var latestCloseTime = binanceKLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
			//DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
			//DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
			TradingSignal signal = new TradingSignal();
			signal.SignalType = StochasticSignal(kLines);
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