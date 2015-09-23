using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;

namespace Sandbox.Scenarios
{
    class E_Antumbra : Scenario
    {
        private const float MinLightRadius = 5;
        private const float MaxLightRadius = 180;
        private const float RadiusSpeed = 2f;

        private Light _light;
        private bool _isRadiusIncreasing;
        private float _progress;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _isRadiusIncreasing = true;
            _progress = 0;

            _light = new PointLight
            {
                //Position = new Vector2(-100, 50),
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Intensity = 2.25f,
                Range = 600,
                Radius = MinLightRadius                
            };
            penumbra.Lights.Add(_light);

            var hullVertices = new[]
            {
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)
            };
            penumbra.Hulls.Add(new Hull(hullVertices)
            {
                Position = new Vector2(100, 0),
                Scale = new Vector2(50f)
            });
            //penumbra.Hulls.Add(new Hull(hullVertices)
            //{
            //    Position = new Vector2(250, 0),
            //    Scale = new Vector2(50f)
            //});
        }

        public override void Update(float deltaSeconds)
        {
            _progress = Math.Min(_progress + deltaSeconds / RadiusSpeed, 1f);

            _light.Radius = _isRadiusIncreasing
                ? MathHelper.Lerp(MinLightRadius, MaxLightRadius, _progress)
                : MathHelper.Lerp(MaxLightRadius, MinLightRadius, _progress);

            if (_progress >= 1f)
            {
                _progress = 0;
                _isRadiusIncreasing = !_isRadiusIncreasing;
            }            
        }
    }
}
