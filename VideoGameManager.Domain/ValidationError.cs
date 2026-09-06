namespace VideoGameManager.Domain
{
    /// <summary>
    /// One broken rule: which field is wrong and why.
    /// </summary>
    /// <param name="Field">
    /// Name of the domain property that failed, for example <c>nameof(Game.Name)</c>. The
    /// presentation layer maps this to the control that shows the message.
    /// </param>
    /// <param name="Message">Message shown to the user.</param>
    public readonly record struct ValidationError(string Field, string Message);
}
