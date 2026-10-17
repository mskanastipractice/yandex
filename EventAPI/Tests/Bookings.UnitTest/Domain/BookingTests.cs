using Booking.Domain.Enums;
using Booking.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Bookings.UnitTest.Domain;
using Bookings = Booking.Domain.Booking;

public class BookingTests
{
    /// <summary>
    /// Проверяет создание бронирования.
    /// </summary>
    [Fact]
    public void Create_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var booking = Bookings.Create(eventId, userId);

        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    /// Проверяет подтверждение бронирования.
    /// </summary>
    [Fact]
    public void Confirm_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var confirmedAt = DateTime.UtcNow.AddDays(5).AddHours(4).AddMinutes(8);
        var booking = Bookings.Create(eventId, userId);

        booking.Confirm(confirmedAt);

        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().Be(confirmedAt);
    }

    /// <summary>
    /// Проверяет отклонение бронирования.
    /// </summary>
    [Fact]
    public void Reject_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rejectedAt = DateTime.UtcNow.AddDays(1).AddHours(6).AddMinutes(20);
        var booking = Bookings.Create(eventId, userId);

        booking.Reject(rejectedAt);

        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.Status.Should().Be(BookingStatus.Rejected);
        booking.ProcessedAt.Should().Be(rejectedAt);
    }

    /// <summary>
    /// Проверяет отмену бронирования.
    /// </summary>
    [Fact]
    public void Cancel_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cancelledAt = DateTime.UtcNow.AddDays(1).AddHours(6).AddMinutes(20);
        var booking = Bookings.Create(eventId, userId);

        booking.Cancel(cancelledAt);

        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.ProcessedAt.Should().Be(cancelledAt);
    }

    /// <summary>
    /// Проверяет подтверждение отклоненного бронирования.
    /// </summary>
    [Fact]
    public void Confirm_WhenBookingIsReject_ShouldThrowBookingMustBeInPendingStatusException()
    {
        var booking = Bookings.Create(Guid.NewGuid(), Guid.NewGuid());
        booking.Reject(DateTime.UtcNow);

        Assert.Throws<BookingMustBeInPendingStatusException>(() => booking.Confirm(DateTime.UtcNow));
    }
}