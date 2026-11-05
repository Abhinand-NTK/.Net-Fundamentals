namespace PersonalDetailsApi.Common.Exceptions;

/// <summary>
/// Base for expected, client-facing errors. Each subclass decides its own HTTP status (polymorphism),
/// and the global exception handler turns it into an RFC 7807 problem response.
/// </summary>
public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }
    public abstract string Title { get; }
}

public sealed class NotFoundException(string resource, object key)
    : AppException($"{resource} with id '{key}' was not found.")
{
    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string Title => "Resource not found";
}

public sealed class ConflictException(string message) : AppException(message)
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string Title => "Conflict";
}
