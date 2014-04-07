using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace BeeHive.Azure
{

    /// <summary>
    /// List name becomes the table name, key becomes Partition key and identity of T becomes RowKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AzureKeyedListStore<T> : IKeyedListStore<T>
        where T : IHaveIdentity
    {

        private const string EntityPropertyName = "__Tee";

        private string _connectionString;

        private async Task<CloudTable> GetTable(string tableName)
        {
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            if (!table.Exists())
            {
                try
                {
                    await table.CreateAsync();
                }
                catch (Exception exception)
                {
                    Trace.TraceError(exception.ToString());
                }

            }

            return table;
        }

        public AzureKeyedListStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddAsync(string listName, Guid key, T t)
        {
            var table = await GetTable(listName);
            var entity = new DynamicTableEntity(key.ToString(), t.Id.ToString());
            entity.Properties[EntityPropertyName] = new EntityProperty(
                JsonConvert.SerializeObject(t));
            await table.ExecuteAsync(TableOperation.Insert(entity));
        }

        public async Task<IEnumerable<T>> GetAsync(string listName, Guid key)
        {
            var table = await GetTable(listName);

            var condition = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, key.ToString());
            var query = new TableQuery<DynamicTableEntity>()
                .Where(condition);
            return table.ExecuteQuery(query)
                .Select(x => JsonConvert.DeserializeObject<T>(x.Properties[EntityPropertyName].StringValue));
        }

        public async Task RemoveAsync(string listName, Guid key)
        {
            var table = await GetTable(listName);
            foreach (var item in await GetAsync(listName, key))
            {
                await table.ExecuteAsync(TableOperation.Delete(new TableEntity(key.ToString(),
                    item.Id.ToString())
                {
                    ETag = "*"
                }));
            }            
        }

        public async Task<bool> ExistsAsync(string listName, Guid key)
        {
            var table = await GetTable(listName);

            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, key.ToString()); 

            var query = new TableQuery<DynamicTableEntity>()
                .Where(conditionPK);

            return table.ExecuteQuery(query).Any();
        }

        public async Task<bool> ListExistsAsync(string listName)
        {
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(listName);
            return table.Exists();
        }

        public async Task<bool> ItemExistsAsync(string listName, Guid key, Guid itemId)
        {
            var table = await GetTable(listName);

            var conditionPK = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, key.ToString());
            var conditionRK = TableQuery.GenerateFilterCondition("RowKey",
                QueryComparisons.Equal, itemId.ToString());

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, "and", conditionRK));
            return table.ExecuteQuery(query).Any();
        }

        public async Task UpdateAsync(string listName, Guid key, T t)
        {
        
            var table = await GetTable(listName);
            var entity = new DynamicTableEntity(key.ToString(), t.Id.ToString());
            entity.Properties[EntityPropertyName] = new EntityProperty(
                JsonConvert.SerializeObject(t));
            await table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
        }    
        
    }
}
