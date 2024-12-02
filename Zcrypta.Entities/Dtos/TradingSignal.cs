using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Dtos
{
    public class TradingSignal
    {
        public string Symbol { get; set; }
        public SignalTypes SignalType { get; set; }
        public DateTime DateTime { get; set; }
        public StrategyTypes StrategyType { get; set; }
        public KLineIntervals Interval { get; set; }
    }
}