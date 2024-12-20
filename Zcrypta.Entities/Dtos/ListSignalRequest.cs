using Zcrypta.Entities.Dtos;
using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities
{
    public class ListSignalRequest
    {
        public string Symbol { get;  set; }
        public int SignalType { get;  set; }
        public int Interval { get;  set; }
        public int StrategyType { get;  set; }
        public DateTime QueryStartDateTime { get;  set; }
        public DateTime QueryEndDateTime { get;  set; }
    }
    public class ListSignalResponseMessage : BaseResponse
    {
        public List<TradingSignal> TradingSignals { get; set; } = new List<TradingSignal>();
    }
}