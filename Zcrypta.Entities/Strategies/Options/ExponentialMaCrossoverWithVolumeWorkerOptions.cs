using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class ExponentialMaCrossoverWithVolumeWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public ExponentialMaCrossoverWithVolumeStrategyOptions StrategyOptions { get; set; }
    }
    public class ExponentialMaCrossoverWithVolumeStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int ShortPeriod { get; set; }
        public int LongPeriod { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
