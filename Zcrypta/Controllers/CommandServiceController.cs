using Microsoft.AspNetCore.Mvc;
using Binance.Net;
using Microsoft.EntityFrameworkCore;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Zcrypta.Entities.Dtos;
using Zcrypta.Context;
using Zcrypta.Entities;
using Zcrypta.Entities.Enums;

namespace Zcrypta.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class CommandServiceController : ControllerBase
    {
        private ApplicationDbContext _context { get; }
        private IHttpContextAccessor _accessor { get; }

        private readonly ILogger<CommandServiceController> _logger;
        public CommandServiceController(ApplicationDbContext context, ILogger<CommandServiceController> logger, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor= accessor;
        }


        [HttpPost(Name = "CreateStrategy")]
        public async Task<object> CreateStrategy([FromBody] CreateStrategyRequestMessage msg)
        {
            try
            {
                _context.SignalStrategies.Add(new Models.SignalStrategy()
                {
                    CreatedBy = _accessor.HttpContext.User.Claims.ToList()[0].Value,
                    Interval = msg.Interval == 0 ? (int)KLineIntervals.OneHour : msg.Interval,
                    IsPredefined = msg.IsPredefined,
                    CreateDate = DateTime.Now,
                    StrategyType = msg.StrategyType,
                    TradingPairId = msg.TradingPair.Id,
                    Properties = msg.Properties
                });
                await _context.SaveChangesAsync();
                return new CreateStrategyResponseMessage
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new CreateStrategyResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }
    }
}
