namespace Zcrypta.Entities.Interfaces
{
	//
	// Summary:
	//     Kline data
	public interface IKLine
	{
		//
		// Summary:
		//     The time this candlestick opened
		DateTime OpenTime { get; set; }

		//
		// Summary:
		//     The price at which this candlestick opened
		decimal OpenPrice { get; set; }

		//
		// Summary:
		//     The highest price in this candlestick
		decimal HighPrice { get; set; }

		//
		// Summary:
		//     The lowest price in this candlestick
		decimal LowPrice { get; set; }

		//
		// Summary:
		//     The price at which this candlestick closed
		decimal ClosePrice { get; set; }

		//
		// Summary:
		//     The volume traded during this candlestick
		decimal Volume { get; set; }

		//
		// Summary:
		//     The close time of this candlestick
		DateTime CloseTime { get; set; }

		//
		// Summary:
		//     The volume traded during this candlestick in the asset form
		decimal QuoteVolume { get; set; }

		//
		// Summary:
		//     The amount of trades in this candlestick
		int TradeCount { get; set; }

		//
		// Summary:
		//     Taker buy base asset volume
		decimal TakerBuyBaseVolume { get; set; }

		//
		// Summary:
		//     Taker buy quote asset volume
		decimal TakerBuyQuoteVolume { get; set; }
	}
}
