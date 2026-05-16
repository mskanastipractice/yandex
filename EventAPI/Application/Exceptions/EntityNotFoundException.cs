namespace Application.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке доступа к несуществующей сущности.
/// </summary>
/// <param name="entityName">Имя типа сущности (например, "Событие").</param>
/// <param name="entityId">Идентификатор отсутствующей сущности.</param>
public class EntityNotFoundException(string entityName, int entityId)
	: Exception($"Сущность [{entityName}] с идентификатором [{entityId}] не найдена.");