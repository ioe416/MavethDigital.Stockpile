
using FluentAssertions;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Domain.Tests.Purchasing.Models
{
    public sealed class ReceiptTests
    {
        [Fact]
        public void Receipt_with_empty_Guid_should_throw()
        {
            var createdAt = DateTimeOffset.UtcNow;
            Action act = () => new Receipt(
                Guid.Empty,
                createdAt);

            act.Should().Throw<InvalidOperationException>
                ("Receipt cannot contain an empty purchase");

        }
    }
}
