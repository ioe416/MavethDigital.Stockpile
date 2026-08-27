
using FluentAssertions;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Domain.Tests.Purchasing.Models;

public sealed class ReceiptLineTests
{
    [Fact]
    public void Empty_purchaseLineId_throws_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Action act = () => new ReceiptLine(
            Guid.Empty,
            1,
            createdAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("purchaseLineId");

    }

    [Fact]
    public void Quantity_less_than_1_throws_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Action act = () => new ReceiptLine(
            Guid.NewGuid(),
            0,
            createdAt);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantityReceived");
    }

    [Fact]
    public void Valid_purchaseLineId_should_be_saved()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseLineId = Guid.NewGuid();

        var line = new ReceiptLine(
            purchaseLineId,
            1,
            createdAt);

        line.UpdatedAt.Should().Be(createdAt);
        line.PurchaseLineId.Should().Be(purchaseLineId);
    }

    [Fact]
    public void Valid_quantity_should_be_saved()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseLineId = Guid.NewGuid();

        var line = new ReceiptLine(
            purchaseLineId,
            1,
            createdAt);

        line.UpdatedAt.Should().Be(createdAt);
        line.QuantityReceived.Should().Be(1);
    }
}
