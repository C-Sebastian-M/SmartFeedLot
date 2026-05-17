namespace Feedlot.Application.Common;

/// <summary>
/// Result pattern. Evita excepciones para flujos de control esperados
/// y hace explícito el éxito/fallo en la firma del método.
/// 
/// Los Handlers retornan Result&lt;T&gt; en lugar de lanzar excepciones para
/// errores de negocio esperados (validación, not found).
/// Las excepciones de dominio (invariantes) sí se propagan — el
/// ExceptionHandlingMiddleware en la API las intercepta.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        ErrorType = ResultErrorType.None;
    }

    private Result(string error, ResultErrorType errorType)
    {
        IsSuccess = false;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error,
        ResultErrorType errorType = ResultErrorType.BusinessRule)
        => new(error, errorType);

    public static Result<T> NotFound(string error)
        => new(error, ResultErrorType.NotFound);

    public static Result<T> Conflict(string error)
        => new(error, ResultErrorType.Conflict);

    public static Result<T> Validation(string error)
        => new(error, ResultErrorType.Validation);
}

/// <summary>Result sin valor de retorno (para Commands que solo confirman éxito).</summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ResultErrorType.None);

    public static Result Failure(string error,
        ResultErrorType errorType = ResultErrorType.BusinessRule)
        => new(false, error, errorType);

    public static Result NotFound(string error)
        => new(false, error, ResultErrorType.NotFound);

    public static Result Conflict(string error)
        => new(false, error, ResultErrorType.Conflict);
}

public enum ResultErrorType
{
    None,
    NotFound,
    Conflict,
    Validation,
    BusinessRule
}
