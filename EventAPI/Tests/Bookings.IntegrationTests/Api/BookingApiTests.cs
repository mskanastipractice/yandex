using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bookings.Api.Contracts.Bookings;
using Bookings.IntegrationTests.Api.Base;
using FluentAssertions;
using Xunit;

namespace Bookings.IntegrationTests.Api;

public class BookingApiTests : BaseApiTest
{
    public BookingApiTests(ApiFixture fixture) : base(fixture)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    #region GetById method

    /// <summary>
    /// Проверяет получение брони по идентификатору.
    /// </summary>
    [Fact]
    public async Task GetById_WhenValidData_ShouldReturn200()
    {
        //Arrange
        Guid? bookingId = null;
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            var booking = dbContext.Bookings.Add(Booking.Domain.Booking.Create(TestData.EventId, TestData.UserId));
            bookingId = booking.Entity.Id;
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.GetAsync($"/bookings/{bookingId}");

        //Assert
        var responseData = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TestData.EventId, responseData.EventId);
    }

    /// <summary>
    /// Проверяет получение несуществующего бронирования.
    /// </summary>
    [Fact]
    public async Task GetById_WhenNonExistentBooking_ShouldReturn404()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.GetAsync($"/bookings/{Guid.NewGuid()}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create method
    /// <summary>
    /// Проверяет создание заявки на бронирование.
    /// </summary>
    [Fact]
    public async Task Book_WhenValidData_ShouldReturn202()
    {
        //Act
        var response = await Client.PostAsync($"/bookings/{TestData.EventId}", null);

        //Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var booking = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;
        Assert.Equal(new Uri($"/Bookings/{booking.BookingId}", UriKind.Relative), response.Headers.Location);
        Assert.Equal(TestData.EventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);

        var createdBooking = (await Client.GetFromJsonAsync<BookingResponse>($"/bookings/{booking.BookingId}"))!; //TODO dbContext
        Assert.Equal(booking.BookingId, createdBooking.BookingId);
        Assert.Equal(TestData.EventId, createdBooking.EventId);
        Assert.Equal(BookingStatus.Pending, createdBooking.Status);
    }

    /// <summary>
    /// Проверяет параллельное создание заявок на бронирование.
    /// </summary>
    [Fact]
    public async Task Book_WhenMultipleValidRequests_ShouldWorkCorrectly()
    {
        //Arrange
        var totalRequests = 10;
        var responses = new ConcurrentBag<HttpResponseMessage>();
        var eventId = Guid.NewGuid();

        //Act
        var tasks = Enumerable.Range(0, totalRequests)
            .Select(_ => Task.Run(async () =>
            {
                var response = await Client.PostAsync($"/bookings/{eventId}", null);
                responses.Add(response);
            })).ToArray();

        await Task.WhenAll(tasks);

        //Assert
        responses.Should().HaveCount(totalRequests);
        responses.Should().OnlyContain(x => x.StatusCode == HttpStatusCode.Accepted);
        responses.Select(x => x.Headers.Location).Distinct().Should().HaveCount(totalRequests);

        foreach (var response in responses)
        {
            var bookingByIdResponse = await Client.GetAsync(response.Headers.Location); //TODO dbContext
            bookingByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<BookingResponse>();
            responseData.Should().NotBeNull();
            responseData.EventId.Should().Be(eventId);
            responseData.Status.Should().Be(BookingStatus.Pending);
        }
    }

    #endregion

    #region Cancel method

    /// <summary>
    /// Проверяет удаление несуществующего бронирования.
    /// </summary>
    [Fact]
    public async Task Cancel_WhenNoData_ShouldReturn400()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Bookings.Add(Booking.Domain.Booking.Create(TestData.EventId, TestData.UserId));
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.DeleteAsync($"/bookings/{TestData.BookingId}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}