namespace TrueDetectWebAPI.Interfaces
{
    public interface IFetchForRetrainingService
    {
        public Dictionary<string, string> FetchForRetraining();
    }
}