
using FluentAssertions;
using Stockpile.Domain.Purchasing.Models;
using System.ComponentModel;

namespace Stockpile.Domain.Tests.Purchasing.Models;

public sealed class ReceiptTests
{
    [Fact]
    public void Receipt_with_empty_Guid_should_throw()
    {
        var createdAt = DateTimeOffset.UtcNow;
        Action act = () => new Receipt(
            Guid.Empty,
            createdAt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Receipt cannot contain an empty purchase*")
            .WithParameterName("purchaseId");

    }

    [Fact]
    public void A_valid_receipt_stores_a_purchaseId()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseId = Guid.NewGuid();

        var receipt = new Receipt(
            purchaseId,
            createdAt);

        receipt.PurchaseId.Should().Be(purchaseId);
    }

    [Fact]
    public void A_valid_receipt_starts_with_no_receipt_lines()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseId = Guid.NewGuid();

        var receipt = new Receipt(
            purchaseId,
            createdAt);

        receipt.Lines.Should().HaveCount(0);
    }

    [Fact]
    public void A_valid_receiptLine_can_be_added_to_a_receipt()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        var line = new ReceiptLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1));

        receipt.AddLine(
            createdAt.AddMinutes(2), line);

        receipt.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
        receipt.Lines.Should().HaveCount(1);
        receipt.Lines.Should().Contain(line);
    }

    [Fact]
    public void A_null_receiptLine_cannot_be_added_to_a_receipt()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        Action act = () => receipt.AddLine(createdAt.AddMinutes(1), null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("line");

        receipt.UpdatedAt.Should().Be(createdAt);
        receipt.Lines.Should().HaveCount(0);
    }

    [Fact]
    public void A_duplicate_receiptLine_cannot_be_added_to_the_same_receipt()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        var line = new ReceiptLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1));

        receipt.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => receipt.AddLine(
                            createdAt.AddMinutes(3), line);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate lines cannot be added to the same receipt.");

        receipt.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
        receipt.Lines.Should().HaveCount(1);
    }

    [Fact]
    public void Receipt_quantities_must_be_positive()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var line = new ReceiptLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(2));

        line.QuantityReceived.Should().Be(1);
    }


    [Fact]
    public void Zero_receipt_quantities_should_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        Action act = () => new ReceiptLine(
            Guid.NewGuid(),
            0,
            createdAt.AddMinutes(2));

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantityReceived");
    }

    [Fact]
    public void Negative_receipt_quantities_should_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        Action act = () => new ReceiptLine(
            Guid.NewGuid(),
            -1,
            createdAt.AddMinutes(2));

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantityReceived");
    }

    [Fact]
    public void A_receipt_can_contain_multiple_distinct_receipt_lines()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        var line = new ReceiptLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1));

        var line2 = new ReceiptLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(2));

        receipt.AddLine(createdAt.AddMinutes(3), line);

        receipt.AddLine(createdAt.AddMinutes(4), line2);

        receipt.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        receipt.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void A_duplicate_purchaseLine_cannot_be_referenced_in_the_same_receipt()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseLineId = Guid.NewGuid();

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        var line = new ReceiptLine(
            purchaseLineId,
            1,
            createdAt.AddMinutes(1));

        var line2 = new ReceiptLine(
            purchaseLineId,
            5,
            createdAt.AddMinutes(3));

        receipt.AddLine(createdAt.AddMinutes(4), line);

        Action act = () => receipt.AddLine(
                            createdAt.AddMinutes(5), line2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate purchase lines cannot be referenced in the same receipt.");

        receipt.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        receipt.Lines.Should().HaveCount(1);
    }
}


