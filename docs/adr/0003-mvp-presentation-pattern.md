# ADR 0003 - Model-View-Presenter for the UI layer

Status: accepted

## Context

All five forms call the database directly from their code-behind and show raw exception
text in a `MessageBox`. None of that is testable. The presentation layer needs a seam.

## Decision

Use Model-View-Presenter: one `I<Name>View` interface and one `<Name>Presenter` per form.
The form implements the view interface; the presenter holds the logic and depends only on
services.

## Rationale

MVP gives a testable seam without adding a framework. The view interface exposes plain
properties and events, so a presenter can be unit-tested against a substitute view with
NSubstitute - which is exactly what the Phase 4 coverage target needs.

MVVM was the alternative. WinForms data binding is weak compared to WPF's, so MVVM here
would mean hand-written `INotifyPropertyChanged` plumbing and binding glue for very little
benefit over an explicit view interface. Passive View also keeps the presenter free of any
WinForms type, which the `layer-guard` hook can then enforce mechanically.

## Consequences

- Presenters must not reference `Form`, `Control` or `MessageBox`; user feedback goes
  through `IView.ShowError` / `ShowInfo` / `Confirm`.
- Form code-behind shrinks to property accessors and event raisers.
- Forms are resolved from the DI container, so a form never constructs another form with
  `new`.
- More files per screen: three instead of one. That is the price of the seam.
