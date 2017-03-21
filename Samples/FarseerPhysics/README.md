# Penumbra Physics: Penumbra + Farseer Physics!
This sample project shows how you could create a complex body from a sprite and add it to the Penumbra Component as a Hull.


![PenumbraPhysics](https://github.com/sqrMin1/penumbra/blob/master/Documentation/PenumbraPhysics.png)

### Building

The following is required to successfully compile the solution:

- MonoGame 3.6
- FarseerPhysics 3.5 (Source included)

### How To

To create a physics object (Body) out of a sprite you need to add the following method:

```cs
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Factories;

public Body CreateComplexBody(World world, Texture2D objectTexture, float Scale,
             TriangulationAlgorithm Algo = TriangulationAlgorithm.Bayazit)
{
    Body body = null;

    uint[] data = new uint[objectTexture.Width * objectTexture.Height];
    objectTexture.GetData(data);
    
    Vertices textureVertices = PolygonTools.CreatePolygon(data, objectTexture.Width, false);
    Vector2 centroid = -textureVertices.GetCentroid();
    textureVertices.Translate(ref centroid);    
        
    tBodyOrigin = -centroid; // Catch the real origin vector so you can draw the texture at the right position!
    
    textureVertices = SimplifyTools.DouglasPeuckerSimplify(textureVertices, 4f);    
    List<Vertices> list = Triangulate.ConvexPartition(textureVertices, Algo);
    
    Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * Scale;
    
    foreach (Vertices vertices in list)
    {
        vertices.Scale(ref vertScale);
    }

    return body = BodyFactory.CreateCompoundPolygon(world, list, 1f);
}
```

Then you need to create the Shadow Hulls for penumbra like this:

> Let's assume you have a Body called "tBody"

```cs
using FarseerPhysics.Collision.Shapes;

// 64 Pixel of your screen should be 1 Meter in our physical world
float MeterInPixels = 64f;

foreach (Fixture f in tBody.FixtureList)
{
    // Creating the Hull out of the Shape (respectively Vertices) of the fixtures of the physics body
    Hull h = new Hull(((PolygonShape)f.Shape).Vertices);

    // We need to scale the Hull according to our "MetersInPixels-Simulation-Value"
    h.Scale = new Vector2(MeterInPixels);

    // A Hull of Penumbra is set in Display space but the physics body is set in Simulation space
    // Thats why we need to convert the simulation units of the physics object to the display units
    // of the Hull object
    h.Position = ConvertUnits.ToDisplayUnits(tBody.Position);

    // We are adding the new Hull to our physics body hull list
    // This is necessary to update the Hulls in the Update method (see below)
    tBodyHull.Add(h);

    // Adding the Hull to Penumbra
    penumbra.Hulls.Add(h);
}
```

Now we update our physical Hulls like this:

```cs
// The rotation and the position of all Hulls will be updated according to the physics body rotation and position
foreach (Hull h in tBodyHull)
{
    h.Rotation = tBody.Rotation;
    h.Position = ConvertUnits.ToDisplayUnits(tBody.Position);
}
```

Finally we can draw our physics object:

```cs
    penumbra.BeginDraw();

    GraphicsDevice.Clear(Color.White);

    // Draw items affected by lighting here ...

    spriteBatch.Begin();

    // Draw the texture of the physics body
    spriteBatch.Draw(tBodyTexture, ConvertUnits.ToDisplayUnits(tBody.Position), null,
              Color.Tomato, tBody.Rotation, tBodyOrigin, 1f, SpriteEffects.None, 0);

    spriteBatch.End();
    
    penumbra.Draw(gameTime);
```

That's all! From know on you can create physical objects and hulls for Penumbra out of sprites!

**Have fun!**
