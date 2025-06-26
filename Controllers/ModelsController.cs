using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Services;
using TrueDetectWebAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace TrueDetectWebAPI.Controllers {
    [Route("api/")]
    [ApiController]
    public class ModelsController : ControllerBase {
        private HttpClient _httpClient;

        private string _endpointUrl = string.Empty; // Replace with actual URL

        private List<string> _lines = new List<string>();

        public ModelsController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _endpointUrl = configuration["Models:Endpoint"]?.ToString() ?? string.Empty;
        }


[HttpPost("SendLineToModels")]
public async Task<IActionResult> Post([FromBody] BatchRequest batchRequest)
{
    if (batchRequest == null || batchRequest.data == null || batchRequest.data.Count == 0)
    {
        return BadRequest("Invalid batch data");
    }

    try
    {
        if (string.IsNullOrEmpty(_endpointUrl))
        {
            return StatusCode(500, "Internal Server Error: Models endpoint URL is not configured.");
        }

        // Convert batchRequest to JSON string
        var jsonContent = JsonSerializer.Serialize(batchRequest);
        Console.WriteLine($"Sending batch to models: {jsonContent}");
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Send request
        HttpResponseMessage response = await _httpClient.PostAsync(_endpointUrl, content);

        Console.WriteLine($"Response from models: {response.ToString()}");

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {responseBody}");
            // Deserialize the response
            var predictionResponse = JsonSerializer.Deserialize<PredictionResponse>(responseBody);

            if (predictionResponse != null && predictionResponse.predictions != null)
            {
                foreach (var prediction in predictionResponse.predictions)
                {
                    var matchingLog = batchRequest.data.FirstOrDefault(log => log.row_id == prediction.row_id);
                    if (matchingLog != null)
                    {
                        // Append anomaly result
                        var enrichedLog = new
                        {
                            matchingLog.row_id,
                            matchingLog.time,
                            matchingLog.src_user,
                            matchingLog.dst_user,
                            matchingLog.src_computer,
                            matchingLog.dst_computer,
                            matchingLog.auth_type,
                            matchingLog.logon_type,
                            matchingLog.auth_orientation,
                            matchingLog.success,
                            anomaly = prediction.anomaly
                        };

                        // Convert to JSON and send to client stream
                        string enrichedLogJson = JsonSerializer.Serialize(enrichedLog);
                        StreamToClientController.AddLine(enrichedLogJson);
                    }
                }
            }
            return Ok(new { message = "Batch sent successfully", response = responseBody });
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Failed to send batch to models.");
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal Server Error: {ex.Message}");
    }
}


    [ApiExplorerSettings(IgnoreApi = true)]
    public void SendLineToMemory(string line) {
        _lines.Add(line);
    }

    [HttpDelete("RemoveLineFromMemory/{line}")]
    public void Delete(string line) {
        _lines.Remove(line);
        }
    }
}