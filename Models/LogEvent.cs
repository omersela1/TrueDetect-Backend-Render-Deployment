namespace TrueDetectWebAPI.Models {
  
  public class LogEvent
    {
        public string? row_id { get; set; }
        public string? time { get; set; }
        public string? src_user { get; set; }
        public string? dst_user { get; set; }

        public string? src_computer { get; set; }

        public string? dst_computer { get; set; }
        public string? auth_type { get; set; }
        public string? logon_type { get; set; }
        public string? auth_orientation { get; set; }
        public int success { get; set; }
    }

}