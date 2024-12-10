using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class VolumePriceTrendWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public VolumePriceTrendStrategyOptions StrategyOptions { get; set; }
    }
    public class VolumePriceTrendStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
