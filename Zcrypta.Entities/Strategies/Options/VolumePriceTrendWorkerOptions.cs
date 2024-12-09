using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zcrypta.Entities.Strategies.Options
{
    public class VolumePriceTrendWorkerOptions
    {
        public string Ticker { get; set; } = "XAIUSDT";
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan KLineInterval { get; set; } = TimeSpan.FromMinutes(15);
    }
}
