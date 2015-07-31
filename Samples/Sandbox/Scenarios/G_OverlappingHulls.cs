using Microsoft.Xna.Framework;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class G_OverlappingHulls : Scenario
    {
        private Light _light;
        private const float RotationSpeed = MathHelper.TwoPi / 6;

        public override void Activate(PenumbraComponent penumbra)
        {
            _light = new Light
            {
                Position = new Vector2(-150, 0),
                Color = Color.White,
                Range = 350,
                Radius = 20
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(-20, 0),
                Scale = new Vector2(80f)
            });
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(20f, 40),
                Scale = new Vector2(60f)
            });
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(10, -40),
                Scale = new Vector2(70f)
            });
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
    }
}
