//using Zcrypta.Context;
//using Microsoft.AspNetCore.Mvc;
//using Zcrypta.Entities.Dtos;

//namespace Zcrypta.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ChatController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        private readonly ILogger<ChatController> _logger;

//        public ChatController(ILogger<ChatController> logger, ApplicationDbContext context)
//        {
//            _logger = logger;
//            _context = context;
//        }

//        [HttpGet(Name = "GetMessages")]
//        public async Task<SearchResult> GetAsync(string? message)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(message))
//                {
//                    return new SearchResult()
//                    {
//                        Data = _context.ChatMessages.ToList()
//                    };
//                }
//                else
//                {
//                    var result = _context.ChatMessages.Where(x => message == x.Message).ToList();
//                    return new SearchResult()
//                    {
//                        Data = result.ToList()
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        [HttpGet("{id}")]
//        public string Get(int id)
//        {
//            return "value";
//        }

//        [HttpPost]
//        public void Post([FromBody] string value)
//        {
//        }

//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }
//    }
//}
