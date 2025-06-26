 using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text;
using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Models;
using TrueDetectWebAPI.Services;
using TrueDetectWebAPI.Controllers;

namespace TrueDetectWebAPI.Controllers
{

    [Route("api/")]
    [ApiController]
    public class StreamToClientController : ControllerBase
    {
        private static readonly ConcurrentQueue<string> _streamQueue = new ConcurrentQueue<string>();

        private readonly RetrainingContextQueue _retrainingContextQueue;

        public StreamToClientController(RetrainingContextQueue retrainingContextQueue)
        {
            _retrainingContextQueue = retrainingContextQueue;
            RetrainingDataController.RetrainingDataSent += RetrainingDataSent;
        }

        [HttpGet("GetStream")]
        public async Task GetStream()
        {
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            var writer = new StreamWriter(Response.Body, Encoding.UTF8);
            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                while (_streamQueue.TryDequeue(out var line))
                {
                    AddRetrainingContext(line);
                    await writer.WriteLineAsync($"data: {line}\n");
                    await writer.FlushAsync();
                }
                await Task.Delay(500);
            }
        }

        public static void AddLine(string line)
        {
            _streamQueue.Enqueue(line);
        }

        public void AddRetrainingContext(string context)
        {
            _retrainingContextQueue.Enqueue(context);
        }
        
        private void RetrainingDataSent(string retrainingMessage)
        {
            AddLine(retrainingMessage);
        }
    }
}