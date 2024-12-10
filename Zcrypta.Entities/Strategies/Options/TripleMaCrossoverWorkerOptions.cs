using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class TripleMaCrossoverWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public TripleMaCrossoverStrategyOptions StrategyOptions { get; set; }
    }
    public class TripleMaCrossoverStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int ShortPeriod { get; set; }
        public int MediumPeriod { get; set; }
        public int LongPeriod { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
