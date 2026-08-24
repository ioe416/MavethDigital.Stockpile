using FluentAssertions;
using MavethDigital.Forge.Core.ValueObjects;
using Stockpile.Domain.Purchasing.Enums;
using Stockpile.Domain.Purchasing.Models;
using System.Diagnostics;


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

    [Fact]
    public void Purchase_should_accept_purchase_line()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(1), line);

        purchase.Lines.Should().HaveCount(1);
        purchase.Lines.Should().Contain(line);

    }

    [Fact]
    public void Adding_line_should_update_purchase_updated_at()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(1), line);

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(1));

    }

    [Fact]
    public void Adding_line_timestamped_before_purchase_last_update_timestamp_should_fail()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(1),
            unitPrice);

        Action act = () => purchase.AddLine(createdAt.AddMinutes(-1), line);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The changed timestamp cannot precede the current updated timestamp.*")
            .WithParameterName("changedAt");

        purchase.UpdatedAt.Should().Be(createdAt);
        purchase.Lines.Should().NotContain(line);

    }

    [Fact]
    public void A_null_purchase_line_should_be_rejected()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        Action act = () => purchase.AddLine(createdAt.AddMinutes(1), null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("line");
    }

    [Fact]
    public void A_line_cannot_be_added_to_an_ordered_purchase()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(3),
            unitPrice);

        purchase.Submit(
            createdAt.AddMinutes(1));

        purchase.Order(
            createdAt.AddMinutes(2), "123456");

        Action act = () => purchase.AddLine(createdAt.AddMinutes(4), line);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Purchase lines cannot be added to an ordered purchase.");

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
        purchase.Lines.Should().NotContain(line);
    }

    [Fact]
    public void A_line_cannot_be_added_to_a_cancelled_purchase()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            1,
            createdAt.AddMinutes(3),
            unitPrice);

        purchase.Submit(
            createdAt.AddMinutes(1));

        purchase.Cancel(
            createdAt.AddMinutes(2));

        Action act = () => purchase.AddLine(createdAt.AddMinutes(4), line);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Purchase lines cannot be added to a cancelled purchase.");

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
        purchase.Lines.Should().NotContain(line);
    }

    [Fact]
    public void Cancelling_an_ordered_purchase_should_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(
            createdAt.AddMinutes(1));

        purchase.Order(
            createdAt.AddMinutes(2), "123456");

        Action act = () => purchase.Cancel(createdAt.AddMinutes(3));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("An ordered purchase cannot be cancelled.");

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));
        purchase.Status.Should().Be(PurchaseStatus.Ordered);
    }

    [Fact]
    public void Cancelling_a_cancelled_purchase_should_throw_exception()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var currencyCode = new CurrencyCode("USD");
        var unitPrice = new Money(1.25m, currencyCode);

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Cancel(
            createdAt.AddMinutes(1));

        Action act = () => purchase.Cancel(createdAt.AddMinutes(2));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A cancelled purchase cannot be cancelled.");

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
        purchase.Status.Should().Be(PurchaseStatus.Cancelled);
    }

    [Fact]
    public void A_draft_purchase_can_be_cancelled()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Cancel(createdAt.AddMinutes(2));

        purchase.Status.Should().Be(PurchaseStatus.Cancelled);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));

    }

    [Fact]
    public void A_requested_purchase_can_be_cancelled()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        purchase.Submit(createdAt.AddMinutes(1));

        purchase.Cancel(createdAt.AddMinutes(2));

        purchase.Status.Should().Be(PurchaseStatus.Cancelled);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(2));

    }

    [Fact]
    public void Updating_quantity_on_an_ordered_purchase_should_throw_and_leave_both_the_purchase_and_line_unchanged()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var unitPrice = new Money(1.25m, new CurrencyCode("USD"));

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Order(createdAt.AddMinutes(4), "123456");

        Action act = () => purchase.UpdateQuantity(createdAt.AddMinutes(5), line.Id, 15);
        
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity cannot be altered on an ordered purchase.");

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        line.Quantity.Should().Be(10);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
    }

    [Fact]
    public void Updating_quantity_on_an_cancelled_purchase_should_throw_and_leave_both_the_purchase_and_line_unchanged()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var unitPrice = new Money(1.25m, new CurrencyCode("USD"));

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Cancel(createdAt.AddMinutes(4));

        Action act = () => purchase.UpdateQuantity(createdAt.AddMinutes(5), line.Id, 15);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity cannot be altered on a cancelled purchase.");

        purchase.Status.Should().Be(PurchaseStatus.Cancelled);
        line.Quantity.Should().Be(10);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
    }

    [Fact]
    public void Updating_unit_price_on_an_ordered_purchase_should_throw_and_leave_both_the_purchase_and_line_unchanged()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var unitPrice = new Money(1.25m, new CurrencyCode("USD"));
        var newUnitPrice = new Money(1.52m, new CurrencyCode("USD"));

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Order(createdAt.AddMinutes(4), "123456");

        Action act = () => purchase.UpdateUnitPrice(createdAt.AddMinutes(5), line.Id, newUnitPrice);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unit Price cannot be altered on an ordered purchase.");

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        line.UnitPrice.Should().Be(unitPrice);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
    }

    [Fact]
    public void Updating_unit_price_on_a_cancelled_purchase_should_throw_and_leave_both_the_purchase_and_line_unchanged()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var unitPrice = new Money(1.25m, new CurrencyCode("USD"));
        var newUnitPrice = new Money(1.52m, new CurrencyCode("USD"));

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Cancel(createdAt.AddMinutes(4));

        Action act = () => purchase.UpdateUnitPrice(createdAt.AddMinutes(5), line.Id, newUnitPrice);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unit Price cannot be altered on a cancelled purchase.");

        purchase.Status.Should().Be(PurchaseStatus.Cancelled);
        line.UnitPrice.Should().Be(unitPrice);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
    }

    [Fact]
    public void A_purchase_line_with_unknown_unit_price_can_be_given_a_unit_price()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var newUnitPrice = new Money(1.52m, new CurrencyCode("USD"));

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            Guid.NewGuid(),
            10,
            createdAt.AddMinutes(1),
            null);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.UpdateUnitPrice(createdAt.AddMinutes(5), line.Id, newUnitPrice);

        purchase.Status.Should().Be(PurchaseStatus.Requested);
        line.UnitPrice.Should().Be(newUnitPrice);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(5));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(5));
    }

    [Fact]
    public void Updating_part_id_on_an_ordered_purchase_should_throw_and_leave_both_the_purchase_and_line_unchanged()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var unitPrice = new Money(1.25m, new CurrencyCode("USD"));
        var partId = Guid.NewGuid();
        var newPartId = Guid.NewGuid();

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            10,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Order(createdAt.AddMinutes(4), "123456");

        Action act = () => purchase.UpdatePartId(createdAt.AddMinutes(5), line.Id, newPartId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Part cannot be altered on an ordered purchase.");

        purchase.Status.Should().Be(PurchaseStatus.Ordered);
        line.PartId.Should().Be(partId);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
    }

    [Fact]
    public void Updating_part_id_on_a_cancelled_purchase_should_throw_and_leave_both_the_purchase_and_line_unchanged()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var unitPrice = new Money(1.25m, new CurrencyCode("USD"));
        var partId = Guid.NewGuid();
        var newPartId = Guid.NewGuid();

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        var line = new PurchaseLine(
            partId,
            10,
            createdAt.AddMinutes(1),
            unitPrice);

        purchase.AddLine(createdAt.AddMinutes(2), line);

        purchase.Submit(createdAt.AddMinutes(3));

        purchase.Cancel(createdAt.AddMinutes(4));

        Action act = () => purchase.UpdatePartId(createdAt.AddMinutes(5), line.Id, newPartId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Part cannot be altered on a cancelled purchase.");

        purchase.Status.Should().Be(PurchaseStatus.Cancelled);
        line.PartId.Should().Be(partId);
        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(1));
    }

    [Fact]
    public void PurchaseLine_update_timestamp_should_match_purchase_update_timestamp()
    {
        var partId = Guid.NewGuid();
        var newPartId = Guid.NewGuid();

        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        purchase.Submit(createdAt.AddMinutes(1));

        var line = new PurchaseLine(
            partId,
            1,
            createdAt.AddMinutes(2),
            new Money(1.25m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(3),
            line);

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(3));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(2));

        purchase.UpdatePartId(createdAt.AddMinutes(4), line.Id, newPartId);

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(4));
    }


    [Fact]
    public void PurchaseLine_update_timestamp_doesnt_match_purchase_update_timestamp_should_throw()
    {
        var partId = Guid.NewGuid();
        var newPartId = Guid.NewGuid();

        var createdAt = DateTimeOffset.UtcNow;

        var purchase = new Purchase(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt,
            null);

        purchase.Submit(createdAt.AddMinutes(1));

        var line = new PurchaseLine(
            partId,
            1,
            createdAt.AddMinutes(3),
            new Money(1.25m, new CurrencyCode("USD")));

        purchase.AddLine(createdAt.AddMinutes(5),
            line);

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(5));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(3));

        Action act  = () => purchase.UpdatePartId(
            createdAt.AddMinutes(4), 
            line.Id, 
            newPartId);

        act.Should().Throw<ArgumentException>();

        purchase.UpdatedAt.Should().Be(createdAt.AddMinutes(5));
        line.UpdatedAt.Should().Be(createdAt.AddMinutes(3));
        line.PartId.Should().Be(partId);
    }

}
