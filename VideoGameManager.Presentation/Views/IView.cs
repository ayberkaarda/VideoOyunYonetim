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
    /// </summary>
    public interface IValidatingView : IView
    {
        /// <summary>Marks one field as rejected, next to the control the user typed into.</summary>
        /// <param name="field">
        /// A domain property name, not a control name. The domain reports which property is
        /// wrong; the view is the only place that knows which control shows it.
        /// </param>
        /// <param name="message">What is wrong with the value, phrased for the user.</param>
        void ShowFieldError(string field, string message);

        /// <summary>
        /// Removes every field marker. Called before a fresh validation so markers from an
        /// earlier attempt cannot survive next to a field that is now correct.
        /// </summary>
        void ClearFieldErrors();
    }

    /// <summary>Convenience for presenters reporting a whole validation result at once.</summary>
    public static class ValidatingViewExtensions
    {
        /// <summary>
        /// Clears the previous markers and shows the given ones, which is what a presenter
        /// wants after every validation and is easy to get wrong one call at a time.
        /// </summary>
        /// <param name="view">The screen to mark up.</param>
        /// <param name="errors">Everything the domain rejected. An empty sequence just clears.</param>
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
