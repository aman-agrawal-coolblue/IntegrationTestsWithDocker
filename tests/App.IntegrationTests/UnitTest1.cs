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
        public void Test1()
        {
            using (var connection = new MySqlConnection(
                "Server=127.0.0.1;Database=MyTestDB;Port=3306;Uid=root;Password=pass123"))
            {
                connection.Open();
                Assert.True(connection.State == System.Data.ConnectionState.Open);
            }
        }

        [Fact]
        public async Task Test2()
        {
            using (var connection = new MySqlConnection(
                "Server=127.0.0.1;Database=MyTestDB;Port=3306;Uid=root;Password=pass123"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "insert into MyTestDB.Products values (1, 100)";
                await cmd.ExecuteNonQueryAsync();

                var cmdSelect = connection.CreateCommand();
                cmdSelect.CommandText = "select count(*) from MyTestDB.Products";
                var rows = (long)await cmdSelect.ExecuteScalarAsync();
                Assert.True(rows == 1);
            }
        }
    }

    public class DBFixture : IDisposable
    {
        public DBFixture()
        {
            CleanUpDatabase();
        }

        public void Dispose()
        {
            CleanUpDatabase();
        }

        private void CleanUpDatabase()
        {
            using (var connection = new MySqlConnection(
                "Server=127.0.0.1;Database=MyTestDB;Port=3306;Uid=root;Password=pass123"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "delete from MyTestDB.Products";
                cmd.ExecuteNonQuery();
            }
        }
    }
}