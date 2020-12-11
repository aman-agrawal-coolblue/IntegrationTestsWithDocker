using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTestsWithDocker.Database
{
    public class RepositoryAdapter
    {
        private readonly DBFixture connectionFactory;

        public RepositoryAdapter(DBFixture connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<long> GetCount()
        {
            using (var conn = connectionFactory.Connection)
            {
                conn.Open();
                var cmdSelect = conn.CreateCommand();
                cmdSelect.CommandText = "select count(*) from MyTestDB.Products";
                var rows = (long)await cmdSelect.ExecuteScalarAsync();

                return rows;
            }
        }

         public async Task<IReadOnlyCollection<Product>> GetAll()
        {
            using (var conn = connectionFactory.Connection)
            {
                conn.Open();
                var rows = await conn.QueryAsync<Product>("select * from MyTestDB.Products");

                return rows.ToList();
            }
        }

        public async Task<long> GetCountOf(int productId)
        {
            using (var conn = connectionFactory.Connection)
            {
                conn.Open();
                var cmdSelect = conn.CreateCommand();
                cmdSelect.CommandText = $"select count(*) from MyTestDB.Products where ProductId = {productId}";
                var rows = (long)await cmdSelect.ExecuteScalarAsync();

                return rows;
            }
        }

         public async Task<int> CreateTestProduct(Product product)
        {
            using (var conn = connectionFactory.Connection)
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"insert into MyTestDB.Products values ({product.ProductId}, 100)";
                var rowsInserted = await cmd.ExecuteNonQueryAsync();
                return rowsInserted;
            }
        }

        public async Task<int> Delete(int productId)
        {
             using (var conn = connectionFactory.Connection)
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"delete from MyTestDB.Products where ProductId = {productId}";
                var deletedRows = await cmd.ExecuteNonQueryAsync();
                return deletedRows;
            }
        }

        public async Task<int> DeleteAll()
        {
            using (var conn = connectionFactory.Connection)
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $"delete from MyTestDB.Products";
                var deletedRows = await cmd.ExecuteNonQueryAsync();
                return deletedRows;
            }
        }
    }
}