using MySql.Data.MySqlClient;
using Polly;
using System;
using System.Data;

namespace IntegrationTestsWithDocker.Database
{
    public class DBFixture: IDisposable
    {
        public MySqlConnection Connection { get; private set; }

        public DBFixture()
        {
            Connection = TryEstablishingConnection();
        }

        private MySqlConnection TryEstablishingConnection()
        {
            var retryPolicy = Policy
                .Handle<MySqlException>()
                .WaitAndRetry(10, retry => TimeSpan.FromSeconds(retry * 2));

            return retryPolicy.Execute(() =>
            {
                using (var connection = new MySqlConnection(
                    "Server=127.0.0.1;Database=MyTestDB;Port=3306;Uid=root;Password=pass123"))
                {
                    connection.Open();
                    return connection;
                }
            });
        }

        public void Dispose()
        {
            CleanUpDatabase();
        }

        private void CleanUpDatabase()
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                var cmd = Connection.CreateCommand();
                cmd.CommandText = "delete from MyTestDB.Products";
                cmd.ExecuteNonQuery();
            }
        }
    }
}