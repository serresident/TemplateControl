using Avalonia;
using Avalonia.Headless;
using Xunit;

namespace TemplateControl.Tests;

/// <summary>
/// Initialises the Avalonia headless platform once per test session.
/// All test collections that need Avalonia's property system must reference this fixture.
/// </summary>
public sealed class AvaloniaFixture : IDisposable
{
    public AvaloniaFixture()
    {
        AppBuilder.Configure<Application>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .SetupWithoutStarting();
    }

    public void Dispose() { }
}

[CollectionDefinition("Avalonia")]
public sealed class AvaloniaCollection : ICollectionFixture<AvaloniaFixture> { }
