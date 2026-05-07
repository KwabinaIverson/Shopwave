namespace Shopwave.Shared.Results;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message if the operation failed; otherwise, null.
    /// </summary>
    public string? Error { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">true if the operation was successful; otherwise, false.</param>
    /// <param name="error">The error message if the operation failed; otherwise, null or empty.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="isSuccess"/> is true but <paramref name="error"/> is not null or empty,
    /// or when <paramref name="isSuccess"/> is false but <paramref name="error"/> is null.
    /// </exception>
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Successful result cannot have an error message.");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
    }
    
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating success.</returns>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A <see cref="Result"/> indicating failure.</returns>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success with the value.</returns>
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the value (not used in failure case).</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A <see cref="Result{T}"/> indicating failure.</returns>
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Represents the result of an operation that returns a value, indicating success or failure.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value if the operation was successful; otherwise, the default value of <typeparamref name="T"/>.
    /// </summary>
    public T Value { get; }
  
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">true if the operation was successful; otherwise, false.</param>
    /// <param name="error">The error message if the operation failed; otherwise, null or empty.</param>
    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}