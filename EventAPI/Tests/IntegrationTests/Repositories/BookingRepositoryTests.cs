using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using Xunit;

namespace IntegrationTests.Repositories;

[Collection("Database collection")]
public class BookingRepositoryTests(DbFixture fixture) : BaseRepositoryTest(fixture)
{
    /// <summary>
    /// Проверяет получение существующей брони по её идентификатору.
    /// </summary>
    [Fact]
    public async Task Find_WhenBookingExists_ShouldReturnBooking()
    {
        var booking = Booking.Create(EventId);

        await using (var context = Fixture.CreateContext())
        {
            context.Events.Add(CreateEvent());
            context.Bookings.Add(booking);

            await context.SaveChangesAsync();
        }

        await using var db = Fixture.CreateContext();
        var repository = new BookingRepository(db);

        var actual = await repository.FindAsync(
            booking.Id,
            CancellationToken.None);

        actual.Should().NotBeNull();
        actual!.EventId.Should().Be(EventId);
        actual.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Проверяет, что при поиске отсутствующей брони возвращается null.
    /// </summary>
    [Fact]
    public async Task Find_WhenBookingIsMissing_ShouldReturnNull()
    {
        await using var context = Fixture.CreateContext();
        var repository = new BookingRepository(context);

        var actual = await repository.FindAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        actual.Should().BeNull();
    }

    /// <summary>
    /// Проверяет добавление новой брони и её сохранение в базе данных.
    /// </summary>
    [Fact]
    public async Task Add_WhenBookingIsValid_ShouldPersistBooking()
    {
        var booking = Booking.Create(EventId);

        await using (var context = Fixture.CreateContext())
        {
            context.Events.Add(CreateEvent());
            await context.SaveChangesAsync();
        }

        await using (var context = Fixture.CreateContext())
        {
            var repository = new BookingRepository(context);

            await repository.AddAsync(booking);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var db = Fixture.CreateContext();

        var savedBooking = await db.Bookings.FindAsync(booking.Id);

        savedBooking.Should().NotBeNull();
        savedBooking!.EventId.Should().Be(EventId);
        savedBooking.Status.Should().Be(BookingStatus.Pending);
        savedBooking.ProcessedAt.Should().BeNull();
    }

    /// <summary>
    /// Проверяет подтверждение ожидающей брони и сохранение нового статуса.
    /// </summary>
    [Fact]
    public async Task Confirm_WhenBookingIsPending_ShouldChangeStatus()
    {
        var booking = Booking.Create(EventId);
        var confirmationTime =
            new DateTime(2024, 06, 11, 14, 20, 00, DateTimeKind.Utc);

        await using (var context = Fixture.CreateContext())
        {
            context.Events.Add(CreateEvent());
            context.Bookings.Add(booking);

            await context.SaveChangesAsync();
        }

        await using (var context = Fixture.CreateContext())
        {
            var repository = new BookingRepository(context);

            var storedBooking = await repository.FindAsync(
                booking.Id,
                CancellationToken.None);

            storedBooking.Should().NotBeNull();

            storedBooking!.Confirm(confirmationTime);

            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var db = Fixture.CreateContext();

        var updatedBooking = await db.Bookings.FindAsync(booking.Id);

        updatedBooking.Should().NotBeNull();
        updatedBooking!.EventId.Should().Be(EventId);
        updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
        updatedBooking.ProcessedAt.Should().Be(confirmationTime);
    }

    /// <summary>
    /// Проверяет отклонение ожидающей брони и сохранение нового статуса.
    /// </summary>
    [Fact]
    public async Task Reject_WhenBookingIsPending_ShouldChangeStatus()
    {
        var booking = Booking.Create(EventId);
        var rejectionTime =
            new DateTime(2024, 06, 11, 16, 45, 00, DateTimeKind.Utc);

        await using (var context = Fixture.CreateContext())
        {
            context.Events.Add(CreateEvent());
            context.Bookings.Add(booking);

            await context.SaveChangesAsync();
        }

        await using (var context = Fixture.CreateContext())
        {
            var repository = new BookingRepository(context);

            var storedBooking = await repository.FindAsync(
                booking.Id,
                CancellationToken.None);

            storedBooking.Should().NotBeNull();

            storedBooking!.Reject(rejectionTime);

            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var db = Fixture.CreateContext();

        var updatedBooking = await db.Bookings.FindAsync(booking.Id);

        updatedBooking.Should().NotBeNull();
        updatedBooking!.EventId.Should().Be(EventId);
        updatedBooking.Status.Should().Be(BookingStatus.Rejected);
        updatedBooking.ProcessedAt.Should().Be(rejectionTime);
    }
}