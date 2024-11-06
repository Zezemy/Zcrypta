using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Dtos
{
    public class TradingSignal
    {
        public string Symbol { get; set; }
        public SignalTypes SignalType { get; set; }
    }
}