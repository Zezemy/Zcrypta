using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zcrypta.Entities.Enums;

namespace Zcrypta.Entities.Strategies.Options
{
    public class RsiWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public RsiStrategyOptions StrategyOptions { get; set; }
    }
    public class RsiStrategyOptions : StrategyOptions
    {
        public string Ticker { get; set; }
        public int Period { get; set; }
        public int Overbought { get; set; }
        public int Oversold { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
