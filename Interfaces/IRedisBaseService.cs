using Microsoft.AspNetCore.Mvc;

namespace TrueDetectWebAPI.Interfaces
{
    public interface IRedisBaseService
    {
        string GetString(string key);
        void SetString(string key, string value);
        Dictionary<string, string> GetDictionary(string key);
        void SetDictionary(string key, Dictionary<string, string> data);
        void RemoveKey(string key);

        public Dictionary<string, string> GetAllTaggings();
    }
}
