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
            var rows = await new Repository2Adapter(() => fixture.Connection).GetAll();

            Assert.True(rows.Count > 0);
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
    }

    [Table("Products")]
    public class Product
    {
        [ExplicitKey]
        public int ProductId { get; set; }
    }
}