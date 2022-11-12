
namespace Redis.Data
{
    public interface IDatabaseCommunication
    {
        void SetField(string key, string value, string field);
        string GetField(string key, string field);
    }
}
