namespace Domain.Exceptions;

public class PastEventBookingException() : Exception("Попытка забронировать прошедшее событие.");