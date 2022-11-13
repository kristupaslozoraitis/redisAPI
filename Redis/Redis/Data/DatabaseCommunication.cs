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

        public void DeleteData(string key)
        {
            var db = _redis.GetDatabase();

            db.KeyDelete(key);
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
        

        public void SetData(string key, string[] value, string[] field)
        {
            var db = _redis.GetDatabase();

            if(value.Length != field.Length)
            {
                throw new InvalidDataException();
            }

            var values = new List<HashEntry>();
            for (int i = 0; i < value.Length; i++)
            {
                values.Add(new HashEntry(field[i], value[i]));
            }

            db.HashSet(key, values.ToArray());
        }
    }
}
