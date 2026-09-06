using System.Collections.Generic;

namespace VideoGameManager.Views
{
    /// <summary>
    /// Members every view shares. A presenter talks to the user through these and never
    /// touches <c>MessageBox</c> or any other WinForms type.
    /// </summary>
    public interface IView
    {
        /// <summary>Shows or hides a busy indicator while an async call is in flight.</summary>
        bool IsBusy { set; }

        /// <summary>Reports a failure the user can act on. Never the raw exception text.</summary>
        void ShowError(string message);

        /// <summary>Reports a completed action.</summary>
        void ShowInfo(string message);

        /// <summary>Asks the user to confirm a destructive action.</summary>
        bool Confirm(string message);
    }

    /// <summary>
    /// A view that edits fields and can point at the one that failed validation.
    /// <paramref name="field"/> is a domain property name, so the view decides which
    /// control it belongs to.
    /// </summary>
    public interface IValidatingView : IView
    {
        void ShowFieldError(string field, string message);

        void ClearFieldErrors();
    }

    /// <summary>Convenience for presenters reporting a whole validation result at once.</summary>
    public static class ValidatingViewExtensions
    {
        public static void ShowFieldErrors(
            this IValidatingView view,
            IEnumerable<Domain.ValidationError> errors)
        {
            view.ClearFieldErrors();
            foreach (Domain.ValidationError error in errors)
            {
                view.ShowFieldError(error.Field, error.Message);
            }
        }
    }
}
