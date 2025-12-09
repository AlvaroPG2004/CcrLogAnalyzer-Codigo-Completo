using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CcrLogAnalyzer.Models.Dao;
using Dapper;
using MoreLinq;
using Z.BulkOperations;
using Z.Dapper.Plus;

namespace CcrLogAnalyzer.Repositories
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class, ITDao
    {
        protected readonly string _connectionString;
        protected readonly string _tableName;

        protected GenericRepository(string connectionString)
        {
            _tableName = nameof(T);
            _connectionString = connectionString;
            SetMapping();
        }

        public virtual async Task<int> GetMaxId(string identifierName)
        {
            using (var connection = CreateConnection())
                return await connection.QuerySingleOrDefaultAsync<int>($"SELECT IFNULL(MAX({identifierName}),0) + 1 FROM {_tableName}");

        }

        public async virtual Task<int> BulkSaveAsync(IList<T> objectsList)
        {
            ResultInfo resultInfo = null;
            using (var connection = CreateConnection())
            {
                //Gets max id before insert
                int maxId = await GetMaxId("Id");

                //Sets max id before insert
                objectsList.ForEach(o => o.Id = maxId++);

                await connection
                    .UseBulkOptions(options =>
                    {
                        options.UseRowsAffected = true;
                        resultInfo = options.ResultInfo;
                    })
                    .BulkActionAsync(x =>
                    {
                        x.BulkInsert(objectsList);
                    });
            }
            return resultInfo != null ? resultInfo.RowsAffectedInserted : 0;
        }

        public async virtual Task<int> BulkUpdateAsync(IList<T> objectsList)
        {
            ResultInfo resultInfo = null;
            using (var connection = CreateConnection())
            {
                await connection
                    .UseBulkOptions(options =>
                    {
                        options.UseRowsAffected = true;
                        resultInfo = options.ResultInfo;
                    })
                    .BulkActionAsync(x =>
                    {
                        x.BulkUpdate(objectsList);
                    });
            }
            return resultInfo != null ? resultInfo.RowsAffectedUpdated : 0;
        }

        public async virtual Task<int> BulkDeleteAsync(IList<T> objectsList)
        {
            ResultInfo resultInfo = null;
            using (var connection = CreateConnection())
            {
                await connection
                    .UseBulkOptions(options =>
                    {
                        options.UseRowsAffected = true;
                        resultInfo = options.ResultInfo;
                    })
                    .BulkActionAsync(x =>
                    {
                        x.BulkDelete(objectsList);
                    });
            }
            return resultInfo != null ? resultInfo.RowsAffectedDeleted : 0;
        }

        public async virtual Task<IEnumerable<T>> GetAllAsync()
        {
            using (var connection = CreateConnection())
                return await connection.QueryAsync<T>($"SELECT * FROM {_tableName}");
        }

        public async virtual Task<T> GetAsync(int id, string identifierName = "")
        {
            if (string.IsNullOrEmpty(identifierName))
                identifierName = "Id";

            using (var connection = CreateConnection())
                return await connection.QueryFirstAsync<T>($"SELECT * FROM {_tableName} WHERE {identifierName}=@Id", new { Id = id });
        }

        public async virtual Task<IEnumerable<T>> GetTopAsync(int count, string identifierName = "")
        {
            if (string.IsNullOrEmpty(identifierName))
                identifierName = "Id";

            using (var connection = CreateConnection())
                return await connection.QueryAsync<T>($"SELECT TOP {count} * FROM {_tableName} order by {identifierName} desc");
        }

        public virtual void SetMapping()
        {
            DapperPlusManager.Entity<T>()
                 .Key(o => o.Id)
                 .Ignore(o => new
                 {
                 });
        }

        protected IDbConnection CreateConnection()
        {
            return SqlConnection();
        }

        private SqlConnection SqlConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
