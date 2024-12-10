using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class MomentumWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public MomentumStrategyOptions StrategyOptions { get; set; }
    }
    public class MomentumStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
