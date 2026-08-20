using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Domain.Purchasing.Models;


namespace Stockpile.Domain.Tests.Purchasing.Models;

public sealed class PurchaseLineTests
{
    [Fact]
    public void Empty_part_id_throws_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Action act = () => new PurchaseLine(
            Guid.Empty,
            1,
            createdAt,
            null);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("partId")
            .WithMessage("A valid part is required.*");
    }

    [Fact]
    public void Quantity_less_than_one_throws_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        Action act = () => new PurchaseLine(
            Guid.NewGuid(),
            0,
            createdAt,
            null);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("A quantity greater than 0 is required.*")
            .WithParameterName("quantity");
    }

    [Fact]
    public void Valid_purchaseLine_should_save_partId()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);

        var line = new PurchaseLine(
            partId,
            1,
            createdAt,
            unitPrice);

        line.PartId.Should().Be(partId);
    }

    [Fact]
    public void Valid_purchaseLine_should_save_quantity()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt,
            unitPrice);

        line.Quantity.Should().Be(15);
    }

    [Fact]
    public void Valid_purchaseLine_should_save_unit_price()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt,
            unitPrice);

        line.UnitPrice.Should().Be(unitPrice);
    }

    [Fact]
    public void PurchaseLine_should_accept_a_null_unitPrice()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
 
        var line = new PurchaseLine(
            partId,
            15,
            createdAt,
            null);

        line.UnitPrice.Should().BeNull();
    }
}
