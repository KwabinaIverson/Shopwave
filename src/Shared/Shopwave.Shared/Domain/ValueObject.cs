namespace Shopwave.Shared.Domain;

/// <summary>
/// Represents a base class for value objects, which are immutable objects compared by their values rather than identity.
/// Value objects should override <see cref="GetEqualityComponents"/> to provide the components that determine equality.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the components that determine equality for this value object.
    /// Subclasses must implement this method to return the properties or fields that should be used for equality comparison.
    /// </summary>
    /// <returns>An enumerable of objects representing the equality components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// Two value objects are equal if they are of the same type and their equality components are equal.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>true if the specified object is equal to the current value object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;
        
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code for the current value object based on its equality components.
    /// </summary>
    /// <returns>A hash code for the current value object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents().Aggregate(0, HashCode.Combine);
    }
}