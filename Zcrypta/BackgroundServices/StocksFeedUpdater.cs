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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Zcrypta.Context;

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
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tradingPairs = context.TradingPairs.ToList();

            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStockPrices(tradingPairs);

                await Task.Delay(_options.UpdateInterval, stoppingToken);
            }
        }

        private async Task UpdateStockPrices(List<Models.TradingPair> tradingPairs)
        {
            try
            {
                var sendedTickerList = new List<string>();
                foreach (var pair in tradingPairs)
                {
                    var ticker = $"{pair.Base}{pair.Quote}";
                    sendedTickerList.Add(ticker);
                    await SendTradingPairPrice(hubContext, logger, restClient, tradingPairs, ticker);
                }

                var remainingTickerList = activeTickerManager.GetAllTickers().ToList().Except(sendedTickerList);
                foreach (var ticker in remainingTickerList)
                {
                    await SendTradingPairPrice(hubContext, logger, restClient, tradingPairs, ticker);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e}");
            }
        }

        private static async Task SendTradingPairPrice(IHubContext<StocksFeedHub, IPriceUpdateClientContract> hubContext, ILogger<StocksFeedUpdater> logger, IBinanceRestClient restClient, List<Models.TradingPair> tradingPairs, string ticker)
        {
            try
            {
                var tradingPairDb = tradingPairs.Where(x => x.Base + x.Quote == ticker).FirstOrDefault();
                var tradingPair = tradingPairDb == null ? new TradingPair { Base = ticker } : new TradingPair { Base = tradingPairDb.Base, Quote = tradingPairDb.Quote };

                var priceData = await restClient.SpotApi.ExchangeData.GetTradingDayTickerAsync(ticker);
                if (priceData != null)
                {
                    var update = new TradingDayTicker()
                    {
                        LastPrice = priceData.Data.LastPrice,
                        Symbol = ticker,
                        PriceChange = priceData.Data.PriceChange,
                        PriceChangePercent = priceData.Data.PriceChangePercent,
                        WeightedAveragePrice = priceData.Data.WeightedAveragePrice,
                        OpenPrice = priceData.Data.OpenPrice,
                        HighPrice = priceData.Data.HighPrice,
                        LowPrice = priceData.Data.LowPrice,
                        Volume = priceData.Data.Volume,
                        QuoteVolume = priceData.Data.QuoteVolume,
                        OpenTime = priceData.Data.OpenTime,
                        CloseTime = priceData.Data.CloseTime,
                        FirstTradeId = priceData.Data.FirstTradeId,
                        TotalTrades = priceData.Data.TotalTrades,
                        TradingPair = tradingPair
                    };

                    //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                    await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                    logger.LogInformation($"Updated {ticker} price to {priceData?.Data?.LastPrice}");
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e}");
            }
        }
    }
}