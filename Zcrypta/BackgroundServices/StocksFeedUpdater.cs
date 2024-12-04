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

namespace Zcrypta.BackgroundServices
{
    internal sealed class StocksFeedUpdater(
        ActiveTickerManager activeTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<StocksFeedHub, IPriceUpdateClientContract> hubContext,
        IOptions<UpdateOptions> options,
        ILogger<StocksFeedUpdater> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly UpdateOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStockPrices();

                await Task.Delay(_options.UpdateInterval, stoppingToken);
            }
        }

        private async Task UpdateStockPrices()
        {
            foreach (string ticker in activeTickerManager.GetAllTickers())
            {
                var priceData = await restClient.SpotApi.ExchangeData.GetPriceAsync(ticker);
                if (priceData == null)
                {
                    continue;
                }

                var update = new CurrentPrice() { Price = priceData.Data.Price, Symbol = ticker };

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update);

                logger.LogInformation($"Updated {ticker} price to {priceData?.Data?.Price}");
            }
        }
    }
}