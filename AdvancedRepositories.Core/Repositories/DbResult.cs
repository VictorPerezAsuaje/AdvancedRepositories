namespace AdvancedRepositories.Core.Repositories;

public enum DbResultType { Success, Fail, Exception }
public class DbResult
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public bool IsFailure => !IsSuccess;
    public DbResultType Type { get; }
    public Exception? ExceptionContent { get; }

    protected DbResult(bool isSuccess, string error, DbResultType type, Exception exception = null)
    {
        if (isSuccess && error != string.Empty && exception != null)
            throw new InvalidOperationException();

        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
        Type = type;
        ExceptionContent = exception;
    }

    public static DbResult Fail(string error) => new DbResult(false, error, DbResultType.Fail);
    public static DbResult<T> Fail<T>(string error) =>
        new DbResult<T>(default, false, error, DbResultType.Fail);

    public static DbResult Exception(string error, Exception exception = null) => new DbResult(false, error, DbResultType.Exception, exception);
    public static DbResult<T> Exception<T>(string error, Exception exception = null) =>
        new DbResult<T>(default, false, error, DbResultType.Exception, exception);

    public static DbResult Ok()
        => new DbResult(true, string.Empty, DbResultType.Success);
    public static DbResult<T> Ok<T>(T value)
        => new DbResult<T>(value, true, string.Empty, DbResultType.Success);
}

public class DbResult<T> : DbResult
{
    private readonly T _value;

    public T Value
    {
        get
        {
            if (!IsSuccess) throw new InvalidOperationException();
            return _value;
        }
    }

    protected internal DbResult(T value, bool isSuccess, string error, DbResultType type, Exception exception = null) : base(isSuccess, error, type, exception)
    {
        _value = value;
    }
}

