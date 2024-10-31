using Zcda.Entities.Dto;

namespace Zcda.Entities.Interfaces
{
    public interface IPriceUpdateClient
    {
        Task ReceiveStockPriceUpdate(CurrentPrice update);
    }
}