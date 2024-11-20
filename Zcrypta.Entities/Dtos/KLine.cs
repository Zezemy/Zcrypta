namespace Zcrypta.Entities.Interfaces
{
	//
	// Summary:
	//     Kline data
	public class KLine : IKLine
	{
		public DateTime OpenTime { get; set; }

		public decimal OpenPrice { get; set; }

		public decimal HighPrice { get; set; }

		public decimal LowPrice { get; set; }

		public decimal ClosePrice { get; set; }

		public decimal Volume { get; set; }

		public DateTime CloseTime { get; set; }

		public decimal QuoteVolume { get; set; }

		public int TradeCount { get; set; }

		public decimal TakerBuyBaseVolume { get; set; }

		public decimal TakerBuyQuoteVolume { get; set; }
	}
}
