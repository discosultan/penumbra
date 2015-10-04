using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;
using static System.Math;

namespace Sandbox.Scenarios
{
    class H_Spotlight : Scenario
    {
        private const int NumHulls = 8;

        private readonly List<Hull> _hulls = new List<Hull>();

        private readonly List<Spotlight> _lights = new List<Spotlight>();
        
        private const float RotationSpeed = MathHelper.TwoPi/8;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _hulls.Clear();
            _lights.Clear();
            _lights.Add(new Spotlight
            {                
                Color = Color.YellowGreen,
                Scale = new Vector2(400),
                Radius = 20,
                //ConeAngle = MathHelper.PiOver2,
                ConeDecay = 1.5f 
            });
            _lights.Add(new Spotlight
            {
                Color = Color.Wheat,
                Range = 500,
                Rotation = MathHelper.Pi - MathHelper.PiOver2 * 0.75f,                                
                ConeDecay = 0.5f
            });
            _lights.Add(new Spotlight
            {
                Color = Color.Turquoise,
                Range = 450,
                Rotation = MathHelper.Pi + MathHelper.PiOver2 * 0.75f,
                ConeDecay = 1f
            });
            _lights.ForEach(penumbra.Lights.Add);            

            GenerateHulls(penumbra);
        }

        private void GenerateHulls(PenumbraComponent penumbra)
        {
            float increment = MathHelper.TwoPi / NumHulls;
            const float distance = 185;
            for (int i = 0; i < NumHulls; i++)
            {
                float angle = increment * i;                
                var hull = new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
                {
                    Position = Rotate(new Vector2(0, distance), angle),
                    Scale = new Vector2(50f)
                };
                _hulls.Add(hull);
                penumbra.Hulls.Add(hull);
            }
        }

        public override void Update(float deltaSeconds)
        {
            float angle = deltaSeconds*RotationSpeed;            

            _lights.ForEach(x =>
            {
                x.Rotation = MathHelper.WrapAngle(x.Rotation - angle);
            });

            _hulls.ForEach(x =>
            {
                x.Position = Rotate(x.Position, angle);
            });
        }

        private Vector2 Rotate(Vector2 vector, float angle)
        {
            var s = (float)Sin(angle);
            var c = (float)Cos(angle);

            return new Vector2(
                vector.X * c - vector.Y * s,
                vector.X * s + vector.Y * c);
        }
    }
}
