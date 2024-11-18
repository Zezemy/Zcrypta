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

namespace Zcrypta.BackgroundServices
{
    internal sealed class MaCrossoverSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<MaCrossoverFeedHub, ISignallerClientContract> hubContext,
        IOptions<MaCrossoverWorkerOptions> options,
        ILogger<MaCrossoverSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly MaCrossoverWorkerOptions _options = options.Value;

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
            var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, Binance.Net.Enums.KlineInterval.OneMinute);
            var closePricesShort = kLines.Data.TakeLast(10).Select(x => x.ClosePrice).Average();
            var closePricesLong = kLines.Data.TakeLast(20).Select(x => x.ClosePrice).Average();
            var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime).FirstOrDefault();
            //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
            //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;
            TradingSignal signal= new TradingSignal();
            signal.SignalType = Entities.Enums.SignalTypes.Hold;
            signal.Symbol = ticker;
            signal.DateTime = latestCloseTime;
            if (closePricesShort > closePricesLong)
            {
                signal.SignalType = Entities.Enums.SignalTypes.Buy;
            }
            else
            {
                signal.SignalType = Entities.Enums.SignalTypes.Sell;
            }

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
    }
}