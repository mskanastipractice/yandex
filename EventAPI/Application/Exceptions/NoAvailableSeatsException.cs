namespace Application.Exceptions;

public class NoAvailableSeatsException(Guid entityId)
    : Exception($"Свободные места на событие с идентификатором [{entityId}] не найдены.");