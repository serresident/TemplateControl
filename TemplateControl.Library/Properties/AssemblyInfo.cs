using System.Runtime.CompilerServices;

// Grant the test project access to internal members (TryProposeValue, etc.)
// so we can test business logic without exposing it in the public API.
[assembly: InternalsVisibleTo("TemplateControl.Tests")]
