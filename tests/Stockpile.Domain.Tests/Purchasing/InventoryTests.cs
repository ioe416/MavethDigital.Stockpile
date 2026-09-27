using FluentAssertions;
using Stockpile.Domain.Inventory.Enums;
using Stockpile.Domain.Inventory.Models;

namespace Stockpile.Domain.Tests.Purchasing;

public sealed class InventoryTests
{
    [Fact]
    public void Receiving_inventory_should_increase_on_hand_quantity()
    {
        var createdAt = DateTimeOffset.UtcNow;
        // Arrange
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            0,
            createdAt);

        var initialOnHandQuantity = 0;
        var receivedQuantity = 10;
        // Act
        inventory.Receive(receivedQuantity, createdAt.AddMinutes(1));
        // Assert
        Assert.Equal(initialOnHandQuantity + receivedQuantity, inventory.OnHandQuantity);
        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType.Should().Be(TransactionType.Receipt);
    }

    [Fact]
    public void Issuing_inventory_should_decrease_on_hand_quantity()
    {
        var createdAt = DateTimeOffset.UtcNow;

        //Arrange
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            createdAt);

        var issuedQuantity = 4;
        var expectedOnHandQuantity = 6;

        // Act
        inventory.Issue(issuedQuantity, createdAt.AddMinutes(1));

        // Assert
        inventory.OnHandQuantity.Should().Be(expectedOnHandQuantity);
        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType.Should().Be(TransactionType.Issue);
        inventory.TransactionHistory.First().Quantity.Should().Be(issuedQuantity);
    }
}
