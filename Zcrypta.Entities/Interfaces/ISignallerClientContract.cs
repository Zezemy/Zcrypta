using Zcrypta.Entities.Dtos;

namespace Zcrypta.Entities.Interfaces
{
    public interface ISignallerClientContract
    {
        Task ReceiveSignalUpdate(TradingSignal signal);
    }
}