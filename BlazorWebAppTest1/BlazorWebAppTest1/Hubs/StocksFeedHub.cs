using BlazorWebAppTest1.Manager;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using Zcda.Entities.Interfaces;

namespace BlazorWebAppTest1.Hubs
{
    internal sealed class StocksFeedHub(ActiveTickerManager activeTickerManager) : Hub<IPriceUpdateClient>
    {
        public async Task JoinStockGroup(string ticker)
        {
            activeTickerManager.AddTicker(ticker);
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
        }
    };
}