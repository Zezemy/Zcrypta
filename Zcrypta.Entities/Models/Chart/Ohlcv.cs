namespace Zcrypta.Entities.Models.Chart
{
    /// Used to display bar chart
    public class Ohlcv : IChartEntry
    {
        public DateTime Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public decimal DisplayPrice
        {
            get => Close;
        }
    }
}