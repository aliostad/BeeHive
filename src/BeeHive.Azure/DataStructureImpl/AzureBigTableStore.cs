using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeeHive.DataStructures;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace BeeHive.Azure
{

    internal class TableConstants
    {
        public const string PartitionKey = "PartitionKey";
        public const string RowKey = "RowKey";
        public const string EntityPropertyName = "__Tee";
        public const string And = "and";
        public const string Id = "Id";
        public const string RangeKey = "RangeKey";
        public const string LastModified = "LastModified";
        public const string ETag = "ETag";
        

    }

    /// <summary>
    /// Azure implementation of Big Table on top of Azure Table STorage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AzureBigTableStore<T> : IBigTableStore<T>
        where T : IHaveIdentityAndRange, new()
    {

        private static Type[] SimpleTypes =
        {
            typeof (bool),
            typeof (int),
            typeof (long),
            typeof (double),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (string),
            typeof(Guid),
            typeof(byte[]),
            typeof (bool?),
            typeof (int?),
            typeof (long?),
            typeof (double?),
            typeof (DateTime?),
            typeof (DateTimeOffset?),
            typeof(Guid?)

        };

        private string _connectionString;
        private CloudTable _table;
        private Dictionary<string, PropertyInfo> _simpleProperties;
        private Dictionary<string, PropertyInfo> _complexProperties;

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

            ProcessType();
        }

        private void ProcessType()
        {
            var props = typeof (T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _simpleProperties = props
                .Where(x => SimpleTypes.Contains(x.PropertyType)).ToDictionary(x => x.Name);
            _complexProperties = props.Except(_simpleProperties.Values).ToDictionary(x => x.Name);
 
            _simpleProperties.Remove(TableConstants.Id);
            _simpleProperties.Remove(TableConstants.RangeKey);

            if (_simpleProperties.ContainsKey(TableConstants.ETag))
                _simpleProperties.Remove(TableConstants.ETag);

            if (_simpleProperties.ContainsKey(TableConstants.LastModified))
                _simpleProperties.Remove(TableConstants.LastModified);


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
            var conditionPK = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey,
                QueryComparisons.Equal, id);
            var conditionRK = TableQuery.GenerateFilterCondition(TableConstants.RowKey,
                QueryComparisons.Equal, rangeKey);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, TableConstants.And, conditionRK));
            return GetItem(table.ExecuteQuery(query).FirstOrDefault(), id, rangeKey);
        }

        private T GetItem(DynamicTableEntity entity, string id, string rangeKey)
        {
            if (entity == null)
                throw new KeyNotFoundException(string.Format("PK: {0} - RK: {1}", id, rangeKey));

            if (entity.Properties.ContainsKey(TableConstants.EntityPropertyName))
            {
                var deserializedObject = JsonConvert.DeserializeObject<T>(
                    entity.Properties[TableConstants.EntityPropertyName].StringValue);
                return deserializedObject;
            }
            else
            {
                var foo = new T();
                foreach (var simpleProperty in _simpleProperties.Values)
                {
                    if (entity.Properties.ContainsKey(simpleProperty.Name))
                    {
                        if (simpleProperty.PropertyType == typeof(DateTimeOffset)) // datetimeoffset is special case since object coming back is datetime
                            simpleProperty.GetSetMethod()
                            .Invoke(foo, new object[] { new DateTimeOffset(
                                entity.Properties[simpleProperty.Name].DateTimeOffsetValue.Value.DateTime,TimeSpan.Zero)});
                        else
                            simpleProperty.GetSetMethod()
                                .Invoke(foo, new[] { entity.Properties[simpleProperty.Name].PropertyAsObject });    
                    }                    
                }
                foreach (var complexProperty in _complexProperties.Values)
                {
                    if (entity.Properties.ContainsKey(complexProperty.Name))
                    {
                        complexProperty.GetSetMethod().Invoke(foo, new[]
                        {
                            JsonConvert.DeserializeObject(entity.Properties[complexProperty.Name].StringValue,
                                complexProperty.PropertyType)
                        });
                    }
                }
                return foo;
            }
        }

        public async Task<IEnumerable<T>> GetRangeAsync(string id, string rangeStart, string rangeEnd)
        {
            var table = await GetTable();
            TableQuery<DynamicTableEntity> query = null;

            if (id == null)
            {
                query = new TableQuery<DynamicTableEntity>();
            }
            else
            {
                var conditionPK = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey,
               QueryComparisons.Equal, id);
                var conditionRKgt = TableQuery.GenerateFilterCondition(TableConstants.RowKey,
                    QueryComparisons.GreaterThanOrEqual, rangeStart);
                var conditionRKlt = TableQuery.GenerateFilterCondition(TableConstants.RowKey,
                    QueryComparisons.LessThanOrEqual, rangeEnd);

                var conditionRK = TableQuery.CombineFilters(conditionRKgt, TableConstants.And, conditionRKlt);
                query = new TableQuery<DynamicTableEntity>()
                    .Where(TableQuery.CombineFilters(conditionPK, TableConstants.And, conditionRK));
            }
                      
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
            var conditionPK = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey,
                QueryComparisons.Equal, id);
            var conditionRK = TableQuery.GenerateFilterCondition(TableConstants.RowKey,
                QueryComparisons.Equal, rangeKey);

            var query = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.CombineFilters(conditionPK, TableConstants.And, conditionRK));
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
            {
                foreach (var simpleProperty in _simpleProperties.Values)
                {
                    tableEntity.Properties[simpleProperty.Name] =
                        EntityProperty.CreateEntityPropertyFromObject(simpleProperty.GetGetMethod().Invoke(t, null));
                }

                foreach (var complexProperty in _complexProperties.Values)
                {
                    tableEntity.Properties[complexProperty.Name] =
                        EntityProperty.CreateEntityPropertyFromObject(
                            JsonConvert.SerializeObject(complexProperty.GetGetMethod().Invoke(t, null)));
                }
            }

            return tableEntity;
        }
    }
}
