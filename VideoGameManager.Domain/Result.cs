using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VideoGameManager.Domain
{
    /// <summary>
    /// Outcome of an operation that either succeeds or fails because the input broke a rule.
    /// </summary>
    /// <remarks>
    /// A rejected input is an expected outcome, not an exceptional one, so it is returned
    /// rather than thrown. Infrastructure failures stay exceptions.
    /// </remarks>
    public sealed class Result
    {
        private static readonly Result Successful = new Result(true, ValidationResult.Ok);

        private Result(bool isSuccess, ValidationResult validation)
        {
            IsSuccess = isSuccess;
            Validation = validation;
        }

        /// <summary>
        /// <c>true</c> when the operation ran.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Why the operation was rejected. <see cref="ValidationResult.Ok"/> on success, so this
        /// is never <c>null</c>.
        /// </summary>
        public ValidationResult Validation { get; }

        /// <summary>
        /// Shortcut to <see cref="ValidationResult.Errors"/>. Empty on success.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => Validation.Errors;

        /// <summary>
        /// The outcome of an operation that ran.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success() => Successful;

        /// <summary>
        /// The outcome of an operation that was rejected.
        /// </summary>
        /// <param name="validation">The failed validation. Must carry at least one error.</param>
        /// <returns>A rejected result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validation"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="validation"/> reports no error.</exception>
        public static Result Invalid(ValidationResult validation)
        {
            if (validation == null)
            {
                throw new ArgumentNullException(nameof(validation));
            }

            if (validation.IsValid)
            {
                throw new ArgumentException("A rejected result needs at least one error.", nameof(validation));
            }

            return new Result(false, validation);
        }

        /// <summary>
        /// The outcome of an operation that was rejected, built from loose errors.
        /// </summary>
        /// <param name="errors">Errors to carry. At least one is required.</param>
        /// <returns>A rejected result.</returns>
        public static Result Invalid(params ValidationError[] errors) =>
            Invalid(ValidationResult.Failed(errors));
    }

    /// <summary>
    /// Outcome of an operation that returns a value when it succeeds.
    /// </summary>
    /// <typeparam name="T">Type of the value produced on success.</typeparam>
    /// <remarks>
    /// This type does not derive from <see cref="Result"/> on purpose. Static factory members
    /// are inherited in C#, so a derived <c>Result&lt;T&gt;</c> would also expose
    /// <c>Result.Invalid</c>, which returns the non-generic type and therefore cannot be
    /// returned from a method declared as <c>Result&lt;T&gt;</c>. Keeping the two types
    /// independent makes every factory return exactly the type it is called on.
    /// </remarks>
    public sealed class Result<T>
    {
        private readonly T? _value;

        private Result(bool isSuccess, T? value, ValidationResult validation)
        {
            IsSuccess = isSuccess;
            _value = value;
            Validation = validation;
        }

        /// <summary>
        /// <c>true</c> when the operation ran and produced a value.
        /// </summary>
        /// <remarks>
        /// The attribute states the invariant the two factory methods below establish and
        /// nothing else can break: the stored value is present exactly when this is
        /// <c>true</c>. It is what lets <see cref="Value"/> hand the value back without
        /// claiming, against the compiler, that a field which really can be absent never is.
        /// </remarks>
        [MemberNotNullWhen(true, nameof(_value))]
        public bool IsSuccess { get; }

        /// <summary>
        /// Why the operation was rejected. <see cref="ValidationResult.Ok"/> on success, so this
        /// is never <c>null</c>.
        /// </summary>
        public ValidationResult Validation { get; }

        /// <summary>
        /// Shortcut to <see cref="ValidationResult.Errors"/>. Empty on success.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => Validation.Errors;

        /// <summary>
        /// The produced value.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The operation was rejected. Reading a value that was never produced is a bug in the
        /// caller, so it fails loudly instead of returning a default.
        /// </exception>
        public T Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException(
                        "The operation was rejected and produced no value. Check IsSuccess first.");
                }

                return _value;
            }
        }

        /// <summary>
        /// The outcome of an operation that ran.
        /// </summary>
        /// <param name="value">Value produced by the operation.</param>
        /// <returns>A successful result carrying <paramref name="value"/>.</returns>
        public static Result<T> Success(T value) => new Result<T>(true, value, ValidationResult.Ok);

        /// <summary>
        /// The outcome of an operation that was rejected.
        /// </summary>
        /// <param name="validation">The failed validation. Must carry at least one error.</param>
        /// <returns>A rejected result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validation"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="validation"/> reports no error.</exception>
        public static Result<T> Invalid(ValidationResult validation)
        {
            if (validation == null)
            {
                throw new ArgumentNullException(nameof(validation));
            }

            if (validation.IsValid)
            {
                throw new ArgumentException("A rejected result needs at least one error.", nameof(validation));
            }

            return new Result<T>(false, default, validation);
        }

        /// <summary>
        /// The outcome of an operation that was rejected, built from loose errors.
        /// </summary>
        /// <param name="errors">Errors to carry. At least one is required.</param>
        /// <returns>A rejected result.</returns>
        public static Result<T> Invalid(params ValidationError[] errors) =>
            Invalid(ValidationResult.Failed(errors));
    }
}
