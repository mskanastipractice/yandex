using Application.Contracts;
using Application.Contracts.DTOs;
using Application.Exceptions;
using Application.Services;
using FluentAssertions;
using Xunit;

namespace Tests.Application;

public class EventServiceUnitTests
{
	private readonly DateTime _now = DateTime.UtcNow;
	private readonly EventService _service = new();

	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Create_ValidData_Success()
	{
		//Arrange
		var dto = new EventDto(Guid.NewGuid(), "8 марта", "Международный женский день",
			_now.AddMonths(-5), _now.AddMonths(-5).AddDays(2), 10);

		//Act
		var result = await _service.CreateAsync(dto);

		//Assert
		Assert.NotNull(result);
		Assert.Equal(result.Title, dto.Title);
		Assert.Equal(result.Description, dto.Description);
		Assert.Equal(result.StartAt, dto.StartAt);
		Assert.Equal(result.EndAt, dto.EndAt);
		Assert.Equal(result.Id, dto.Id);
	}

	/// <summary>
	/// Проверяет получение всех событий.
	/// </summary>
	[Fact]
	public async Task GetBy_ValidData_Success()
	{
		//Arrange
		var totalCount = await CreateEvents();

		//Act
		var result = _service.GetAll(new Filters(), 1, 10);

		//Assert
		Assert.NotNull(result);
		Assert.Equal(result.Items.Count, totalCount);
	}
	
	/// <summary>
	/// Проверяет получение события по ID.
	/// </summary>
	[Fact]
	public void GetById_ValidData_Success()
	{
		//Arrange
		Guid id = Guid.NewGuid();
		CreateEventAsync(id);

		//Act
		var result = _service.GetById(id);

		//Assert
		Assert.NotNull(result);
		Assert.Equal(id, result.Id);
	}
	
	/// <summary>
	/// Проверяет удаление существующего события.
	/// </summary>
	[Fact]
	public void Remove_ValidData_Success()
	{
		//Arrange
		Guid id = Guid.NewGuid();
		CreateEventAsync(id);

		//Act
		_service.Delete(id);

		//Assert
		Action act = () => _service.GetById(id);
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{id}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет фильтрацию по наименованию.
	/// </summary>
	[Theory]
	[InlineData("Детская")]
	[InlineData("дЕТ")]
	[InlineData("ДЕТ")]
	[InlineData("конференция")]
	public void GetBy_FilterByTitle_Success(string title)
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetAll(new Filters(Title: title), 1, 10);

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
	public void GetBy_FilterByFrom_Success(int daysToAdd, int totalItems)
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetAll(new Filters(From: _now.AddDays(daysToAdd)), 1, 10);

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
		var totalCount = await CreateEvents();

		//Act
		var result = _service.GetAll(new Filters(), page, pageSize);

		//Assert
		result.TotalItems.Should().Be(totalCount);
		result.CurrentPage.Should().Be(page);
		result.ItemsPerPage.Should().Be(itemsPerPage);
		result.Items.Count.Should().Be(itemsPerPage);
	}
	
	/// <summary>
	/// Проверяет получение события по несуществующему ID.
	/// </summary>
	[Fact]
	public void GetBy_CombinedFilterBy_Success()
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetAll(new Filters(Title: "Март", _now.AddDays(-10), _now.AddDays(6)), 1, 10);

		//Assert
		result.Items.Count.Should().Be(2);
	}
	
	/// <summary>
	/// Проверяет получение события по несуществующему ID.
	/// </summary>
	[Fact]
	public void GetById_InvalidData_Failed()
	{
		//Arrange
		CreateEvents();
		Guid id = Guid.NewGuid();

		//Act
		Action act = () => _service.GetById(id);

		//Assert
		act.Should().Throw<EntityNotFoundException>().WithMessage($"Сущность [Событие] с идентификатором [{id}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет обновление события с несуществующим ID.
	/// </summary>
	[Fact]
	public async Task Update_InvalidID_Failed()
	{
		//Arrange
		int totalCount = await CreateEvents();
		Guid id = Guid.NewGuid();
		var dto = new EventDto(id, "Новые данные", "Новые данные", _now, _now.AddDays(-1), 10);

		//Act
		Action act = () => new EventService().Update(id, dto);

		//Assert
		act.Should().Throw<EntityNotFoundException>().WithMessage($"Сущность [Событие] с идентификатором [{id}] не найдена.");
		_service.GetAll(new Filters(), 1, 10).Items.Count.Should().Be(totalCount);
	}
	
	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public void Add_InvalidData_Failed()
	{
		//Arrange
		Guid id = Guid.NewGuid();
		var dto = new EventDto(id, "День семьи", "Семейный праздник на площади", default, default, 10);

		//Act
		Func<Task> act = () => _service.CreateAsync(dto);

		//Assert
		act.Should().ThrowAsync<ArgumentException>();
		Action act2 = () => _service.GetById(id);
		act2.Should().Throw<EntityNotFoundException>().WithMessage($"Сущность [Событие] с идентификатором [{id}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Update_InvalidData_Failed()
	{
		//Arrange
		var id = Guid.NewGuid();
		await CreateEventAsync(id);
		var dto = new EventDto(id, "Новый год", "Праздник наступления Нового Года", _now, _now.AddDays(-1), 10);

		//Act
		Action act = () => _service.Update(id, dto);

		//Assert
		act.Should().Throw<ArgumentException>().WithMessage("Начало события должно быть раньше его завершения.");
		var @event = _service.GetById(id);
		@event.Title.Should().Be("Новый год");
		@event.Description.Should().Be("Праздник наступления Нового Года");
		@event.StartAt.Should().Be(_now);
		@event.EndAt.Should().Be(_now.AddDays(7));
	}
	
	private async Task CreateEventAsync(Guid guid)
	{
		await _service.CreateAsync(new EventDto(guid, "Новый год", "Праздник наступления Нового Года", _now, _now.AddDays(7), 10));
	}
	
	private async Task<int> CreateEvents()
	{
		var count = 5;
		await _service.CreateAsync(new EventDto(Guid.NewGuid(), "Новый год", "Праздник наступления Нового Года", _now, _now.AddDays(7), 10));
		await _service.CreateAsync(new EventDto(Guid.NewGuid(), "Пасха", "Празднование Пасхи", _now.AddMonths(-1), _now.AddMonths(-1).AddDays(2), 10));
		await _service.CreateAsync(new EventDto(Guid.NewGuid(),"Детская конференция", "Детские праздники и мероприятия", _now.AddHours(-10), _now.AddHours(-9), 10));
		await _service.CreateAsync(new EventDto(Guid.NewGuid(), "8 марта", "Международный женский день", _now.AddDays(-8), _now.AddDays(5), 10));
		await _service.CreateAsync(new EventDto(Guid.NewGuid(), "Весна и март", "Международный день весны", _now.AddDays(-7), _now.AddDays(-6), 10));
		return count;
	}
}