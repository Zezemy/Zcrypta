using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class BollingerBandsWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public BollingerBandsStrategyOptions StrategyOptions { get; set; }
    }

    public class BollingerBandsStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int Period { get; set; }
        public int StandardDeviations { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
