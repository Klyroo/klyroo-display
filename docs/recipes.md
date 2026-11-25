# Recipes & Testing — Klyroo.Display

## Recipe 1: Modal File Preview Provider Setup

```csharp
@* In MainLayout.razor *@
<KlyFilePreviewProvider />

@* In any page *@
@inject FilePreviewService Preview
<KlyButton Label="Preview PDF" OnClick="@(() => Preview.Open(new FilePreviewItem("spec.pdf", "/files/spec.pdf")))" />
```

## Recipe 2: Headless Unit Testing (`xUnit`)

Test all business logic in `Klyroo.Display.Core` without spinning up a browser or Blazor renderer:

```csharp
using Xunit;

public sealed class DisplayUnitTests
{
    [Fact]
    public void Headless_core_runs_without_ui_host()
    {
        // Arrange & Act using Klyroo.Display.Core directly
        // Assert deterministic outputs with zero JSInterop setup
    }
}
```

## Recipe 3: Component Testing (`bUnit`)

```csharp
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class DisplayComponentTests : BunitContext
{
    public DisplayComponentTests()
    {
        Services.AddKlyrooDisplay();
    }
}
```

> **Tip**: Always register Klyroo services on `Services` **before** calling `SetRendererInfo(...)` in bUnit tests.
