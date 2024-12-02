using Zcrypta.Managers;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using Zcrypta.Entities.Interfaces;
using Zcrypta.Entities.Enums;

namespace Zcrypta.Hubs
{
    internal sealed class TradingSignalSenderHub(SignalTickerManager signalTickerManager) : Hub<ISignallerClientContract>
    {
        public async Task JoinStockGroup(string ticker, StrategyTypes strategyType)
        {
            signalTickerManager.AddTicker(ticker);
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker + strategyType);
        }
    };
}