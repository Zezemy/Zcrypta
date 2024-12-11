using Microsoft.AspNetCore.Mvc;
using Binance.Net;
using Microsoft.EntityFrameworkCore;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Zcrypta.Entities.Dtos;
using Zcrypta.Context;

namespace Zcrypta.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class QueryServiceController : ControllerBase
    {
        private ApplicationDbContext _context { get; }
        public QueryServiceController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet(Name = "GetTradingPairs")]
        public async Task<object> GetTradingPairsAsync()
        {
            try
            {
                return _context.TradingPairs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
