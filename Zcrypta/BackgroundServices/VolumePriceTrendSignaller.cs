using Binance.Net.Interfaces.Clients;
using Zcrypta.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Zcrypta.Entities.Interfaces;
using Zcrypta.Entities.Strategies.Options;
using Zcrypta.Extensions;
using Zcrypta.Entities.Enums;
using Zcrypta.Context;
using Microsoft.EntityFrameworkCore;

namespace Zcrypta.BackgroundServices
{
    internal sealed class VolumePriceTrendSignaller(
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
				await SendSignal();

				await Task.Delay(_options.WorkInterval, stoppingToken);
			}
		}

		private async Task SendSignal()
		{
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.VolumePriceTrend).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
			{
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<VolumePriceTrendStrategyOptions>(strategy.Properties);
                    var ticker = props.Ticker;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.Period);
                    var closePricesLongList = kLines.Data.TakeLast(props.Period).Select(x => x.ConvertToKLine());
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)VolumePriceTrendSignal(closePricesLongList.ToList(), props.Period);
                    dbSignal.Symbol = ticker;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyType = (int)StrategyTypes.VolumePriceTrend;
                    dbSignal.Interval = (int)KLineIntervals.OneMinute;
                    context.TradingSignals.Add(dbSignal);
                    await context.SaveChangesAsync();

                    logger.LogInformation($"Saved {ticker} signal to {dbSignal}");
                }
                catch (Exception e)
                {
                    logger.LogError($"Error : {e}");
                }
            }
		}

        // 8. Volume Price Trend
        public static SignalTypes VolumePriceTrendSignal(List<IKLine> prices, int period)
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