namespace Zcrypta.Entities.Models.Chart
{
    public class ChartData
    {
        /// <summary>
        /// Fill this object with chart entry data such as Ohlcv or PricePoint
        /// </summary>
        public List<IChartEntry> ChartEntries { get; set; }


        /// <summary>
        /// Optional marker arrow to be displayed in addition to the primary chart data
        /// </summary>
        public List<Marker> MarkerData { get; set; }
    }
}
