using System.Threading.Tasks;
using IntegrationTestsWithDocker.Database;
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
            var sut = new RepositoryAdapter(fixture);

            var product = new Product
                    {
                        ProductId = RandomIdGenerator.GetId()
                    };
            await sut.CreateTestProduct(product);
            var rows = await sut.GetCount();
            Assert.True(rows > 0);
            await sut.Delete(product.ProductId);
        }
    }
}