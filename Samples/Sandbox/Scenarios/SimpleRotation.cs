using Microsoft.Xna.Framework;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class SimpleRotation : Scenario
    {
        private readonly Light _light;
        private const float RotationSpeed = MathHelper.TwoPi/6;

        public SimpleRotation(PenumbraComponent penumbra)
        {
            _light = new Light
            {
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Range = 300,
                Radius = 20,
                ShadowType = ShadowType.Illuminated
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(50f)
            });
        }

        public override string Name { get; } = "Simple rotation";

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
    }
}
