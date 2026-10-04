namespace Domain.Exceptions;

public class BookingLimitReachingException(string message) : Exception(message);