using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class J_NormalMappedLighting : Scenario
    {
        private Light _light;
        private const float RotationSpeed = MathHelper.TwoPi/6;
        private Texture2D normal;
        private Texture2D diffuse;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            normal = content.Load<Texture2D>("face_normals");
            diffuse = content.Load<Texture2D>("face");

            penumbra.NormalMappedLightingEnabled = true;

            _light = new PointLight
            {
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Scale = new Vector2(600),
                Radius = 20,
                Height  = 400
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(Hull.CreateRectangle(scale: new Vector2(50f)));            
        }

        public override void Update(float deltaSeconds)
        {
            float angle = deltaSeconds*RotationSpeed;
            var s = (float) Sin(angle);
            var c = (float) Cos(angle);

            _light.Position = new Vector2(
                _light.Position.X * c - _light.Position.Y * s,
                _light.Position.X * s + _light.Position.Y * c
            );
        }

        public override void DrawDiffuse(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                diffuse,
                new Rectangle(new Point(0, 0), new Point(Device.Viewport.Width, Device.Viewport.Height)),
                Color.White);
        }

        public override void DrawNormals(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                normal,
                new Rectangle(new Point(0, 0), new Point(Device.Viewport.Width, Device.Viewport.Height)),
                Color.White);
        }
    }
}
