namespace Booking.Application.Exceptions;

public class AccessDeniedException(string message) : Exception(message);