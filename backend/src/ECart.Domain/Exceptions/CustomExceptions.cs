namespace ECart.Domain.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public List<string>? Errors { get; }

    public AppException(string message, int statusCode = 500, List<string>? errors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with id '{key}' was not found.", 404)
    {
    }
}

public class ValidationException : AppException
{
    public ValidationException(string message, List<string>? errors = null) 
        : base(message, 400, errors)
    {
    }

    public ValidationException(List<string> errors) 
        : base("Validation failed.", 400, errors)
    {
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message, 400)
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication failed or token is invalid.") 
        : base(message, 401)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") 
        : base(message, 403)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409)
    {
    }
}
