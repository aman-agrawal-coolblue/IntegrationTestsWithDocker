using IntegrationTestsWithDocker.Database;
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
            var product1 = new Product
                    {
                        ProductId = RandomIdGenerator.GetId()
                    };
            var product2 = new Product
                    {
                        ProductId = product1.ProductId + 1
                    };
            var sut = new RepositoryAdapter(fixture);

            await sut.CreateTestProduct(product1);
            await sut.CreateTestProduct(product2);
            var rows1 = await sut.GetCountOf(product1.ProductId);
            var rows2 = await sut.GetCountOf(product2.ProductId);

            Assert.True(rows1 == 1, $"Expected 1 row for product1 {product1.ProductId} found {rows1}");
            Assert.True(rows2 == 1, $"Expected 1 row for product2 {product2.ProductId} found {rows2}");
            await sut.Delete(product1.ProductId);
            await sut.Delete(product2.ProductId);
        }
    }
}