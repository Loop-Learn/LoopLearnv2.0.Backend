namespace LoopLearn.Entities.Helpers.Exceptions
{
    /// <summary>
    /// Base custom exception for application-specific errors
    /// </summary>
    public class ApplicationException : Exception
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public List<string> Errors { get; set; } = new();

        public ApplicationException(string message, int statusCode = 400, string errorCode = "APPLICATION_ERROR")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public ApplicationException(string message, List<string> errors, int statusCode = 400, string errorCode = "APPLICATION_ERROR")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Errors = errors;
        }
    }

    /// <summary>
    /// Exception thrown when a resource is not found
    /// </summary>
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message, string errorCode = "NOT_FOUND")
            : base(message, statusCode: 404, errorCode: errorCode)
        {
        }
    }

    /// <summary>
    /// Exception thrown when no data is available
    /// </summary>
    public class NoDataException : ApplicationException
    {
        public NoDataException(string message, string errorCode = "NO_DATA")
            : base(message, statusCode: 204, errorCode: errorCode)
        {
        }
    }

    /// <summary>
    /// Exception thrown when access is denied
    /// </summary>
    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException(string message, string errorCode = "UNAUTHORIZED")
            : base(message, statusCode: 401, errorCode: errorCode)
        {
        }
    }

    /// <summary>
    /// Exception thrown for internal server errors
    /// </summary>
    public class InternalServerException : ApplicationException
    {
        public InternalServerException(string message, List<string> errors = null, string errorCode = "INTERNAL_SERVER_ERROR")
            : base(message, errors ?? new(), statusCode: 500, errorCode: errorCode)
        {
        }
    }
}