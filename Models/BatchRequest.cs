using TrueDetectWebAPI.Models;

namespace TrueDetectWebAPI.Models {
  public class BatchRequest
    {
            public string? batch_id { get; set; }
            public List<LogEvent>? data { get; set; }
    }

}
