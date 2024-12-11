using Zcrypta.Entities.Dtos;

namespace Zcrypta.Entities
{
    public class CreateStrategyRequestMessage
    {
        public string CreatedBy { get;  set; }
        public int Interval { get;  set; }
        public bool IsPredefined { get;  set; }
        public int StrategyType { get;  set; }
        public TradingPair TradingPair { get;  set; }
        public string Properties { get;  set; }
    }
    public class CreateStrategyResponseMessage: BaseResponse
    {
    }
}