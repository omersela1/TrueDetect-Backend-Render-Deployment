using TrueDetectWebAPI.Interfaces;

namespace TrueDetectWebAPI.Services.Redis
{
    public class RetrainingRedisService : IRetrainingRedisService
    {
        private readonly IRedisBaseService _redisBaseService;

        public RetrainingRedisService(IRedisBaseService redisBaseService)
        {
            _redisBaseService = redisBaseService;
        }

        public void SetRetrainingTime(string time)
        {
            _redisBaseService.SetString("CurrentRetrainingSchedule", time);
        }

        public string GetRetrainingTime()
        {
            return _redisBaseService.GetString("CurrentRetrainingSchedule") ?? "No retraining scheduled";
        }

    }

}