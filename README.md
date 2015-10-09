## What is this?

Penumbra allows users to easily add 2D lighting with shadowing effects (for both umbra and penumbra regions) to their games.

![Platformer2D sample](https://jvcontent.blob.core.windows.net/images/screen_09.jpg)

## Getting Started

> Currently available only for MonoGame WindowsDX platform targeting .NET 4.0!

### Basic setup

Install through NuGet:

```powershell
Install-Package Penumbra.MonoGame.WindowsDX -Pre
```

In the game constructor, create the penumbra component and add to components:
```cs
penumbra = new PenumbraComponent(this);
Components.Add(penumbra);
```

In the game's `Draw` method, make sure to call `BeginDraw` before any other drawing takes place:

```cs
penumbra.BeginDraw();

GraphicsDevice.Clear(Color.CornflowerBlue);

...
```

This will swap the render target to a custom texture so that the generated lightmap can be blended atop of it once PenumbraComponent is drawn.
