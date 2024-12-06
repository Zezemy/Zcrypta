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
                var price = await restClient.SpotApi.ExchangeData.GetPriceAsync(ticker);
                if (price == null)
                {
                    continue;
                }

                decimal newPrice = CalculateNewPrice(price.Data.Price);

                var update = new CurrentPrice() { Price = newPrice, Symbol = ticker };

                //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update);

                logger.LogInformation("Updated {Ticker} price to {Price}", ticker, newPrice);
            }
        }

        private decimal CalculateNewPrice(decimal currentPrice)
        {
            double change = _options.MaxPercentageChange;
            decimal priceFactor = (decimal)(_random.NextDouble() * change * 2 - change);
            decimal priceChange = currentPrice * priceFactor;
            decimal newPrice = Math.Max(0, currentPrice + priceChange);
            newPrice = Math.Round(newPrice, 2);
            return newPrice;
        }
    }
}