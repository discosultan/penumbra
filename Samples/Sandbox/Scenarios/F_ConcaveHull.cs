using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class F_ConcaveHull : Scenario
    {
        private Light _light;
        private const float RotationSpeed = MathHelper.TwoPi / 6;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _light = new PointLight
            {
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Range = 300,
                Radius = 20
            };
            penumbra.Lights.Add(_light);

            Vector2[] hullVertices =
            {
                new Vector2(0, 50),
                new Vector2(14, 20),
                new Vector2(47, 15),
                new Vector2(23, -7),
                new Vector2(29, -40),
                new Vector2(0, -25),
                new Vector2(-29, -40),
                new Vector2(-24, -7),
                new Vector2(-47, 15),
                new Vector2(-14, 20),
            };

            penumbra.Hulls.Add(new Hull(hullVertices)
            {
                Position = new Vector2(0, 0),
                //Scale = new Vector2(50f)
            });

            //penumbra.Hulls.Add(new Hull(new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(0, 1) })
            //{
            //    Scale = new Vector2(50f)
            //});

            //penumbra.Hulls.Add(new Hull(new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1) })
            //{
            //    Scale = new Vector2(50f)
            //});
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
            //_light.Position = new Vector2(-10, -55);
        }
    }
}
