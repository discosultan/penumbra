using System;
using Microsoft.Xna.Framework;
using Penumbra;

namespace Sandbox.Scenarios
{
    class B_PassThrough : Scenario
    {
        private const float HullRotationSpeed = MathHelper.TwoPi/2f;
        private const float LightSpeed = 4f;
        private const float Padding = 50f;

        private PenumbraComponent _penumbra;
        public override string Name { get; } = "Pass through";

        private Light _light1;
        private Light _light2;
        private bool _isMovingLeft;
        private float _progress;

        public override void Activate(PenumbraComponent penumbra)
        {
            _penumbra = penumbra;
            var vertices = new[]
            {
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)
            };

            _light1 = new Light
            {
                Color = Color.OrangeRed,
                Range = 300,
                Radius = 20,
                ShadowType = ShadowType.Illuminated
            };
            _light2 = new Light
            {
                Color = Color.LightBlue,
                Range = 300,
                Radius = 20,
                ShadowType = ShadowType.Illuminated
            };
            _penumbra.Lights.Add(_light1);
            _penumbra.Lights.Add(_light2);

            _penumbra.Hulls.Add(new Hull(vertices) {Position = new Vector2(-200, 0), Scale = new Vector2(50f)});
            _penumbra.Hulls.Add(new Hull(vertices) { Position = new Vector2(0, 0), Scale = new Vector2(50f) });
            _penumbra.Hulls.Add(new Hull(vertices) { Position = new Vector2(200, 0), Scale = new Vector2(50f) });
        }

        public override void Update(float deltaSeconds)
        {
            float angle = deltaSeconds * HullRotationSpeed;

            for (int i = 0; i < _penumbra.Hulls.Count; i++)
            {
                Hull hull = _penumbra.Hulls[i];                
                if (i % 2 == 0)                
                    hull.Rotation += angle;
                else
                    hull.Rotation -= angle;
            }

            _progress = Math.Min(_progress + deltaSeconds / LightSpeed, 1f);

            
            float halfWidth = _penumbra.GraphicsDevice.Viewport.Width / 2f;
                        
            float posX1 = MathHelper.SmoothStep(halfWidth - Padding, -halfWidth + Padding, _progress);                            
            float posX2 = MathHelper.SmoothStep(-halfWidth + Padding, halfWidth - Padding, _progress);                
            
            _light1.Position = new Vector2(_isMovingLeft ? posX1 : posX2, 0);
            _light2.Position = new Vector2(_isMovingLeft ? posX2 : posX1, 0);

            if (_progress >= 1f)
            {
                _progress = 0;
                _isMovingLeft = !_isMovingLeft;
            }            
        }
    }
}
