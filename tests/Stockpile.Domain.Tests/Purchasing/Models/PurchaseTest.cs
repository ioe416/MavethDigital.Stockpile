using FluentAssertions;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;


namespace Stockpile.Domain.Tests.Purchasing.Models;

public sealed class PurchaseTest
{
    [Fact]
    public void New_purchase_starts_as_draft()
    {
        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);


        purchase.Status.Should().Be(
            PurchaseStatus.Draft);
    }

    [Fact]
    public void Empty_vendor_id_throws_exception()
    {
        Action act = () => new Purchase(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);


        act.Should().Throw<ArgumentException>()
            .WithParameterName("vendorId")
            .WithMessage("A vendor is required.*");
            
    }

    [Fact]
    public void Empty_department_id_throws_exception()
    {
        Action act = () => new Purchase(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);


        act.Should().Throw<ArgumentException>()
            .WithParameterName("departmentId")
            .WithMessage("A department is required.*");
    }

    [Fact]
    public void Empty_requested_by_employee_id_throws_exception()
    {
        Action act = () => new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            DateTimeOffset.UtcNow);


        act.Should().Throw<ArgumentException>()
            .WithParameterName("requestedByEmployeeId")
            .WithMessage("A requesting employee is required.*");
    }

    [Fact]
    public void A_draft_should_be_able_to_be_submitted()
    {
        var submittedAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            submittedAt);

        purchase.Submit(submittedAt.AddMinutes(2));

        purchase.Status.Should().Be(PurchaseStatus.Requested);
        purchase.UpdatedAt.Should().Be(submittedAt.AddMinutes(2));
    }

    [Fact]
    public void A_non_draft_purchase_should_not_be_able_to_be_submitted()
    {
        var submittedAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            submittedAt);

        purchase.Submit(submittedAt.AddMinutes(2));

        Action act = () => purchase.Submit(submittedAt.AddMinutes(3));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only draft purchases can be submitted.");
    }

    [Fact]
    public void Submitted_time_before_CreatedAt_should_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        Action act = () => purchase.Submit(createdAt.AddMinutes(-1));

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage(
                "The changed timestamp cannot precede the current updated timestamp.*");

    }

    [Fact]
    public void A_requested_purchase_should_be_able_to_be_ordered()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(2));

        var orderedAt = createdAt.AddMinutes(3);

        var poNumber = "123456";

        purchase.Order(orderedAt, poNumber);

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        purchase.PurchaseOrderNumber.Should().Be(poNumber);
        purchase.UpdatedAt.Should().Be(orderedAt);
    }

    [Fact]
    public void Non_requested_purchase_should_not_be_able_to_be_ordered()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        Action act = () => purchase.Order(createdAt.AddMinutes(2), "123456");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only requested purchases can be ordered.");
    }

    [Fact]
    public void Empty_purchaseOrder_number_should_be_rejected()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(1));

        Action act = () => purchase.Order(createdAt.AddMinutes(2), string.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("A purchase order number is required.*")
            .WithParameterName("poNumber");

    }

    [Fact]
    public void Whitespace_only_purchaseOrder_number_should_be_rejected()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(1));

        Action act = () => purchase.Order(createdAt.AddMinutes(2), "   ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("A purchase order number is required.*")
            .WithParameterName("poNumber");

    }

    [Fact] 
    public void Purchase_order_number_is_normalized_before_storage()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(), 
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(1));

        var orderedAt = createdAt.AddMinutes(2);

        purchase.Order(orderedAt, "   123456   ");

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        purchase.PurchaseOrderNumber.Should().Be("123456");
        purchase.UpdatedAt.Should().Be(orderedAt);
    }

    [Fact]
    public void OrderedAt_before_current_updatedAt_should_be_rejected()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(2));

        Action act = () => purchase.Order(createdAt.AddMinutes(1), "123456");

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage(
                "The changed timestamp cannot precede the current updated timestamp.*");

    }

    [Fact]
    public void When_orderedAt_invalid_order_should_fail_and_remain_requested()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(2));

        Action act = () => purchase.Order(createdAt.AddMinutes(-3), "123456");

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage(
                "The changed timestamp cannot precede the current updated timestamp.*");

        purchase.Status.Should().Be(PurchaseStatus.Requested);
        purchase.PurchaseOrderNumber.Should().BeNull();

    }
}
            