namespace TrueDetectWebAPI.Interfaces
{
    public interface IRetrainingRedisService
    {
        public void SetRetrainingTime(string time);
        public string GetRetrainingTime();
    }
}
