using Dapper.Contrib.Extensions;

namespace IntegrationTestsWithDocker.Database
{
    [Table("Products")]
    public class Product
    {
        [ExplicitKey]
        public int ProductId { get; set; }
    }
}