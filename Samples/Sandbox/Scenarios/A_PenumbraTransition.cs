using Microsoft.Xna.Framework;
using Penumbra;
using Penumbra.Core;
using static System.Math;

namespace Sandbox.Scenarios
{
    class A_PenumbraTransition : Scenario
    {
        private Light _light;
        private const float RotationSpeed = MathHelper.TwoPi/6;

        public override void Activate(PenumbraComponent penumbra)
        {
            _light = new Light
            {
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Range = 300,
                Radius = 20                
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(50f)
            });
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
    }
}
