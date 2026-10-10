using FluentAssertions;
using Stockpile.Domain.Inventory.Models;

namespace Stockpile.Domain.Tests;

public sealed class InventoryLifecycleTest
{
    [Fact]
    public void Inventory_lifecycle_should_support_all_core_mvp_operations()
    {
        // Arrange

        var createdAt = DateTimeOffset.UtcNow;

        var inventory = new InventoryItem(
            Guid.NewGuid(),
            onHandQuantity: 0,
            allocatedQuantity: 0,
            minimumQuantity: 5,
            reorderQuantity: 10,
            createdAt);

        // Receive

        inventory.Receive(
            quantity: 10,
            createdAt);

        inventory.OnHandQuantity.Should().Be(10);
        inventory.AvailableQuantity.Should().Be(10);

        // Allocate

        inventory.Allocate(
            quantity: 8,
            createdAt);

        inventory.OnHandQuantity.Should().Be(10);
        inventory.AllocatedQuantity.Should().Be(8);
        inventory.AvailableQuantity.Should().Be(2);

        // Replenishment

        inventory.NeedsReplenishment.Should().BeTrue();

        // Issue allocated inventory

        inventory.IssueAllocated(
            quantity: 8,
            createdAt);

        inventory.OnHandQuantity.Should().Be(2);
        inventory.AllocatedQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(2);

        // Replenishment still required

        inventory.NeedsReplenishment.Should().BeTrue();

        // Audit trail

        inventory.TransactionHistory.Should().HaveCount(3);
    }
}
