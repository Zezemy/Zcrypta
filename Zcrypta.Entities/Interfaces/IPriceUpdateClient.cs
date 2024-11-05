using Zcrypta.Entities.Dtos;

namespace Zcrypta.Entities.Interfaces
{
    public interface IPriceUpdateClient
    {
        Task ReceiveStockPriceUpdate(CurrentPrice update);
    }
}