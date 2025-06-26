using TrueDetectWebAPI.Interfaces;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;


namespace TrueDetectWebAPI.Services.Redis
{

    public class FetchForRetrainingService : IFetchForRetrainingService
    {
        private readonly IRedisBaseService _redisBaseService;

        public FetchForRetrainingService(IRedisBaseService redisBaseService)
        {
            _redisBaseService = redisBaseService;
        }

        public Dictionary<string, string> FetchForRetraining()
        {
            return _redisBaseService.GetAllTaggings();
        }
    }
}