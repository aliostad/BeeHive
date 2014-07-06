using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace BeeHive.Azure
{

    /// <summary>
    /// Azure implementation of Big Table on top of Azure Table STorage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AzureBigTableStore<T> : IBigTableStore<T>
        where T : IHaveIdentityAndRange
    {

        private string _connectionString;
        private CloudTable _table;
        private const string EntityPropertyName = "__Tee";

        public AzureBigTableStore(string connectionString)
            : this(connectionString, GetDefaultTableName())
        {

        }

        public AzureBigTableStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudTableClient();
            _table = client.GetTableReference(tableName);
        }

        private static string GetDefaultTableName()
        {
            string tableName = typeof(T).Name;
            if (tableName.EndsWith("Entity"))
                tableName = tableName.Substring(0, tableName.LastIndexOf("Entity"));

            return tableName;
        }

        private async Task<CloudTable> GetTable()
        {

            if (!_table.Exists())
            {
                try
                {
                    await _table.CreateAsync();
                }
                catch (Exception exception)
                {
                    Trace.TraceError(exception.ToString());
                }

            }

            return _table;
        }

        public async Task<T> GetAsync(string id, string rangeKey)
        {
            var table = await GetTable();
            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, id);
            var conditionRK = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.Equal, rangeKey);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, "and", conditionRK));
            return GetItem(table.ExecuteQuery(query).FirstOrDefault(), id, rangeKey);
        }

        private T GetItem(DynamicTableEntity entity, string id, string rangeKey)
        {
            if (entity == null)
                throw new KeyNotFoundException(string.Format("PK: {0} - RK: {1}", id, rangeKey));

            var deserializedObject = JsonConvert.DeserializeObject<T>(
                entity.Properties[EntityPropertyName].StringValue);
            return deserializedObject;
        }

        public async Task<IEnumerable<T>> GetRangeAsync(string id, string rangeStart, string rangeEnd)
        {
            var table = await GetTable();
            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, id);
            var conditionRKgt = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.GreaterThanOrEqual, rangeStart);
            var conditionRKlt = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.LessThanOrEqual, rangeEnd);

            var conditionRK = TableQuery.CombineFilters(conditionRKgt, "and", conditionRKlt);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, "and", conditionRK));
            return
                table.ExecuteQuery(query).Select(x => GetItem(x, x.PartitionKey,x.RowKey));
                
        }

        public async Task InsertAsync(T t)
        {
            var table = await GetTable();
            await table.ExecuteAsync(TableOperation.Insert(GetEntity(t, true)));
        }

        public async Task UpsertAsync(T t)
        {
            var tcw = t as IConcurrencyAware;
            OperationContext ctx = null;
            if (tcw != null && tcw.ETag != null)
            {
              
                ctx = new OperationContext()
                {
                    UserHeaders =
                    {
                        {"If-Match", tcw.ETag}
                    }
                };
            }
                

            var table = await GetTable();
            await table.ExecuteAsync(TableOperation.InsertOrReplace(GetEntity(t, true)),
                null, ctx);
        }

        public async Task DeleteAsync(T t)
        {
            var table = await GetTable();
            await table.ExecuteAsync(TableOperation.Delete(GetEntity(t)));
        }

        public async Task<bool> ExistsAsync(string id, string rangeKey)
        {
            var table = await GetTable();
            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, id);
            var conditionRK = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.Equal, rangeKey);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, "and", conditionRK));
            return table.ExecuteQuery(query).Any();
        }

        private DynamicTableEntity GetEntity(T t, bool storeEntity = false)
        {
            var cwt = t as IConcurrencyAware;
            var tableEntity = new DynamicTableEntity()
            {
                PartitionKey = t.Id,
                RowKey = t.RangeKey,
                ETag = cwt == null || cwt.ETag == null ? "*" : cwt.ETag,
                Timestamp = cwt == null || cwt.LastModified == null ? DateTimeOffset.UtcNow : cwt.LastModified.Value
            };
            if (storeEntity)
                tableEntity.Properties[EntityPropertyName] = new EntityProperty(JsonConvert.SerializeObject(t));
            return tableEntity;
        }
    }
}
