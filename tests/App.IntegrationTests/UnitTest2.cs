using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace App.IntegrationTests
{
    public class UnitTest2 : IClassFixture<DBFixture>
    {
        private readonly DBFixture fixture;

        public UnitTest2(DBFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestGetAll()
        {
            var productTable = new ProductTableFacade(fixture.Connection);
            var productId1 = RandomIdGenerator.GetId();
            var productId2 = productId1 + 1;
            
            productTable.CreateTestProduct(
                    new Product
                    {
                        ProductId = productId1
                    });

            productTable.CreateTestProduct(
                    new Product
                    {
                        ProductId = productId2
                    });

            var rows1 = await new Repository2Adapter(() => fixture.Connection).GetCountOf(productId1);
            var rows2 = await new Repository2Adapter(() => fixture.Connection).GetCountOf(productId2);

            Assert.True(rows1 == 1, $"Expected 1 row for product1 {productId1} found {rows1}");
            Assert.True(rows2 == 1, $"Expected 1 row for product2 {productId2} found {rows2}");
            productTable.Delete(productId1);
            productTable.Delete(productId2);
        }
    }

    public class Repository2Adapter
    {
        private readonly Func<MySqlConnection> connectionFactory;

        public Repository2Adapter(Func<MySqlConnection> connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyCollection<Product>> GetAll()
        {
            using (var conn = connectionFactory())
            {
                conn.Open();
                var rows = await conn.QueryAsync<Product>("select * from MyTestDB.Products");

                return rows.ToList();
            }
        }

        public async Task<long> GetCountOf(int productId)
        {
            using (var conn = connectionFactory())
            {
                conn.Open();
                var cmdSelect = conn.CreateCommand();
                cmdSelect.CommandText = $"select count(*) from MyTestDB.Products where ProductId = {productId}";
                var rows = (long)await cmdSelect.ExecuteScalarAsync();

                return rows;
            }
        }
    }

    [Table("Products")]
    public class Product
    {
        [ExplicitKey]
        public int ProductId { get; set; }
    }
}