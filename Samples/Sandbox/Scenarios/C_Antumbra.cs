using System;
using Microsoft.Xna.Framework;
using Penumbra;

namespace Sandbox.Scenarios
{
    class C_Antumbra : Scenario
    {
        private const float MinLightRadius = 10;
        private const float MaxLightRadius = 140;
        private const float RadiusSpeed = 2f;

        private Light _light;
        private bool _isRadiusIncreasing = true;
        private float _currentRadiusProgress;

        public override string Name { get; } = "Antumbra";

        public override void Activate(PenumbraComponent penumbra)
        {
            _light = new Light
            {
                Position = new Vector2(-100, 0),
                Color = Color.White,
                Range = 500,
                Radius = MinLightRadius,
                ShadowType = ShadowType.Illuminated
            };
            penumbra.Lights.Add(_light);
            penumbra.Hulls.Add(new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
            {
                Position = new Vector2(100, 0),
                Scale = new Vector2(50f)
            });
        }

        public override void Update(float deltaSeconds)
        {
            _currentRadiusProgress = Math.Min(_currentRadiusProgress + deltaSeconds / RadiusSpeed, 1f);

            _light.Radius = _isRadiusIncreasing 
                ? MathHelper.Lerp(MinLightRadius, MaxLightRadius, _currentRadiusProgress) 
                : MathHelper.Lerp(MaxLightRadius, MinLightRadius, _currentRadiusProgress);

            if (_currentRadiusProgress >= 1f)
            {
                _currentRadiusProgress = 0;
                _isRadiusIncreasing = !_isRadiusIncreasing;
            }
        }
    }
}
