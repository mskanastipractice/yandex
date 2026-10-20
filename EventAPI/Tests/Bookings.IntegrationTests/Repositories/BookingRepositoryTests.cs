using Booking.Domain.Enums;
using Booking.Infrastructure.Repositories;
using Bookings.IntegrationTests.Repositories.Base;
using FluentAssertions;
using Xunit;

namespace Bookings.IntegrationTests.Repositories;

using Bookings = Booking.Domain.Booking;

[Collection("Database collection")]
public class BookingRepositoryTests(DbFixture dbFixture) : BaseRepositoryTest(dbFixture)
{
    /// <summary>
    /// Проверяет поиск брони по идентификатору.
    /// </summary>
    [Fact]
    public async Task Find_WhenValidData_ShouldReturnBooking()
    {
        // Arrange
        var booking = Bookings.Create(TestData.EventId, TestData.UserId);
        Guid bookingId = booking.Id;

        await using (var context = DbFixture.CreateContext())
        {
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        await using (var context = DbFixture.CreateContext())
        {
            // Act
            var result = await new BookingRepository(context).FindAsync(bookingId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(bookingId);
            result.EventId.Should().Be(TestData.EventId);
            result.Status.Should().Be(BookingStatus.Pending);
        }
    }

    /// <summary>
    /// Проверяет поиск несуществующей брони по идентификатору.
    /// </summary>
    [Fact]
    public async Task Find_WhenBookingDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var context = DbFixture.CreateContext();

        // Act
        var result = await new BookingRepository(context).FindAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет получение необработанных броней.
    /// </summary>
    [Fact]
    public async Task GetPending_WhenDifferentStatuses_ShouldReturnOnlyPendingBookings()
    {
        // Arrange
        var pendingBooking1 = Bookings.Create(TestData.EventId, TestData.UserId);
        var pendingBooking2 = Bookings.Create(TestData.EventId, TestData.UserId);
        var confirmedBooking = Bookings.Create(TestData.EventId, TestData.UserId);
        var rejectedBooking = Bookings.Create(TestData.EventId, TestData.UserId);

        confirmedBooking.Confirm(DateTime.UtcNow);
        rejectedBooking.Reject(DateTime.UtcNow);

        await using (var context = DbFixture.CreateContext())
        {
            context.Bookings.AddRange(pendingBooking1, pendingBooking2, confirmedBooking, rejectedBooking);
            await context.SaveChangesAsync();
        }

        await using (var context = DbFixture.CreateContext())
        {
            // Act
            var result = await new BookingRepository(context).GetPendingAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain([pendingBooking1.Id, pendingBooking2.Id]);
            result.Should().NotContain([confirmedBooking.Id, rejectedBooking.Id]);
        }
    }

    /// <summary>
    /// Проверяет сохранение брони (через цепочку Add + SaveChangesAsync).
    /// </summary>
    [Fact]
    public async Task Add_WhenValidData_ShouldSaveCorrectly()
    {
        // Arrange
        Guid? bookingId;
        await using (var context = DbFixture.CreateContext())
        {
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = DbFixture.CreateContext())
        {
            var repository = new BookingRepository(context);
            bookingId = repository.Add(Bookings.Create(TestData.EventId, TestData.UserId));
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using (var context = DbFixture.CreateContext())
        {
            var result = await context.Bookings.FindAsync(bookingId);

            result.Should().NotBeNull();
            result.Id.Should().Be(bookingId.Value);
            result.EventId.Should().Be(TestData.EventId);
            result.Status.Should().Be(BookingStatus.Pending);
            result.ProcessedAt.Should().BeNull();
        }
    }

    /// <summary>
    /// Проверяет подтверждение заявки на бронь (через цепочку Find + Confirm + SaveChangesAsync).
    /// </summary>
    [Fact]
    public async Task Confirm_WhenValidData_ShouldSaveCorrectly()
    {
        // Arrange
        Guid? bookingId;
        var processedAt = new DateTime(2022, 04, 05, 15, 37, 00, DateTimeKind.Utc);
        await using (var context = DbFixture.CreateContext())
        {
            var booking = context.Bookings.Add(Bookings.Create(TestData.EventId, TestData.UserId));
            bookingId = booking.Entity.Id;
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = DbFixture.CreateContext())
        {
            var repository = new BookingRepository(context);
            var booking = (await repository.FindAsync(bookingId.Value, CancellationToken.None))!;
            booking.Confirm(processedAt);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using (var context = DbFixture.CreateContext())
        {
            var result = await context.Bookings.FindAsync(bookingId.Value);

            result.Should().NotBeNull();
            result.EventId.Should().Be(TestData.EventId);
            result.Status.Should().Be(BookingStatus.Confirmed);
            result.ProcessedAt.Should().Be(processedAt);
        }
    }

    /// <summary>
    /// Проверяет отклонение заявки на бронь (через цепочку Find + Reject + SaveChangesAsync).
    /// </summary>
    [Fact]
    public async Task Reject_WhenValidData_ShouldSaveCorrectly()
    {
        // Arrange
        Guid? bookingId;
        var processedAt = new DateTime(2022, 04, 05, 15, 37, 00, DateTimeKind.Utc);
        await using (var context = DbFixture.CreateContext())
        {
            var booking = context.Bookings.Add(Bookings.Create(TestData.EventId, TestData.UserId));
            bookingId = booking.Entity.Id;
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = DbFixture.CreateContext())
        {
            var repository = new BookingRepository(context);
            var booking = (await repository.FindAsync(bookingId.Value, CancellationToken.None))!;
            booking.Reject(processedAt);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        // Assert
        await using (var context = DbFixture.CreateContext())
        {
            var result = await context.Bookings.FindAsync(bookingId.Value);

            result.Should().NotBeNull();
            result.Id.Should().Be(bookingId.Value);
            result.EventId.Should().Be(TestData.EventId);
            result.Status.Should().Be(BookingStatus.Rejected);
            result.ProcessedAt.Should().Be(processedAt);
        }
    }
}