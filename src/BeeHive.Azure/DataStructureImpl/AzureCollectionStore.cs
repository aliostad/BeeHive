using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace BeeHive.Azure
{
    /// <summary>
    /// Supports concurrency aware update and delete
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AzureCollectionStore<T> : ICollectionStore<T>
        where T : IHaveIdentity
    {
        private string _tableName;

        private const string EntityPropertyName = "__Tee";

        private string _connectionString;
        private CloudTable _table;

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

        public AzureCollectionStore(string connectionString)
            : this(connectionString, GetDefaultTableName())
        {
        }

        public AzureCollectionStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudTableClient();
            _table = client.GetTableReference(_tableName);
        }


        private static string GetDefaultTableName()
        {
            string tableName = typeof (T).Name;
            if (tableName.EndsWith("Entity"))
                tableName = tableName.Substring(0, tableName.LastIndexOf("Entity"));

            return tableName;
        }

     

        private string GetPartitionKey(string id)
        {
            return id.ToString().Split('-').First();
        }

        public async Task<T> GetAsync(string id)
        {
            var table =await  GetTable();
            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, GetPartitionKey(id) );
            var conditionRK = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.Equal, id);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, "and", conditionRK));
            return GetItem(table.ExecuteQuery(query).FirstOrDefault(), id);
        }

        private T GetItem(DynamicTableEntity entity, string id)
        {
            if(entity == null)
                throw new KeyNotFoundException(id);

            return JsonConvert.DeserializeObject<T>(entity.Properties[EntityPropertyName].StringValue);
        }

        private DynamicTableEntity GetEntity(T t, bool storeEntity = false)
        {
            var cwt = t as IConcurrencyAware;
            var tableEntity = new DynamicTableEntity()
            {
                PartitionKey = GetPartitionKey(t.Id),
                RowKey = t.Id,
                ETag = cwt == null ? "*" : cwt.ETag,
                Timestamp = cwt == null ? DateTimeOffset.UtcNow : cwt.LastModified.Value
            };
            if (storeEntity)
                tableEntity.Properties[EntityPropertyName] = new EntityProperty(JsonConvert.SerializeObject(t));
            return tableEntity;
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
            if (tcw != null)
            {
                ctx = new OperationContext()
                {
                    UserHeaders = new Dictionary<string, string>()
                };
                ctx.UserHeaders.Add("ETag", tcw.ETag);
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

        public async Task<bool> ExistsAsync(string id)
        {
            var table = await GetTable();
            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, GetPartitionKey(id));
            var conditionRK = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.Equal, id);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, "and", conditionRK));
            return table.ExecuteQuery(query).Any();
        }
    }
}
