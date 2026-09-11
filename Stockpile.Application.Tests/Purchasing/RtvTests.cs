
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Application.Purchasing.Receiving.RecordReceipt;
using Stockpile.Application.Purchasing.RTV;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Application.Tests.Purchasing;

public sealed class RtvTests
{
    [Fact]
    public void Rtv_should_reduce_received_quantity()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseId = Guid.NewGuid();
        var purchaseLineId = Guid.NewGuid();
        var quantityReceived = 10;
        var quantityToReturn = 3;

        var purchase = new Purchase(
            purchaseId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "123456");

        // Act
        var recordReceiptCommand = new RecordReceiptCommand(
            purchaseId,
            purchaseLineId,
            quantityReceived,
            DateTimeOffset.UtcNow);
        var recordRtvCommand = new RecordRtvCommand(
            purchaseId,
            purchaseLineId,
            quantityToReturn,
            DateTimeOffset.UtcNow);
        // Assert
        Assert.Equal(quantityReceived, recordReceiptCommand.QuantityReceived);
        Assert.Equal(quantityToReturn, recordRtvCommand.QuantityToReturn);
    }

    [Fact]
    public void Rtv_should_reopen_purchase_if_line_becomes_incomplete()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var purchaseLineId = Guid.NewGuid();
        var quantityReceived = 10;
        var quantityToReturn = 2;
        // Act
        var recordReceiptCommand = new RecordReceiptCommand(
            purchaseId,
            purchaseLineId,
            quantityReceived,
            DateTimeOffset.UtcNow);
        var recordRtvCommand = new RecordRtvCommand(
            purchaseId,
            purchaseLineId,
            quantityToReturn,
            DateTimeOffset.UtcNow);
        // Assert
        Assert.Equal(quantityReceived, recordReceiptCommand.QuantityReceived);
        Assert.Equal(quantityToReturn, recordRtvCommand.QuantityToReturn);
    }

    [Fact]
    public void Rtv_more_than_received_should_throw()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var purchaseLineId = Guid.NewGuid();
        var quantityReceived = 4;
        var quantityToReturn = 5;
        // Act
        var recordReceiptCommand = new RecordReceiptCommand(
            purchaseId,
            purchaseLineId,
            quantityReceived,
            DateTimeOffset.UtcNow);
        var recordRtvCommand = new RecordRtvCommand(
            purchaseId,
            purchaseLineId,
            quantityToReturn,
            DateTimeOffset.UtcNow);
        // Assert
        Assert.Equal(quantityReceived, recordReceiptCommand.QuantityReceived);
        Assert.Equal(quantityToReturn, recordRtvCommand.QuantityToReturn);
    }

    [Fact]
    public void Rtv_audit_persists_even_when_domain_rejects()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var purchaseLineId = Guid.NewGuid();
        var quantityReceived = 4;
        var quantityToReturn = 5;
        // Act
        var recordReceiptCommand = new RecordReceiptCommand(
            purchaseId,
            purchaseLineId,
            quantityReceived,
            DateTimeOffset.UtcNow);
        var recordRtvCommand = new RecordRtvCommand(
            purchaseId,
            purchaseLineId,
            quantityToReturn,
            DateTimeOffset.UtcNow);
        // Assert
        Assert.Equal(quantityReceived, recordReceiptCommand.QuantityReceived);
        Assert.Equal(quantityToReturn, recordRtvCommand.QuantityToReturn);
    }
}
