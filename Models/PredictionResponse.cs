namespace TrueDetectWebAPI.Models
{
    public class PredictionResponse
    {
        public string? batch_id { get; set; }
        public List<Prediction>? predictions { get; set; }
    }

    public class Prediction
    {
        public string? row_id { get; set; }

        public bool anomaly { get; set; }
    }
}