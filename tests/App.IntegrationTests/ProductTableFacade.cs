using MySql.Data.MySqlClient;

namespace App.IntegrationTests
{
    public class ProductTableFacade
    {
        private readonly MySqlConnection connection;

        public ProductTableFacade(MySqlConnection connection)
        {
            this.connection = connection;
        }
        
        public void CreateTestProduct(Product product)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"insert into MyTestDB.Products values ({product.ProductId}, 100)";
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void Delete(int productId)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"delete from MyTestDB.Products where ProductId = {productId}";
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void DeleteAll()
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"delete from MyTestDB.Products";
            cmd.ExecuteNonQuery();
            connection.Close();
        }
    }
}