using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zcrypta.Entities.Enums.Chart
{
    public enum LineStyle
    {
        Solid = 0,
        Dotted = 1,
        Dashed = 2,
        LargeDashed = 3,
        SparseDotted = 4,
    }

    public enum ChartType
    {
        Unspecified = 0,
        Candlestick = 1,
        Line = 2,
    }
}
