namespace Domain.Exceptions;

public class BookingMustBeInPendingStatusException(string message) : Exception(message);