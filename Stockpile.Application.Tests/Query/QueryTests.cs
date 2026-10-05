
using FluentAssertions;
using Stockpile.Application.Query;
using Stockpile.Domain.Inventory.Enums;
using Stockpile.Domain.Inventory.Models;
namespace Stockpile.Application.Tests.Query;

public sealed class QueryTests
{
    [Fact]
    public async Task Should_return_only_parts_requiring_replenishment()
    {
        var inventoryItems = new List<InventoryItem>
    {
        new(
            Guid.NewGuid(),
            20,
            0,
            5,
            10,
            DateTimeOffset.UtcNow),

        new(
            Guid.NewGuid(),
            3,
            0,
            5,
            10,
            DateTimeOffset.UtcNow)
    };

        var repository =
            new FakeInventoryRepository(inventoryItems);

        var handler =
            new GetPartsRequiringReplenishmentQueryHandler(
                repository);

        var result = await handler.Handle(
            new GetPartsRequiringReplenishmentQuery(),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().AvailableQuantity.Should().Be(3);
    }

    [Fact]
    public async Task Should_return_empty_collection_when_no_parts_need_replenishment()
    {
        var inventoryItems = new List<InventoryItem>
    {
        new(
            Guid.NewGuid(),
            20,
            0,
            5,
            10,
            DateTimeOffset.UtcNow)
    };

        var repository =
            new FakeInventoryRepository(inventoryItems);

        var handler =
            new GetPartsRequiringReplenishmentQueryHandler(
                repository);

        var result = await handler.Handle(
            new GetPartsRequiringReplenishmentQuery(),
            CancellationToken.None);

        result.Should().BeEmpty();
    }


}
