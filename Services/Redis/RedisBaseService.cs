using TrueDetectWebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace TrueDetectWebAPI.Services
{
    public class RedisBaseService : IRedisBaseService
    {
        private readonly IDatabase _database;

        public RedisBaseService(IConfiguration configuration)
        {
            string connectionString = configuration["Redis:ConnectionString"].ToString();
            _database = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        }

        public string GetString(string key) => _database.StringGet(key);

        public void SetString(string key, string value) => _database.StringSet(key, value);

        public Dictionary<string, string> GetDictionary(string key)
        {
            var result = new Dictionary<string, string>();
            var data = _database.HashGetAll(key);
            foreach (var entry in data)
            {
                result[entry.Name] = entry.Value;
            }
            return result;
        }

        public void SetDictionary(string key, Dictionary<string, string> data)
        {
            var entries = data.Select(d => new HashEntry(d.Key, d.Value)).ToArray();
            _database.HashSet(key, entries);
        }

        public void RemoveKey(string key) => _database.KeyDelete(key);

        public Dictionary<string, string> GetAllTaggings()
        {
            var result = new Dictionary<string, string>();
            var endpoints = _database.Multiplexer.GetEndPoints();
            var server = _database.Multiplexer.GetServer(endpoints.First());

            // Get all keys matching the tagging pattern
            foreach (var key in server.Keys(pattern: "*#Tag"))
            {
                var value = _database.StringGet(key);
                if (value.HasValue && key.ToString() != "keep-alive#Tag")
                {
                    // Remove the "#Tag" suffix to get the original line
                    var line = key.ToString().Replace("#Tag", "");
                    result[line] = value;
                }
            }
            Console.WriteLine(result);
            return result;
        }
    }

    
}
