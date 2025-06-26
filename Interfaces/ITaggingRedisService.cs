namespace TrueDetectWebAPI.Interfaces
{
    public interface ITaggingRedisService
    {
        public void SetTagging(string line, string tag);
        public string GetTagging(string line);
        public void RemoveTagging(string line);
    }
}
