using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class J_ConcaveHull2 : Scenario
    {
        private const float HullRotationSpeed = MathHelper.TwoPi / 4f;
        private const float Light1MovementSpeed = MathHelper.TwoPi / 10f;
        private const float Light2MovementSpeed = -MathHelper.TwoPi / 5f;
        private const float Light3MovementSpeed = MathHelper.TwoPi / 6f;
        private static readonly Color[] LightColors =
        {            
            Color.Red, Color.Orange, Color.Yellow
        };

        private Light _light1;
        private Light _light2;
        private Light _light3;
        private Hull _hull;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _light1 = new PointLight
            {
                Position = new Vector2(-250, 0),
                Color = LightColors[0],
                Intensity = 1f,
                Scale = new Vector2(800),
                Radius = 20
            };
            _light2 = new PointLight
            {
                Position = new Vector2(0, 150),
                Color = LightColors[1],
                Intensity = 1.2f,
                Scale = new Vector2(700),
                Radius = 30
            };
            _light3 = new PointLight
            {
                Position = new Vector2(0, -60),
                Color = LightColors[2],
                Scale = new Vector2(700),
                Radius = 25,
                Intensity = 0.8f
            };
            penumbra.Lights.Add(_light1);
            penumbra.Lights.Add(_light2);
            penumbra.Lights.Add(_light3);

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

            _hull = new Hull(hullVertices)
            {
                Position = new Vector2(0, 0),
            };
            penumbra.Hulls.Add(_hull);
        }

        public override void Update(float deltaSeconds)
        {
            float angle = HullRotationSpeed * deltaSeconds;
            _hull.Rotation += angle;

            MoveLight(_light1, Light1MovementSpeed, deltaSeconds);
            MoveLight(_light2, Light2MovementSpeed, deltaSeconds);
            MoveLight(_light3, Light3MovementSpeed, deltaSeconds);
        }

        private static void MoveLight(Light light, float speed, float deltaSeconds)
        {
            float angle = deltaSeconds * speed;
            var s = (float)Sin(angle);
            var c = (float)Cos(angle);

            light.Position = new Vector2(
                light.Position.X * c - light.Position.Y * s,
                light.Position.X * s + light.Position.Y * c
            );
        }
    }
}
