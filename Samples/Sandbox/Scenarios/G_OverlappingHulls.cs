using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class G_OverlappingHulls : Scenario
    {
        private Light _light;
        private const float RotationSpeed = MathHelper.TwoPi / 6;

        private Hull _hull1, _hull2, _hull3;

        private static readonly Vector2 HullPos1 = new Vector2(-20, 0);
        private static readonly Vector2 HullPos2 = new Vector2(20, 50);
        private static readonly Vector2 HullPos3 = new Vector2(10, -40);

        private const float ShakingMagnitude = 2;
        private readonly Random _random = new Random();

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _light = new PointLight
            {
                Position = new Vector2(-150, 0),
                Color = Color.White,
                Scale = new Vector2(700),
                Radius = 20
            };
            penumbra.Lights.Add(_light);
            _hull1 = new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = HullPos1,
                Scale = new Vector2(90f)
            };
            _hull2 = new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = HullPos2,
                Scale = new Vector2(70f)
            };
            _hull3 = new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = HullPos3,
                Scale = new Vector2(80f)
            };
            penumbra.Hulls.Add(_hull1);
            penumbra.Hulls.Add(_hull2);
            penumbra.Hulls.Add(_hull3);
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

            _hull1.Position = HullPos1 + GetRandomOffset() * ShakingMagnitude;
            _hull2.Position = HullPos2 + GetRandomOffset() * ShakingMagnitude;
            _hull3.Position = HullPos3 + GetRandomOffset() * ShakingMagnitude;
        }

        private Vector2 GetRandomOffset()
        {
            return new Vector2(
                (float)((_random.NextDouble() * 2) - 1),
                (float)((_random.NextDouble() * 2) - 1));
        }
    }
}
