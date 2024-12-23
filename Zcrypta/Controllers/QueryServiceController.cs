using Microsoft.AspNetCore.Mvc;
using Binance.Net;
using Microsoft.EntityFrameworkCore;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Zcrypta.Entities.Dtos;
using Zcrypta.Context;
using Zcrypta.Entities.Enums;
using Zcrypta.Entities;
using Zcrypta.BackgroundServices;

namespace Zcrypta.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class QueryServiceController : ControllerBase
    {
        private ApplicationDbContext _context { get; }

        private ILogger<QueryServiceController> _logger;

        public QueryServiceController(ApplicationDbContext context, ILogger<QueryServiceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetUserById")]
        public async Task<object> GetUserByIdAsync(string id)
        {
            try
            {
                return _context.Users.Where(x => x.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetUsers")]
        public async Task<object> GetUsersAsync()
        {
            try
            {
                return _context.Users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        [HttpPost(Name = "SignalsList")]
        public async Task<object> SignalsListAsync()
        {
            try
            {
                return _context.TradingSignals;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost(Name = "ListSignals")]
        public async Task<ListSignalResponseMessage> ListSignals([FromBody] ListSignalRequest msg)
        {
            var ret = new ListSignalResponseMessage();
            try
            {
                var listSignals = _context.TradingSignals.Where(
                    x => x.DateTime <= msg.QueryEndDateTime
                      && x.DateTime >= msg.QueryStartDateTime
                      ).OrderByDescending(x=>x.Id).ToList();

                var signals = new List<TradingSignal>();
                if (msg.StrategyType >= 0)
                    listSignals = listSignals.Where(x => x.StrategyType == msg.StrategyType).ToList();
                if (msg.SignalType >= 0)
                    listSignals = listSignals.Where(x => x.SignalType == msg.SignalType).ToList();
                if (msg.Interval >= 0)
                    listSignals = listSignals.Where(x => x.Interval == msg.Interval).ToList();
                if (!string.IsNullOrWhiteSpace(msg.Symbol))
                    listSignals = listSignals.Where(x => x.Symbol == msg.Symbol).ToList();

                foreach (var signal in listSignals)
                {
                    ret.TradingSignals.Add(new TradingSignal()
                    {
                        DateTime = signal.DateTime,
                        Interval = (KLineIntervals)Enum.Parse(typeof(KLineIntervals), signal.Interval.ToString()),
                        SignalType = (SignalTypes)Enum.Parse(typeof(SignalTypes), signal.SignalType.ToString()),
                        Symbol = signal.Symbol,
                        StrategyType = (StrategyTypes)Enum.Parse(typeof(StrategyTypes), signal.StrategyType.ToString()),
                    });
                }
                ret.ResponseCode = "0";
                ret.ResponseDescription = "Transaction is successful.";
                return ret;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ListSignalResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }
    }
}
