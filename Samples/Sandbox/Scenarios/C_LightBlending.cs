using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class C_LightBlending : Scenario
    {
        private const float HullRotationSpeed = MathHelper.TwoPi / 2f;
        private const float Light1MovementSpeed = MathHelper.TwoPi / 7f;
        private const float Light2MovementSpeed = -MathHelper.TwoPi / 3f;
        private const float Light3MovementSpeed = MathHelper.TwoPi / 5f;

        private static readonly Color[] LightColors =
        {
            //Color.LightBlue, Color.Violet, Color.LimeGreen,
            Color.Red, Color.Orange, Color.Yellow
        };

        private Light _light1;
        private Light _light2;
        private Light _light3;
        private Hull _hull;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _hull = new Hull(new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f))
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(50f)
            };
            penumbra.Hulls.Add(_hull);

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
                Scale = new Vector2(400),
                Radius = 30
            };
            _light3 = new PointLight
            {
                Position = new Vector2(0, -50),
                Color = LightColors[2],
                Scale = new Vector2(600),
                Radius = 25,
                Intensity = 0.8f
            };
            penumbra.Lights.Add(_light1);
            penumbra.Lights.Add(_light2);
            penumbra.Lights.Add(_light3);
        }

        public override void Update(float deltaSeconds)
        {
            MoveLight(_light1, Light1MovementSpeed, deltaSeconds);
            MoveLight(_light2, Light2MovementSpeed, deltaSeconds);
            MoveLight(_light3, Light3MovementSpeed, deltaSeconds);

            float angle = HullRotationSpeed * deltaSeconds;
            _hull.Rotation += angle;
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
