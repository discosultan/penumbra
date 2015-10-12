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

### Adding lights

Penumbra supports three types of lights: `PointLight`, `Spotlight`, `TexturedLight`

![Different types of light](https://jvcontent.blob.core.windows.net/images/light_types.png)

While `PointLight` and `Spotlight` are generated on the shader, `TexturedLight` allows for more customization by requiring a custom texture used for lighting.

Lights provide two types of shadowing schemes: `ShadowType.Illuminated`, `ShadowType.Solid`

![Different types of shadowing](https://jvcontent.blob.core.windows.net/images/shadow_types.png)

To add a light:

```cs
penumbra.Lights.Add(light);
```

### Adding shadow hulls

Hulls are polygons from which shadows are cast. They are usually created using the same geometry as the scene and can be ordered both clockwise or counter-clockwise. Hull points can be manipulated though the `hull.Points` property.

For a hull to be valid and included in the shadow mask generation, it must conform to the following rules:

- Contain at least 3 points
- Points must form a simple polygon (polygon where no two edges intersect)

Hull validity can be checked through the `hull.Valid` property.

To add a hull:

```cs
penumbra.Hulls.Add(hull);
```
