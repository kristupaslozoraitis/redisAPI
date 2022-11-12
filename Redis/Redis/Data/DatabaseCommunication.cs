using StackExchange.Redis;
using System.Text.Json;

namespace Redis.Data
{
    public class DatabaseCommunication : IDatabaseCommunication
    {
        private readonly IConnectionMultiplexer _redis;

        public DatabaseCommunication(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public string GetField(string key, string field)
        {
            var db = _redis.GetDatabase();

            var fieldValue = (string)db.HashGet(key, field);
            if (!string.IsNullOrEmpty(fieldValue))
            {
                return fieldValue;
            }
            return null;
        }
        

        public void SetField(string key, string value, string field)
        {
            var db = _redis.GetDatabase();

            var values = new List<HashEntry>();
            values.Add(new HashEntry(field, value));

            db.HashSet(key, values.ToArray());
        }
    }
}
