using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Services;
using TrueDetectWebAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;
using System.Diagnostics;

namespace TrueDetectWebAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class RetrainingDataController : ControllerBase
    {
        private readonly IFetchForRetrainingService _fetchForRetrainingService;

        public static RetrainingContextQueue _retrainingContextQueue;

        private readonly string _retrainEndpoint = string.Empty;
        private readonly HttpClient _httpClient;

        public static event Action<string>? RetrainingDataSent;

        public RetrainingDataController(IFetchForRetrainingService fetchForRetrainingService,
        RetrainingContextQueue retrainingContextQueue,
        IConfiguration configuration)
        {
            _fetchForRetrainingService = fetchForRetrainingService;
            _retrainingContextQueue = retrainingContextQueue;
            _httpClient = new HttpClient();
            _retrainEndpoint = configuration["Models:RetrainingEndpoint"]?.ToString() ?? string.Empty;
        }

        [HttpPost("SendRetrainingData")]
        public async Task<IActionResult> SendRetrainingData()
        {
            var data = _fetchForRetrainingService.FetchForRetraining();
            if (data == null || data.Count == 0)
            {
                return BadRequest("No retraining data found.");
            }

            // 1) start your output array with the data_size object
            int dataSize = data.Count;
            var output = new List<Dictionary<string, JsonElement>>();

            // 1.5) Add context from RetrainingContextQueue
            List<string> contextLines = _retrainingContextQueue.GetFullContext();
            if (contextLines.Count > 0)
            {
                foreach (var contextLine in contextLines)
                {
                    var contextDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(contextLine);
                    if (contextDict != null)
                    {
                        output.Add(contextDict);
                        dataSize++; // Increment data size for each context line added
                    }

                }
            }

            // 2) for each JSON‐string/key + Tag string
            foreach (var (jsonString, tagString) in data)
            {
                var trimmed = jsonString.Trim();
                if ((!trimmed.StartsWith("{")) || (trimmed.Contains("timestamp")))
                {
                    dataSize--;
                    Console.WriteLine($"Skipping Irrelevant key: {trimmed}, current data size: {dataSize}");
                    continue;
                }

                // parse into a mutable dictionary
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString)!;

                string rowId = dict["row_id"].GetString()!;
                int removed = output.RemoveAll(item =>
                    item.ContainsKey("row_id") && item["row_id"].GetString() == rowId);

                if (removed > 0)
                {
                    Console.WriteLine($"Removed {removed} duplicate row(s) with row_id: {rowId}");
                    dataSize -= removed;
            }

                var current = dict["anomaly"].GetBoolean();

                // flip only when needed
                if (tagString == TagType.Threat.ToString() && !current)
                    dict["anomaly"] = JsonDocument.Parse("true").RootElement;
                else if (tagString == TagType.Safe.ToString() && current)
                    dict["anomaly"] = JsonDocument.Parse("false").RootElement;

                output.Add(dict);
            }

            output.Insert(0, new Dictionary<string, JsonElement>
            {
                { "data_size", JsonDocument.Parse(dataSize.ToString()).RootElement }
            });

            // 3) serialize & send
            var jsonContent = JsonSerializer.Serialize(output);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            Console.WriteLine("Data for retraining:");
            Console.WriteLine(jsonContent);
            var response = await _httpClient.PostAsync(_retrainEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Response from retraining endpoint: {response.StatusCode}");
                string clientMessage = "Retraining data sent. Line amount: " + dataSize;
                RetrainingDataSent?.Invoke(clientMessage);
                return Ok("Retraining data sent successfully.");
            }

            else
                return StatusCode((int)response.StatusCode, "Failed to send retraining data.");

        }

    }
    

}
