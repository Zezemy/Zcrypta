using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zcrypta.Entities.Models.Chart
{
    public interface IChartEntry
    {
        public DateTime Time { get; set; }
        public decimal Volume { get; set; }
        public decimal DisplayPrice { get; }
    }
}
