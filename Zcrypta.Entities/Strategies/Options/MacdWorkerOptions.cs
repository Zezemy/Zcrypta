using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class MacdWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public MacdStrategyOptions StrategyOptions { get; set; }
    }
    public class MacdStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int FastPeriod { get; set; }
        public int SlowPeriod { get; set; }
        public int SignalPeriod { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
