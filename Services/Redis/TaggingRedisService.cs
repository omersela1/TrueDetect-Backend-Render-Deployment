using TrueDetectWebAPI.Interfaces;

namespace TrueDetectWebAPI.Services.Redis
{
    public class TaggingRedisService : ITaggingRedisService
    {
        private readonly IRedisBaseService _redisBaseService;

        public TaggingRedisService(IRedisBaseService redisBaseService)
        {
            _redisBaseService = redisBaseService;
        }

        public void SetTagging(string line, string tag)
        {
            _redisBaseService.SetString($"{line}#Tag", tag);
        }

        public string GetTagging(string line) {
            string? result = _redisBaseService.GetString($"{line}#Tag");
            return result ?? "Untagged";
        }

        public void RemoveTagging(string line)
        {
            _redisBaseService.RemoveKey($"{line}#Tag");
        }
        
    }
}