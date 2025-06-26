using System.Collections.Concurrent;
using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Models;
using TrueDetectWebAPI.Services;

namespace TrueDetectWebAPI.Services
{
    public class RetrainingContextQueue
    {
        private readonly ConcurrentQueue<string> _retrainingContext = new ConcurrentQueue<string>();

        public void Enqueue(string context)
        {
            _retrainingContext.Enqueue(context);
            if (_retrainingContext.Count > 30) // Limit the queue size to prevent memory issues
            {
                _retrainingContext.TryDequeue(out _); // Remove the oldest item if the limit is exceeded
            }
        }

        public bool TryDequeue(out string context)
        {
            return _retrainingContext.TryDequeue(out context);
        }

        public List<string> GetFullContext()
        {
            return _retrainingContext.ToList();
        }

        public int Count => _retrainingContext.Count;
    }
}