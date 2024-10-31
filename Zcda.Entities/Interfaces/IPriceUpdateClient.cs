using Zcda.Entities.Dtos;

namespace Zcda.Entities.Interfaces
{
    public interface IPriceUpdateClient
    {
        Task ReceiveStockPriceUpdate(CurrentPrice update);
    }
}