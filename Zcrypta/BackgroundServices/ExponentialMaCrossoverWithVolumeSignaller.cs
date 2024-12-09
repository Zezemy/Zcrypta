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
using System.Linq;

namespace Zcrypta.BackgroundServices
{
	internal sealed class ExponentialMaCrossoverWithVolumeSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
		IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
		IOptions<ExponentialMaCrossoverWithVolumeWorkerOptions> options,
		ILogger<ExponentialMaCrossoverWithVolumeSignaller> logger,
		IBinanceRestClient restClient)
		: BackgroundService
	{
		private readonly Random _random = new();
		private readonly ExponentialMaCrossoverWithVolumeWorkerOptions _options = options.Value;

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
				signal.SignalType = EMAVolumeSignal(kLines);
				signal.Symbol = ticker;
				signal.DateTime = latestCloseTime;
				signal.StrategyType = StrategyTypes.ExponentialMaCrossoverWithVolume;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.ExponentialMaCrossoverWithVolume).ReceiveSignalUpdate(signal);

				logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
			}
		}

        // 10. Exponential Moving Average Crossover with Volume
        public static SignalTypes EMAVolumeSignal(IEnumerable<IKLine> prices, int shortPeriod = 10, int longPeriod = 20)
        {
            if (prices.Count() < longPeriod) return SignalTypes.Hold;

            var closePrices = prices.Select(p => p.ClosePrice).ToList();
            var volumes = prices.Select(p => p.Volume).ToList();

            var shortEMA = CalculateEMA(closePrices, shortPeriod);
            var longEMA = CalculateEMA(closePrices, longPeriod);
            var averageVolume = volumes.TakeLast(shortPeriod).Average();
            var currentVolume = volumes.Last();

            if (shortEMA > longEMA && currentVolume > averageVolume) return SignalTypes.Buy;
            if (shortEMA < longEMA && currentVolume > averageVolume) return SignalTypes.Sell;
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