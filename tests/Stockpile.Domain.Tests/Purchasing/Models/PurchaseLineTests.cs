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

    [Fact]
    public void A_positive_quantity_should_update_quantity_and_updatedAs_as_long_as_parent_purchase_is_editable()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);
        var newQuantity = 30;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);
        
        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.UpdateQuantity(createdAt.AddMinutes(3), line.Id, newQuantity);

        line.Quantity.Should().Be(newQuantity);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(3));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(3));

    }

    [Fact]
    public void A_negative_quantity_should_not_update_line_or_purchase_and_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);
        var newQuantity = -5;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => purchase.UpdateQuantity(createdAt.AddMinutes(3), line.Id, newQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>(
            "A valid positive quantity is required");

        line.Quantity.Should().Be(15);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
    }

    [Fact]
    public void A_zero_quantity_should_not_update_line_or_purchase_and_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);
        var newQuantity = 0;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => purchase.UpdateQuantity(createdAt.AddMinutes(3), line.Id, newQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>(
            "A valid positive quantity is required");

        line.Quantity.Should().Be(15);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
    }

    [Fact]
    public void A_valid_unit_price_should_update_unit_price_and_updatedAs_as_long_as_parent_purchase_is_editable()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);
        var newUnitPrice = new Money(1.42m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.UpdateUnitPrice(createdAt.AddMinutes(4), line.Id, newUnitPrice);

        line.UnitPrice.Should().Be(newUnitPrice);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));

    }

    [Fact]
    public void A_negative_unit_price_should_not_update_line_or_purchase_and_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);
        var newUnitPrice = new Money(-1.24m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => purchase.UpdateUnitPrice(createdAt.AddMinutes(4), line.Id, newUnitPrice);

        act.Should().Throw<ArgumentOutOfRangeException>
            ("A unit Price of $0.00 or greater is required.");

        line.UnitPrice.Should().Be(unitPrice);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
    }

    [Fact]
    public void A_zero_unit_price_should_update_line_and_purchase()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);
        var newUnitPrice = new Money(0m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.UpdateUnitPrice(createdAt.AddMinutes(4), line.Id, newUnitPrice);

        line.UnitPrice.Should().Be(newUnitPrice);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
    }

    [Fact]
    public void Duplicate_lineIds_should_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1),
            new Money(1.25m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => purchase.AddLine(createdAt.AddMinutes(3), line);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate lines cannot be added to the same purchase.");

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
        purchase.Lines.Should().Contain(line);
        purchase.Lines.Count.Should().Be(1);
    }

    [Fact]
    public void A_purchase_line_should_allow_a_valid_part_substitution_and_update_updatedAs_as_long_as_parent_purchase_is_editable()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var partId = Guid.NewGuid();
        var newPartId = Guid.NewGuid();
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.24m, currencyCode);;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            15,
            createdAt.AddMinutes(1),
            null);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.UpdatePartId(createdAt.AddMinutes(4), line.Id, newPartId);

        line.PartId.Should().Be(newPartId);
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));

    }
}
