using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Services;
using TrueDetectWebAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace TrueDetectWebAPI.Controllers {
    [Route("api/")]
    [ApiController]
    public class TaggingController : ControllerBase
    {
        private readonly ITaggingRedisService _taggingRedisService;


        public TaggingController(ITaggingRedisService taggingRedisService)
        {
            _taggingRedisService = taggingRedisService;
        }

        [HttpGet("GetTagging/{line}")]
        public Dictionary<string, string> Get(string line)
        {
            string tag = _taggingRedisService.GetTagging(line);
            return new Dictionary<string, string> {
                { "Line", line },
                { "Tag", tag }
            };
        }

        [HttpPost("SetTagging/{line}/{tag}")]
        public void Post(string line, string tag)
        {
            if (line == null)
            {
                throw new Exception("Invalid line");
            }
            if (tag == null)
            {
                throw new Exception("Invalid tag");
            }
            if (tag != TagType.Untagged.ToString() && tag != TagType.Safe.ToString() && tag != TagType.Threat.ToString())
            {
                throw new Exception("Invalid tag");
            }
            _taggingRedisService.SetTagging(line, tag);
        }

        [HttpDelete("RemoveTagging/{line}")]
        public void Delete(string line)
        {
            _taggingRedisService.RemoveTagging(line);
        }
        // Temporary endpoint to test large request body size limit
        [HttpPost("TestLargePayload")]
        public IActionResult TestLargePayload([FromBody] object payload)
        {
            Console.WriteLine("TestLargePayload endpoint hit.");
            // We don't need to do anything with the payload for this test
            return Ok("Received large payload test request.");
        }
    }
}