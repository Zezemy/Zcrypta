using AutoMapper;
using Binance.Net.Interfaces;
using Zcrypta.Entities.Interfaces;

namespace Zcrypta.Extensions
{
	public static class KLineExtensions
	{
		public static IKLine ConvertToKLine (this IBinanceKline binanceKline)
		{
			var configuration = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<IBinanceKline, KLine>();
			});
			var mapper = configuration.CreateMapper();
			var kLine = mapper.Map<KLine>(binanceKline);
			return kLine;
		}
	}
}
