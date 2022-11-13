
namespace Redis.Data
{
    public interface IDatabaseCommunication
    {
        void SetData(string key, string[] value, string[] field);
        string GetField(string key, string field);
        void DeleteData(string key);
    }
}
