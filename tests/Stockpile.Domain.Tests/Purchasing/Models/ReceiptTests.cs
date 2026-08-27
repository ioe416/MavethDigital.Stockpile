
using FluentAssertions;
using Stockpile.Domain.Purchasing.Models;

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

        var receipt = new Receipt(
            Guid.NewGuid(),
            createdAt);

        var line = new ReceiptLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(2));

    }
}


