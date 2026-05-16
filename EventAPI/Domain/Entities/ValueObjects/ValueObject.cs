namespace Domain.Entities.ValueObjects;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object? obj) => Equals(obj as ValueObject);
    
    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (GetType() != other.GetType()) return false;
        
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode() => 
        GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate((a, b) => a ^ b);
    
    public static bool operator ==(ValueObject? a, ValueObject? b) => 
        a?.Equals(b) ?? b is null;
    
    public static bool operator !=(ValueObject? a, ValueObject? b) => 
        !(a == b);
}