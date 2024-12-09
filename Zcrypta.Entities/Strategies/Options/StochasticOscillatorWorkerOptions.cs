using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class StochasticOscillatorWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public StochasticOscillatorStrategyOptions StrategyOptions { get; set; }
    }
    public class StochasticOscillatorStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int Period { get; set; }
        public int Overbought { get; set; }
        public int Oversold { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
