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
            0,
            10,
            10,
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
            0,
            10,
            0,
            createdAt);

        var issuedQuantity = 4;
        var expectedOnHandQuantity = 6;

        // Act
        inventory.IssueUnallocated(issuedQuantity, createdAt.AddMinutes(1));

        // Assert
        inventory.OnHandQuantity.Should().Be(expectedOnHandQuantity);
        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType.Should().Be(TransactionType.Issue);
        inventory.TransactionHistory.First().Quantity.Should().Be(issuedQuantity);
    }

    [Fact]
    public void Issuing_more_than_available_should_allow_negative_inventory()
    {
        var createdAt = DateTimeOffset.UtcNow;

        //Arrange
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            5,
            0,
            5,
            5,
            createdAt);

        var issuedQuantity = 10;
        var expectedOnHandQuantity = -5;

        // Act
        inventory.IssueUnallocated(issuedQuantity, createdAt.AddMinutes(1));

        // Assert
        inventory.OnHandQuantity.Should().Be(expectedOnHandQuantity);
        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType.Should().Be(TransactionType.Issue);
        inventory.TransactionHistory.First().Quantity.Should().Be(issuedQuantity);
    }

    [Fact]
    public void Available_quantity_should_consider_allocations()
    {
        var createdAt = DateTimeOffset.UtcNow;

        //Arrange
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            0,
            5,
            5,
            createdAt);

        var allocatedQuantity = 4;

        // Act
        inventory.Allocate(allocatedQuantity, createdAt.AddMinutes(1));

        // Assert
        inventory.OnHandQuantity.Should().Be(10);
        inventory.AllocatedQuantity.Should().Be(4);
        inventory.AvailableQuantity.Should().Be(6);
        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType.Should().Be(TransactionType.Allocate);
        inventory.TransactionHistory.First().Quantity.Should().Be(allocatedQuantity);
    }

    [Fact]
    public void Available_quantity_greater_than_minimum_quantity_should_not_need_replenishment()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            0,
            5,
            5,
            DateTimeOffset.UtcNow);

        inventory.NeedsReplenishment.Should().BeFalse();
    }

    [Fact]
    public void Available_quantity_equal_to_minimum_quantity_should_not_need_replenishment()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            0,
            10,
            0,
            DateTimeOffset.UtcNow);

        inventory.NeedsReplenishment.Should().BeFalse();
    }

    [Fact]
    public void Available_quantity_less_than_minimum_quantity_should_need_replenishment()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            0,
            15,
            5,
            DateTimeOffset.UtcNow);

        inventory.NeedsReplenishment.Should().BeTrue();
    }

    [Fact]
    public void Negative_available_quantity_should_need_replenishment()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            5,
            10,
            5,
            5,
            DateTimeOffset.UtcNow);

        inventory.NeedsReplenishment.Should().BeTrue();
    }

    [Fact]
    public void Allocate_zero_quantity_should_throw()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            0,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.Allocate(
            0,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Allocate_negative_quantity_should_throw()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            0,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.Allocate(
            -1,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Deallocate_zero_quantity_should_throw()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.Deallocate(
            0,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Deallocate_negative_quantity_should_throw()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.Deallocate(
            -1,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Cannot_deallocate_more_than_allocated_quantity()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.Deallocate(
            6,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*allocated quantity*");
    }

    [Fact]
    public void Deallocate_should_reduce_allocated_quantity()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        inventory.Deallocate(
            3,
            DateTimeOffset.UtcNow);

        inventory.AllocatedQuantity.Should().Be(2);
        inventory.AvailableQuantity.Should().Be(8);

        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType
            .Should().Be(TransactionType.Deallocate);
    }

    [Fact]
    public void Issue_allocated_inventory_should_reduce_on_hand_and_allocated_quantity()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            6,
            5,
            5,
            createdAt);

        inventory.IssueAllocated(
            4,
            createdAt.AddMinutes(1));

        inventory.OnHandQuantity.Should().Be(6);
        inventory.AllocatedQuantity.Should().Be(2);
        inventory.AvailableQuantity.Should().Be(4);

        inventory.TransactionHistory.Should().HaveCount(1);
        inventory.TransactionHistory.First().TransactionType
            .Should().Be(TransactionType.Issue);

        inventory.TransactionHistory.First().Quantity
            .Should().Be(4);
    }

    [Fact]
    public void Cannot_issue_more_than_allocated_quantity()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.IssueAllocated(
            6,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*allocated quantity*");
    }

    [Fact]
    public void Issue_allocated_quantity_equal_to_allocation_should_clear_allocation()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        inventory.IssueAllocated(
            5,
            DateTimeOffset.UtcNow);

        inventory.OnHandQuantity.Should().Be(5);
        inventory.AllocatedQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(5);
    }

    [Fact]
    public void Issue_allocated_zero_quantity_should_throw()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.IssueAllocated(
            0,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Issue_allocated_negative_quantity_should_throw()
    {
        var inventory = new InventoryItem(
            Guid.NewGuid(),
            10,
            5,
            5,
            5,
            DateTimeOffset.UtcNow);

        Action act = () => inventory.IssueAllocated(
            -1,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*greater than zero*");
    }
}
