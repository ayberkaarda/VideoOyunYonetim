using System;
using System.Collections.Generic;

namespace VideoGameManager.Domain
{
    /// <summary>
    /// Outcome of a validation run: either no errors, or the complete list of them.
    /// </summary>
    /// <remarks>
    /// Validation collects every broken rule instead of stopping at the first one, so a form
    /// can mark all of its wrong fields in a single pass.
    /// </remarks>
    public sealed class ValidationResult
    {
        private static readonly ValidationError[] NoErrors = new ValidationError[0];
        private static readonly ValidationResult Valid = new ValidationResult(NoErrors);

        /// <summary>
        /// Creates a result from a set of errors. An empty set means the value is valid.
        /// </summary>
        /// <param name="errors">Errors to carry.</param>
        /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <c>null</c>.</exception>
        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            Errors = new List<ValidationError>(errors).AsReadOnly();
        }

        /// <summary>
        /// The shared result that carries no errors.
        /// </summary>
        public static ValidationResult Ok => Valid;

        /// <summary>
        /// <c>true</c> when nothing is wrong.
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        /// <summary>
        /// Every broken rule, in the order the validator found them.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Creates a failed result from one or more errors.
        /// </summary>
        /// <param name="errors">Errors to carry. At least one is required.</param>
        /// <returns>A result whose <see cref="IsValid"/> is <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
        public static ValidationResult Failed(params ValidationError[] errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (errors.Length == 0)
            {
                throw new ArgumentException("A failed validation result needs at least one error.", nameof(errors));
            }

            return new ValidationResult(errors);
        }
    }
}
