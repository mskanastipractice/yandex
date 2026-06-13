namespace Domain.Entities.ValueObjects;

public class EventPeriod : ValueObject
{
    public DateTime StartAt { get; }
    public  DateTime EndAt { get; }
    
    private EventPeriod(DateTime startAt, DateTime endAt)
    {
        StartAt = startAt;
        EndAt = endAt;
    }
    
    public static EventPeriod Create(DateTime startAt, DateTime endAt)
    {
        if (endAt <= startAt)
        {
            throw new ArgumentException("Начало события должно быть раньше его завершения.");
        }
            
        return new EventPeriod(startAt, endAt);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartAt;
        yield return EndAt;
    }
}