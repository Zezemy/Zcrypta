using Zcrypta.Managers;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using Zcrypta.Entities.Interfaces;

namespace Zcrypta.Hubs
{
    internal sealed class StocksFeedHub(ActiveTickerManager activeTickerManager) : Hub<IPriceUpdateClientContract>
    {
        public async Task JoinStockGroup(string ticker)
        {
            activeTickerManager.AddTicker(ticker);
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
        }
    };
}