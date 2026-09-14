using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Application.Purchasing.RTV;
using Stockpile.Application.Tests.Fakes;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;

namespace Stockpile.Domain.Tests.Purchasing;

public sealed class RtvTests
{
    [Fact]
    public async Task Rtv_should_reduce_received_quantity()
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
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "PO123");

        // Receive 10
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 10);

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            3,
            createdAt.AddMinutes(6));

        await handler.HandleAsync(rtvCommand);

        line.ReceivedQuantity.Should().Be(7);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(6));
    }

    [Fact]
    public async Task Rtv_should_reopen_purchase_if_line_becomes_incomplete()
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
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "PO123");

        // Receive full quantity
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 10);
        purchase.Status.Should().Be(PurchaseStatus.Completed);

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            2,
            createdAt.AddMinutes(6));

        await handler.HandleAsync(rtvCommand);

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        line.ReceivedQuantity.Should().Be(8);
    }

    [Fact]
    public async Task Rtv_more_than_received_should_throw()
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
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Order(createdAt.AddMinutes(4), "PO123");

        // Receive 4
        purchase.RecordReceipt(createdAt.AddMinutes(5), line.Id, 4);

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            5, // exceeds received
            createdAt.AddMinutes(6));

        Func<Task> act = async () => await handler.HandleAsync(rtvCommand);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot return more than the received quantity.");
    }

    [Fact]
    public async Task Rtv_is_saved_even_when_purchase_rejects()
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
            10,
            0,
            createdAt.AddMinutes(1),
            new Money(10.00m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(2), line);
        purchase.Submit(createdAt.AddMinutes(3));
        purchase.Cancel(createdAt.AddMinutes(4)); // invalid status for RTV

        var purchaseRepository = new FakePurchaseRepository { Purchase = purchase };
        var rtvRepository = new FakeRtvRepository();

        var handler = new RecordRtvHandler(purchaseRepository, rtvRepository);

        var rtvCommand = new RecordRtvCommand(
            purchase.Id,
            line.Id,
            3,
            createdAt.AddMinutes(5));

        Func<Task> act = async () => await handler.HandleAsync(rtvCommand);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("RTVs can only be recorded for ordered purchases.");


        rtvRepository.Rtvs.Should().HaveCount(1);
        rtvRepository.AddedRtv!.PurchaseId.Should().Be(purchase.Id);
    }

    [Fact]
    public void Duplicate_rtv_lines_should_throw()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var rtv = new ReturnToVendor(Guid.NewGuid(), createdAt);

        var line = new RtvLine(Guid.NewGuid(), 2, createdAt.AddMinutes(1));

        rtv.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => rtv.AddLine(createdAt.AddMinutes(3), line);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate lines cannot be added to the same return to vendor.");
    }

    [Fact]
    public void Duplicate_purchaseLineIds_should_throw()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var purchaseLineId = Guid.NewGuid();

        var rtv = new ReturnToVendor(Guid.NewGuid(), createdAt);

        var line1 = new RtvLine(purchaseLineId, 2, createdAt.AddMinutes(1));
        var line2 = new RtvLine(purchaseLineId, 3, createdAt.AddMinutes(2));

        rtv.AddLine(createdAt.AddMinutes(3), line1);

        Action act = () => rtv.AddLine(createdAt.AddMinutes(4), line2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Duplicate purchase lines cannot be referenced in the same return to vendor.");
    }

    [Fact]
    public void Rtv_timestamp_must_be_monotonic()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var rtv = new ReturnToVendor(Guid.NewGuid(), createdAt);

        var line = new RtvLine(Guid.NewGuid(), 2, createdAt.AddMinutes(1));

        rtv.AddLine(createdAt.AddMinutes(2), line);

        Action act = () => rtv.AddLine(createdAt.AddMinutes(1),
            new RtvLine(Guid.NewGuid(), 1, createdAt.AddMinutes(1)));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The changed timestamp cannot precede the current updated timestamp.*");
    }


}
