// <copyright file="Result.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;

namespace HL7Parser.Domain.Common
{
    /// <summary>
    /// Represents the outcome of an operation.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public sealed class Result<T>
    {
        private Result(bool isSuccess) => this.IsSuccess = isSuccess;

        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value => throw new NotImplementedException();

        /// <summary>
        /// Gets the error.
        /// </summary>
        public string Error { get; } = string.Empty;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="value">The value of the <see cref="Result{T}"/> object.</param>
        /// <returns>A successful <see cref="Result{T}"/> object.</returns>
        /// <exception cref="NotImplementedException">This method has not been implemented yet.</exception>
        public static Result<T> Success(T value) => throw new NotImplementedException();

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="error">A <see cref="string"/> explaining the cause of the failure.</param>
        /// <returns>A failed <see cref="Result{T}"/> object.</returns>
        /// <exception cref="NotImplementedException">This method has not been implemented yet.</exception>
        public static Result<T> Failure(string error) => throw new NotImplementedException();
    }
}
