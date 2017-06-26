# MonoGame.Penumbra

<table>
<tbody>
<tr>
<td><a href="#extensions">Extensions</a></td>
<td><a href="#hull">Hull</a></td>
</tr>
<tr>
<td><a href="#light">Light</a></td>
<td><a href="#penumbracomponent">PenumbraComponent</a></td>
</tr>
<tr>
<td><a href="#pointlight">PointLight</a></td>
<td><a href="#shadowtype">ShadowType</a></td>
</tr>
<tr>
<td><a href="#spotlight">Spotlight</a></td>
<td><a href="#texturedlight">TexturedLight</a></td>
</tr>
</tbody>
</table>


## Extensions

Provides extension methods for various types.

### AddRange\`\`1(listInterface, collection)

Adds the elements of the specified collection to the end of the <a href="#system.collections.generic.ilist\`1">System.Collections.Generic.IList\`1</a>.

#### Type Parameters

- T - Type of collection elements.

| Name | Description |
| ---- | ----------- |
| listInterface | *System.Collections.Generic.IList{\`\`0}*<br>The <a href="#system.collections.generic.ilist\`1">System.Collections.Generic.IList\`1</a> to add elements to. |
| collection | *System.Collections.Generic.IEnumerable{\`\`0}*<br>The collection whose elements should be added to the end of the <a href="#system.collections.generic.ilist\`1">System.Collections.Generic.IList\`1</a>. The collection itself cannot be `null`, but it can contain elements that are `null`, if type T is a reference type. |


## Hull

A hull is an object from which shadows are cast. It is a simple convex or concave polygon impassable by light rays.

### Constructor(points)

Constructs a new instance of <a href="#hull">Hull</a>.

| Name | Description |
| ---- | ----------- |
| points | *Microsoft.Xna.Framework.Vector2[]*<br>Points of the hull polygon. In order for the hull to be valid, the points must form: <br>1. A polygon with at least 3 points.<br>2. A simple polygon (no two edges intersect with each other).<br> |

### Constructor(points)

Constructs a new instance of <a href="#hull">Hull</a>.

| Name | Description |
| ---- | ----------- |
| points | *System.Collections.Generic.IEnumerable{Microsoft.Xna.Framework.Vector2}*<br>Points of the hull polygon. In order for the hull to be valid, the points must form: <br>1. A polygon with at least 3 points.<br>2. A simple polygon (no two edges intersect with each other).<br> |

### CreateRectangle(position, scale, rotation, origin)

Factory method for creating a rectangular <a href="#hull">Hull</a> with points defined so that min vertex is at (0.0, 0.0) and max vertex is at (1.0, 1.0).

| Name | Description |
| ---- | ----------- |
| position | *System.Nullable{Microsoft.Xna.Framework.Vector2}*<br>Optional initial position. Default is (0.0, 0.0). |
| scale | *System.Nullable{Microsoft.Xna.Framework.Vector2}*<br>Optional initial scale. Default is (1.0, 1.0). |
| rotation | *System.Single*<br>Optional initial rotation in radians. Default is 0.0. |
| origin | *System.Nullable{Microsoft.Xna.Framework.Vector2}*<br>Optional initial origin. Default is (0.5, 0.5). |

#### Returns

A rectangular <a href="#hull">Hull</a>.

### Enabled

Gets or sets if the hull is enabled and participates in shadow casting. Shadows are only cast from enabled hulls.

### Origin

Gets or sets the origin ((0, 0) point) of the hull's local space.

### Points

Points of the hull polygon. In order for the hull to be valid, the points must form: 
1. A polygon with at least 3 points.
2. A simple polygon (no two edges intersect with each other).
Points can be defined in either clockwise or counter-clockwise order.

### Position

Gets or sets the position of the hull in world space.

### Rotation

Gets or sets the rotation of the hull in radians.

### Scale

Gets or sets the scale (width and height) along X and Y axes.

### Valid

Gets if the hull forms a valid polygon and participates in shadow casting. See Points property for rules of a valid polygon.


## Light

A light is an object which lights the world and casts shadows from <a href="#hull">Hull</a>s.

#### Remarks

It is an abstract class - one of the three concrete implementations should be used instead: <a href="#pointlight">PointLight</a>, <a href="#spotlight">Spotlight</a>, <a href="#texturedlight">TexturedLight</a>.

### Constructor

Initializes a new instance of the <a href="#light">Light</a> class.

### CastsShadows

Gets or sets if the light casts shadows.

### Color

Gets or sets the color of the light. Color is in non-premultiplied format. Default is white.

### Enabled

Gets or sets if the light is enabled and should be rendered.

### IgnoredHulls

Gets a list of hulls not participating in the light's shadow casting process.

### Intensity

Gets or sets the intensity of the color applied to the final scene. Color will be raised to the power of intensity.

### Origin

Gets or sets the origin (anchor) of the light. It is used for both positioning and rotating. Normalized to the range [0..1].

#### Remarks


Each light is essentially a quad. Origin is the anchor point which marks the (0, 0) point on that quad (in local space). Depending if you are operating in SpriteBatch's screen space (y-axis runs from top to bottom) origin (0, 0) represents the light quad's top left corner while (1, 1) represents the bottom right corner. The reason it's normalized to [0..1] is so that if you change the scale of the light, you wouldn't need to change the origin: an origin (0.5, 0.5) would still mark the center of the light.

When it comes to the setter, there is no automatic normalization being done: it is expected to be set in its normalized form. The reason values outside [0..1] range are allowed is that it might be desirable for some weird rotation scenarios, though such usage should be rather uncommon.

Default value is usually sufficient for <a href="#pointlight">PointLight</a> and <a href="#spotlight">Spotlight</a>.


### Position

Gets or sets the light's position in world space.

### Radius

Gets or sets the radius of the light source (the area where light is emitted). This determines the shape of the umbra and penumbra regions for cast shadows.

#### Remarks

Not to be confused with <a href="#light.scale">Light.Scale</a>, while radius is only used to control the softness of the shadow being cast and should usually be kept at a small value, scale is used to determine how far the light rays reach (range of the light). <a href="#light.scale">Light.Scale</a> for more info.

### Rotation

Gets or sets the rotation of the light in radians.

### Scale

Gets or sets the scale (width and height) along X and Y axes.

#### Remarks

Not to be confused with <a href="#light.radius">Light.Radius</a>, scale determines the attenuation of the light or how far the light rays reach (range of the light), while radius determines the radius of the light source (the area where light is emitted). <a href="#light.radius">Light.Radius</a> for more info.

### ShadowType

Gets or sets how the shadow <a href="#hull">Hull</a>s are shadowed. See <a href="#light.shadowtype">Light.ShadowType</a> for more information.


## PenumbraComponent

GPU based 2D lighting and shadowing engine with penumbra support. Operates with <a href="#light">Light</a>s and shadow <a href="#hull">Hull</a>s, where light is a colored light source which casts shadows on shadow hulls that are outlines of scene geometry (polygons) impassable by light.

#### Remarks

Before rendering scene, ensure to call `PenumbraComponent.BeginDraw` to swap render target in order for the component to be able to later apply generated lightmap.

### Constructor(game)

Constructs a new instance of <a href="#penumbracomponent">PenumbraComponent</a>.

| Name | Description |
| ---- | ----------- |
| game | *Microsoft.Xna.Framework.Game*<br>Game object to associate the engine with. |

### AmbientColor

Gets or sets the ambient color of the scene. Color is in non-premultiplied format.

### BeginDraw

Sets up the lightmap generation sequence. This should be called before Draw.

### Debug

Gets or sets if debug outlines should be drawn for shadows and light sources and if logging is enabled.

### DiffuseMap

Gets the diffuse map render target used by Penumbra.

### Dispose(System.Boolean)

### Draw(gameTime)

Generates the lightmap, blends it with whatever was drawn to the scene between the calls to BeginDraw and this and presents the result to the backbuffer.

| Name | Description |
| ---- | ----------- |
| gameTime | *Microsoft.Xna.Framework.GameTime*<br>Time passed since the last call to Draw. |

### Hulls

Gets the list of shadow hulls registered with the engine.

### Initialize

Explicitly initializes the engine. This should only be called if the component was not added to the game's components list through `Components.Add`.

### LightMap

Gets the lightmap render target used by Penumbra.

### Lights

Gets the list of lights registered with the engine.

### SpriteBatchTransformEnabled

Gets or sets if this component is used with <a href="#microsoft.xna.framework.graphics.spritebatch">Microsoft.Xna.Framework.Graphics.SpriteBatch</a> and its transform should be automatically applied. Default value is `true`.

### Transform

Gets or sets the custom transformation matrix used by the component.

### UnloadContent


## PointLight

A <a href="#light">Light</a> which equally lights the surroundings in all directions.


## ShadowType

Determines how the shadows cast by the light affect shadow <a href="#hull">Hull</a>s.

### Illuminated

Shadow hulls are lit by the light.

### Occluded

Occluded shadow hulls (hulls behind other hulls) are not lit.

### Solid

Shadow hulls are not lit by the light.


## Spotlight

A <a href="#light">Light</a> emitting light only in a single direction (similar to flashlight).

#### Remarks

Default direction is to the right. Use <a href="#light.rotation">Light.Rotation</a> to control in which direction the spotlight is aimed at.

### Constructor

Constructs a new instance of <a href="#spotlight">Spotlight</a>.

### ConeDecay

Gets or sets the rate of cone attenuation to the sides. A higher value means softer edges. Default is 1.5.


## TexturedLight

A <a href="#light">Light</a> which allows its shape to be determined by a custom <a href="#microsoft.xna.framework.graphics.texture2d">Microsoft.Xna.Framework.Graphics.Texture2D</a>.

### Constructor(texture)

Constructs a new instance of <a href="#texturedlight">TexturedLight</a>.

| Name | Description |
| ---- | ----------- |
| texture | *Microsoft.Xna.Framework.Graphics.Texture2D*<br>Texture used to determine light strength at the sampled point. Pass NULL to set texture later. |

### Texture

Gets or sets the texture used to determine in what shape to render the light. A spotlight could be simulated with a spotlight texture. If no texture is set, uses a linear falloff equation to render a point light shaped light.
