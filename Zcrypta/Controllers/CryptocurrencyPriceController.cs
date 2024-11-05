using Microsoft.AspNetCore.Mvc;
using Binance.Net;
using Microsoft.EntityFrameworkCore;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Zcrypta.Entities.Dtos;

namespace Zcrypta.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CryptocurrencyPriceController : ControllerBase
    {
        private IBinanceRestClient _restClient { get; }
        public CryptocurrencyPriceController(IBinanceRestClient restClient)
        {
            _restClient = restClient;
        }


        [HttpGet(Name = "GetStockPrice")]
        public async Task<object> GetAsync(string? ticker)
        {
            try
            {
                ticker = ticker ?? "BTCUSDT";
                var price = await _restClient.SpotApi.ExchangeData.GetPriceAsync(ticker);
                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(price.Data);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<CurrentPrice>(jsonStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
