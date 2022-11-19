
namespace Redis.Data
{
    public interface IDatabaseCommunication
    {
        void SetData(string key, string[] value, string[] field, string db);
        string GetField(string key, string field, string db);
        void DeleteData(string key, string db);
    }
}
