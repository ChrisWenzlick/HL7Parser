// <copyright file="Result.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;

namespace HL7Parser.Domain.Common;

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the value produced by a successful operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessing Value on a failed result.
    /// Check <see cref="IsSuccess"/> before accessing this property.
    /// </exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result. Check IsSuccess before accessing Value.");

    /// <summary>
    /// Gets the error.
    /// </summary>
    public string Error { get; } = string.Empty;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The value of the <see cref="Result{T}"/> object.</param>
    /// <returns>A successful <see cref="Result{T}"/> object.</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">A <see cref="string"/> explaining the cause of the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> object.</returns>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }
}
