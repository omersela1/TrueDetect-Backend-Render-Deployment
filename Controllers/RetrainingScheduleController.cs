using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Services;
using TrueDetectWebAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace TrueDetectWebAPI.Controllers {
    [Route("api/")]
    [ApiController]
    public class RetrainingScheduleController : ControllerBase {
        private readonly IRetrainingRedisService _retrainingRedisService;


        public RetrainingScheduleController(IRetrainingRedisService retrainingRedisService) {
            _retrainingRedisService = retrainingRedisService;
        }

        [HttpGet("GetCurrentRetrainingSchedule")]
        public Dictionary<string, string> Get() {
            string time = _retrainingRedisService.GetRetrainingTime();
            return new Dictionary<string, string> {
                { "CurrentRetrainingTime", time },
            };
        }

        [HttpPost("SetRetrainingTime/{time}")]
        public void Post(string time) {
            if (time == null)
            {
                throw new Exception("Invalid time");
            }
            if (!DateTime.TryParse(time, out DateTime parsedTime))
            {
                throw new Exception("Invalid time format");
            }
            _retrainingRedisService.SetRetrainingTime(time);
        }
        
    }
}