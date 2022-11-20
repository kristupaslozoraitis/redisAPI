using MediaBrowser.Model.Extensions;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;

namespace Redis.Data
{
    public class DatabaseCommunication : IDatabaseCommunication
    {
        private readonly IConnectionMultiplexer _redis;    // production 9001

        private IDatabase db_ProdArchivedYears;  // DB0
        private IDatabase db_ProdCurrentYears;   // DB1
        private IDatabase db_SalesArchivedYears; // DB2
        private IDatabase db_SalesCurrentYears;  // DB3

        public DatabaseCommunication(IConnectionMultiplexer redis)
        {
            _redis = redis;
            db_ProdArchivedYears = _redis.GetDatabase(0);
            db_ProdCurrentYears = _redis.GetDatabase(1);
            db_SalesArchivedYears = _redis.GetDatabase(2);
            db_SalesCurrentYears = _redis.GetDatabase(3);
        }

        public void DeleteData(string key, string db)
        {
            switch (db)
            {
                case "db_arch_sales":
                    db_ProdArchivedYears.KeyDelete(ChangeKey(key, false));
                    db_SalesArchivedYears.KeyDelete(key);
                    break;
                case "db_curr_sales":
                    db_ProdCurrentYears.KeyDelete(ChangeKey(key, false));
                    db_SalesCurrentYears.KeyDelete(key);
                    break;
                case "db_arch_prod":
                    db_ProdArchivedYears.KeyDelete(key);
                    DeleteProductsInSales(key, db_SalesArchivedYears);
                    break;
                case "db_curr_prod":
                    db_ProdCurrentYears.KeyDelete(key);
                    DeleteProductsInSales(key, db_SalesCurrentYears);
                    break;
                default:
                    return;
            }
        }

        public string GetField(string key, string field, string db)
        {
            var fieldValue = string.Empty;
            switch (db)
            {
                case "db_arch_sales":
                    fieldValue = db_SalesArchivedYears.HashGet(key, field);
                    break;
                case "db_curr_sales":
                    fieldValue = db_SalesCurrentYears.HashGet(key, field);
                    break;
                case "db_arch_prod":
                    fieldValue = db_ProdArchivedYears.HashGet(key, field);
                    break;
                case "db_curr_prod":
                    fieldValue = db_ProdCurrentYears.HashGet(key, field);
                    break;
                default:
                    return string.Empty;
            }
            if (!string.IsNullOrEmpty(fieldValue))
            {
                return fieldValue;
            }
            return null;

        }
        public void SetData(string key, string[] value, string[] field, string database)
        {
            if (value.Length != field.Length)
            {
                throw new InvalidDataException();
            }

            var values = new List<HashEntry>();
            for (int i = 0; i < value.Length; i++)
            {
                values.Add(new HashEntry(field[i], value[i]));
            }
            IDatabase db;
            switch (database)
            {
                case "db_arch_sales":
                    if (key.Contains("products"))
                    {
                        throw new Exception("Cannot update child element.");
                    }
                    db = db_SalesArchivedYears;
                    break;
                case "db_curr_sales":
                    if (key.Contains("products"))
                    {
                        throw new Exception("Cannot update child element.");
                    }
                    db = db_SalesCurrentYears;
                    break;
                case "db_arch_prod":
                    db = db_ProdArchivedYears;
                    UpdateProducts(ChangeKey(key), values, db_SalesArchivedYears);
                    break;
                case "db_curr_prod":
                    db = db_ProdCurrentYears;
                    UpdateProducts(ChangeKey(key), values, db_SalesCurrentYears);
                    break;
                default:
                    return;
            }

            db.HashSet(key, values.ToArray());
        }

        // change deleted value to 1 in sales table
        private void DeleteProductsInSales(string key, IDatabase db)
        {
            if (key.Contains("products"))
            {
                var newEntry = new List<HashEntry>
                {
                    new HashEntry("deleted", "1")
                };
                db.HashSet(ChangeKey(key), newEntry.ToArray());
            }
        }

        // vertical fragmentation via products table
        private void UpdateProducts(string key, List<HashEntry> values, IDatabase db)
        {
            if (key.Contains("products"))
            {
                List<HashEntry> newValues = values.Where(x => x.Name == "product_name" || x.Name == "list_price").ToList();
                db.HashSet(key, newValues.ToArray());
            }
        }

        // if you need to change table name from sales.products to production.products or vice versa for vertical fragmentation
        private string ChangeKey (string key, bool fromProductionToSales = true)
        {
            if (fromProductionToSales)
            {
                return key.Replace("production", "sales");
            }
            return key.Replace("sales", "production");
            
        }
    }
}
