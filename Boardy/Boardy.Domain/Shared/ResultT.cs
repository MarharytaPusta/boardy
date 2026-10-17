using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Domain.Shared;

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, string? errorMessage, T? data) : base(isSuccess, errorMessage) 
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new Result<T>(true, null, data);
    public static new Result<T> Failure(string errorMesssage) => new Result<T>(false, errorMesssage, default);
}
