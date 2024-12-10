using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class PriceChannelWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public PriceChannelStrategyOptions StrategyOptions { get; set; }
    }
    public class PriceChannelStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
