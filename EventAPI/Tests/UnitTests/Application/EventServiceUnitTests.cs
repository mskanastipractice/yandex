using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Entities.ValueObjects;
using Domain.Exceptions;
using FluentAssertions;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.Application;

public class EventServiceUnitTests: IDisposable
{
	private readonly DateTime _now = DateTime.UtcNow;
	private const int TotalSeats = 5;
	protected readonly IServiceProvider ServiceProvider;

	public EventServiceUnitTests()
	{
		var _databaseName = Guid.NewGuid().ToString();
		var services = new ServiceCollection();
		services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
		services.AddScoped<IEventRepository, EventRepository>();
		services.AddScoped<IEventService, EventService>();
		ServiceProvider = services.BuildServiceProvider();
		
		SeedDatabase();
	}
	
	private void SeedDatabase()
	{
		using var scope = ServiceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var now = DateTime.UtcNow;
		
		var count = 5;
		
		context.Events.AddRange(
			Event.Create(Guid.NewGuid(), "Новый год", "Праздник наступления Нового Года",
				EventPeriod.Create(now, now.AddDays(7)), 10),
			Event.Create(Guid.NewGuid(), "Пасха", "Празднование Пасхи",
				EventPeriod.Create(_now.AddMonths(-1), _now.AddMonths(-1).AddDays(2)), 10),
			Event.Create(Guid.NewGuid(), "Детская конференция", "Детские праздники и мероприятия",
				EventPeriod.Create(_now.AddHours(-10), _now.AddHours(-9)), 10),
			Event.Create(Guid.NewGuid(), "8 марта", "Международный женский день",
				EventPeriod.Create(_now.AddDays(-8), _now.AddDays(5)), 10),
			Event.Create(Guid.NewGuid(), "Весна и март",  "Международный день весны",
				EventPeriod.Create(_now.AddDays(-7), _now.AddDays(-6)), 10));

		context.SaveChanges();
	}
	
	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Create_ValidData_Success()
	{
		//Arrange
		var dto = new EventDto("8 марта", "Международный женский день",
			_now.AddMonths(-5), _now.AddMonths(-5).AddDays(2), 10);
		EventInfoDto result;

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			result = await service.CreateAsync(dto, CancellationToken.None);
		}

		//Assert
		Assert.NotNull(result);
		Assert.Equal(result.Title, dto.Title);
		Assert.Equal(result.Description, dto.Description);
		Assert.Equal(result.StartAt, dto.StartAt);
		Assert.Equal(result.EndAt, dto.EndAt);
	}

	/// <summary>
	/// Проверяет получение всех событий.
	/// </summary>
	[Fact]
	public async Task GetBy_ValidData_Success()
	{
		//Arrange
		PaginatedResultDto<EventInfoDto> result;

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			result = await service.GetAllAsync(new Filters(), 1, 10, CancellationToken.None);
		}

		//Assert
		Assert.NotNull(result);
		Assert.Equal(TotalSeats, result.Items.Count);
	}
	
	/// <summary>
	/// Проверяет получение события по ID.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		//Arrange
		EventInfoDto result;

		//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		var @event = await service.CreateAsync(new EventDto("Новый год", "Праздник наступления Нового Года", _now,
			_now.AddDays(7), 10), CancellationToken.None);
		result = await service.GetByIdAsync(@event.Id, CancellationToken.None);

		//Assert
		Assert.NotNull(result);
	}
	
	/// <summary>
	/// Проверяет удаление существующего события.
	/// </summary>
	[Fact]
	public async Task Remove_ValidData_Success()
	{
		//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var all = await service.GetAllAsync(new Filters(),1,1, CancellationToken.None);
		var firstid = all.Items.First().Id;
		await service.DeleteAsync(firstid, CancellationToken.None);

		//Assert
		Func<Task> act = () => service.GetByIdAsync(firstid, CancellationToken.None);
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{firstid}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет фильтрацию по наименованию.
	/// </summary>
	[Theory]
	[InlineData("Детская")]
	[InlineData("дЕТ")]
	[InlineData("ДЕТ")]
	[InlineData("конференция")]
	public async Task GetBy_FilterByTitle_Success(string title)
	{
		//Arrange
		PaginatedResultDto<EventInfoDto> result;
		
		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			result = await service.GetAllAsync(new Filters(Title: title), 1, 10, CancellationToken.None);
		}

		//Assert
		result.Should().NotBeNull();
		result.ItemsPerPage.Should().Be(1);
		var item = result.Items.Should().ContainSingle().Subject;
		item.Title.Should().Be("Детская конференция");
	}
	
	/// <summary>
	/// Проверяет фильтрацию по дате начала события.
	/// </summary>
	[Theory]
	[InlineData(1, 0)]
	[InlineData(-7, 3)]
	public async Task GetBy_FilterByFrom_Success(int daysToAdd, int totalItems)
	{
		//Arrange
		PaginatedResultDto<EventInfoDto> result;

		//Act
		using var scope = ServiceProvider.CreateScope();
		
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		result = await service.GetAllAsync(new Filters(From: _now.AddDays(daysToAdd)), 1, 10, CancellationToken.None);
		

		//Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Items.Should().OnlyContain(item => item.StartAt >= _now.AddDays(daysToAdd));
	}
	
	/// <summary>
	/// Проверяет пагинацию событий.
	/// </summary>
	[Theory]
	[InlineData(1, 2, 2)]
	[InlineData(2, 2, 2)]
	[InlineData(3, 1, 1)]
	public async Task GetBy_Pagination_Success(int page, int pageSize, int itemsPerPage)
	{
		//Arrange
		PaginatedResultDto<EventInfoDto> result;

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			result = await service.GetAllAsync(new Filters(), page, pageSize, CancellationToken.None);
		}

		//Assert
		result.TotalItems.Should().Be(TotalSeats);
		result.CurrentPage.Should().Be(page);
		result.ItemsPerPage.Should().Be(itemsPerPage);
		result.Items.Count.Should().Be(itemsPerPage);
	}
	
	/// <summary>
	/// Проверяет получение события по несуществующему ID.
	/// </summary>
	[Fact]
	public async Task GetBy_CombinedFilterBy_Success()
	{
		//Arrange
		PaginatedResultDto<EventInfoDto> result;

		//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		result = await service.GetAllAsync(new Filters(Title: "Март", _now.AddDays(-10), _now.AddDays(6)), 1, 10, CancellationToken.None);
		

		//Assert
		result.Items.Count.Should().Be(2);
	}
	
	/// <summary>
	/// Проверяет получение события по несуществующему ID.
	/// </summary>
	[Fact]
	public async Task GetById_InvalidData_Failed()
	{
		//Arrange
		Guid id = Guid.NewGuid();

		//Act//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		Func<Task> act = () =>
			service.GetByIdAsync(id, CancellationToken.None);

		//Assert
		await act.Should()
			.ThrowAsync<EntityNotFoundException>()
			.WithMessage(
				$"Сущность [Событие] с идентификатором [{id}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет обновление события с несуществующим ID.
	/// </summary>
	[Fact]
	public async Task Update_InvalidID_Failed()
	{
		//Arrange
		Guid id = Guid.NewGuid();
		var dto = new EventDto( "Новые данные", "Новые данные", _now, _now.AddDays(-1), 10);

		//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		Func<Task> act = () => service.UpdateAsync(id, dto,CancellationToken.None);
		
		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"Сущность [Событие] с идентификатором [{id}] не найдена.");
		var result = await service.GetAllAsync(new Filters(), 1, 10, CancellationToken.None);
		result.Items.Count.Should().Be(TotalSeats);
	}
	
	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Add_InvalidData_Failed()
	{
		//Arrange
		Guid id = Guid.NewGuid();
		var dto = new EventDto("День семьи", "Семейный праздник на площади", default, default, 10);

		//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		Func<Task> act = () => service.CreateAsync(dto, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<ArgumentException>();
		Func<Task> act2 = () => service.GetByIdAsync(id, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"Сущность [Событие] с идентификатором [{id}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Update_InvalidData_Failed()
	{
		//Arrange
		var dto = new EventDto("Новый год", "Праздник наступления Нового Года", _now, _now.AddDays(-1), 10);

		//Act
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var result = await service.CreateAsync(new EventDto( "Новый год", "Праздник наступления Нового Года", _now,
			_now.AddDays(7), 10), CancellationToken.None);
		Func<Task> act = () => service.UpdateAsync(result.Id, dto,CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<ArgumentException>().WithMessage("Начало события должно быть раньше его завершения.");
		var @event = await service.GetByIdAsync(result.Id,CancellationToken.None);
		@event.Title.Should().Be("Новый год");
		@event.Description.Should().Be("Праздник наступления Нового Года");
		@event.StartAt.Should().Be(_now);
		@event.EndAt.Should().Be(_now.AddDays(7));
	}
	
	public void Dispose()
	{
		if (ServiceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}