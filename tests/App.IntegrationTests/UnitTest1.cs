using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using Xunit;

namespace App.IntegrationTests
{
    public class UnitTest1 : IClassFixture<DBFixture>
    {
        private readonly DBFixture fixture;

        public UnitTest1(DBFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestGetCount()
        {
            var rows = await new RepositoryAdapter(() => fixture.Connection).GetCount();
            Assert.True(rows == 1);
        }
    }

    public class RepositoryAdapter
    {
        private readonly Func<MySqlConnection> connectionFactory;

        public RepositoryAdapter(Func<MySqlConnection> connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<long> GetCount()
        {
            using (var conn = connectionFactory())
            {
                conn.Open();
                var cmdSelect = conn.CreateCommand();
                cmdSelect.CommandText = "select count(*) from MyTestDB.Products";
                var rows = (long)await cmdSelect.ExecuteScalarAsync();

                return rows;
            }
        }
    }
}