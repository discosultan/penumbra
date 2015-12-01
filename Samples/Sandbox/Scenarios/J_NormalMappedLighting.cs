using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class J_NormalMappedLighting : Scenario
    {
        private const float RotationSpeed = MathHelper.TwoPi / 6;

        private Light _light;        
        private Texture2D _normal;
        private Texture2D _diffuse;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _normal = content.Load<Texture2D>("199_norm");
            _diffuse = content.Load<Texture2D>("199");
            //normal = content.Load<Texture2D>("metal_normal");
            //diffuse = content.Load<Texture2D>("metal");
            //normal = content.Load<Texture2D>("floor_normal");
            //diffuse = content.Load<Texture2D>("floor");
            //normal = content.Load<Texture2D>("stone_normal");
            //diffuse = content.Load<Texture2D>("stone");

            penumbra.NormalMappedLightingEnabled = true;

            _light = new PointLight
            {
                Position = new Vector2(-300, 0),
                Color = Color.White,
                Scale = new Vector2(2000),
                Radius = 20,
                Height = 200,
                Intensity = 1                
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(Hull.CreateRectangle(scale: new Vector2(50f)));
        }

        public override void Update(float deltaSeconds)
        {
            float angle = deltaSeconds * RotationSpeed;
            var s = (float)Sin(angle);
            var c = (float)Cos(angle);

            _light.Position = new Vector2(
                _light.Position.X * c - _light.Position.Y * s,
                _light.Position.X * s + _light.Position.Y * c
            );
        }

        public override void DrawDiffuse(PrimitiveRenderer renderer)
        {
            //var size = new Vector2(Device.Viewport.Width, Device.Viewport.Height);
            var size = new Vector2(_diffuse.Width, _diffuse.Height);
            renderer.RenderQuad(
                _diffuse,
                -size/2,
                size);
        }

        public override void DrawNormals(PrimitiveRenderer renderer)
        {
            //var size = new Vector2(Device.Viewport.Width, Device.Viewport.Height);
            var size = new Vector2(_normal.Width, _normal.Height);
            renderer.RenderQuad(
                _normal,
                -size / 2,
                size);
        }
    }
}