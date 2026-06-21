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
	public void Create_ValidData_Success()
	{
		//Arrange
		var dto = new EventDto(1, "8 марта", "Международный женский день",
			_now.AddMonths(-5), _now.AddMonths(-5).AddDays(2));

		//Act
		var result = _service.Create(dto);

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
	public void GetBy_ValidData_Success()
	{
		//Arrange
		var totalCount = CreateEvents();

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
		CreateEvents();
		int id = 3;

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
		CreateEvents();
		int id = 3;

		//Act
		_service.Delete(id);

		//Assert
		Action act = () => _service.GetById(id);
		act.Should().Throw<EntityNotFoundException>().WithMessage("Сущность [Событие] с идентификатором [3] не найдена.");
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
	public void GetBy_Pagination_Success(int page, int pageSize, int itemsPerPage)
	{
		//Arrange
		var totalCount = CreateEvents();

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
		int id = 6;

		//Act
		Action act = () => _service.GetById(id);

		//Assert
		act.Should().Throw<EntityNotFoundException>().WithMessage("Сущность [Событие] с идентификатором [6] не найдена.");
	}
	
	/// <summary>
	/// Проверяет обновление события с несуществующим ID.
	/// </summary>
	[Fact]
	public void Update_InvalidID_Failed()
	{
		//Arrange
		int totalCount = CreateEvents();
		var dto = new EventDto(23, "Новые данные", "Новые данные", _now, _now.AddDays(-1));

		//Act
		Action act = () => new EventService().Update(23, dto);

		//Assert
		act.Should().Throw<EntityNotFoundException>().WithMessage("Сущность [Событие] с идентификатором [23] не найдена.");
		_service.GetAll(new Filters(), 1, 10).Items.Count.Should().Be(totalCount);
	}
	
	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public void Add_InvalidData_Failed()
	{
		//Arrange
		var dto = new EventDto(1, "День семьи", "Семейный праздник на площади", default, default);

		//Act
		Action act = () => _service.Create(dto);

		//Assert
		act.Should().Throw<ArgumentException>();
		Action act2 = () => _service.GetById(1);
		act2.Should().Throw<EntityNotFoundException>().WithMessage("Сущность [Событие] с идентификатором [1] не найдена.");
	}
	
	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public void Update_InvalidData_Failed()
	{
		//Arrange
		CreateEvents();
		var dto = new EventDto(1, "Новый год", "Праздник наступления Нового Года", _now, _now.AddDays(-1));

		//Act
		Action act = () => _service.Update(1, dto);

		//Assert
		act.Should().Throw<ArgumentException>().WithMessage("Начало события должно быть раньше его завершения.");
		var @event = _service.GetById(1);
		@event.Title.Should().Be("Новый год");
		@event.Description.Should().Be("Праздник наступления Нового Года");
		@event.StartAt.Should().Be(_now);
		@event.EndAt.Should().Be(_now.AddDays(7));
	}
	
	private int CreateEvents()
	{
		var count = 0;
		_service.Create(new EventDto(++count, "Новый год", "Праздник наступления Нового Года", _now, _now.AddDays(7)));
		_service.Create(new EventDto(++count, "Пасха", "Празднование Пасхи", _now.AddMonths(-1), _now.AddMonths(-1).AddDays(2)));
		_service.Create(new EventDto(++count, "Детская конференция", "Детские праздники и мероприятия", _now.AddHours(-10), _now.AddHours(-9)));
		_service.Create(new EventDto(++count, "8 марта", "Международный женский день", _now.AddDays(-8), _now.AddDays(5)));
		_service.Create(new EventDto(++count, "Весна и март", "Международный день весны", _now.AddDays(-7), _now.AddDays(-6)));
		return count;
	}
}