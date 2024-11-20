using Zcrypta.Managers;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using Zcrypta.Entities.Interfaces;

namespace Zcrypta.Hubs
{
    internal sealed class BollingerBandsFeedHub() : Hub<ISignallerClientContract>
    {
        public async Task JoinStockGroup(string ticker)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
        }
    };
}