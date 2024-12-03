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
	internal sealed class VolumePriceTrendSignaller(
        SignalTickerManager signalTickerManager,
        IServiceScopeFactory serviceScopeFactory,
		IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
		IOptions<VolumePriceTrendWorkerOptions> options,
		ILogger<VolumePriceTrendSignaller> logger,
		IBinanceRestClient restClient)
		: BackgroundService
	{
		private readonly Random _random = new();
		private readonly VolumePriceTrendWorkerOptions _options = options.Value;

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
				var kLines = binanceKLines.Data.TakeLast(20).Select(x => x.ConvertToKLine()).ToList();
				var latestCloseTime = binanceKLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
				//DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
				//DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
				TradingSignal signal = new TradingSignal();
				signal.SignalType = VolumePriceTrendSignal(kLines);
				signal.Symbol = ticker;
				signal.DateTime = latestCloseTime;
				signal.StrategyType = StrategyTypes.VolumePriceTrend;
                signal.Interval = KLineIntervals.OneMinute;

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker + StrategyTypes.VolumePriceTrend).ReceiveSignalUpdate(signal);

				logger.LogInformation("Updated {ticker} signal to {signal}", ticker, signal);
			}
		}

        // 8. Volume Price Trend
        public static SignalTypes VolumePriceTrendSignal(List<IKLine> prices, int period = 14)
        {
            if (prices.Count() < period + 1) return SignalTypes.Hold;

            var vpt = 0m;
            for (int i = 1; i < prices.Count(); i++)
            {
                var priceChange = (prices[i].ClosePrice - prices[i - 1].ClosePrice) / prices[i - 1].ClosePrice;
                vpt += priceChange * prices[i].Volume;
            }

            var previousVPT = 0m;
            for (int i = 1; i < prices.Count() - 1; i++)
            {
                var priceChange = (prices[i].ClosePrice - prices[i - 1].ClosePrice) / prices[i - 1].ClosePrice;
                previousVPT += priceChange * prices[i].Volume;
            }

            if (vpt > previousVPT) return SignalTypes.Buy;
            if (vpt < previousVPT) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}