using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Penumbra;

namespace Sandbox.Scenarios
{
    class B_LightPassThrough : Scenario
    {
        //private const float HullRotationSpeed = MathHelper.TwoPi/2f;
        private const float LightSpeed = 2.5f;
        private const float LightPaddingFromEdge = 120f;
        private const float HullSpacing = 160f;

        private PenumbraComponent _penumbra;        

        private Light _light1;
        //private Light _light2;
        private bool _isMovingLeft;
        private float _progress;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _isMovingLeft = false;
            _progress = 0;

            _penumbra = penumbra;
            var vertices = new[]
            {
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)
            };

            _light1 = new PointLight
            {
                //Color = Color.OrangeRed,
                Color = Color.White,
                Scale = new Vector2(700),
                //Radius = 20,
                Radius = 40,
                ShadowType = ShadowType.Illuminated
            };
            //_light2 = new PointLight
            //{
            //    Color = Color.LightBlue,
            //    Range = 300,
            //    Radius = 20,
            //    ShadowType = ShadowType.Illuminated
            //};
            _penumbra.Lights.Add(_light1);
            //_penumbra.Lights.Add(_light2);            

            _penumbra.Hulls.Add(new Hull(vertices) { Position = new Vector2(-HullSpacing, 0), Scale = new Vector2(50f) });
            _penumbra.Hulls.Add(new Hull(vertices) { Position = new Vector2(0, 0), Scale = new Vector2(50f) });
            _penumbra.Hulls.Add(new Hull(vertices) { Position = new Vector2(HullSpacing, 0), Scale = new Vector2(50f) });
        }

        public override void Update(float deltaSeconds)
        {
            //float angle = deltaSeconds * HullRotationSpeed;
            //for (int i = 0; i < _penumbra.Hulls.Count; i++)
            //{
            //    Hull hull = _penumbra.Hulls[i];                
            //    if (i % 2 == 0)                
            //        hull.Rotation += angle;
            //    else
            //        hull.Rotation -= angle;
            //}

            _progress = Math.Min(_progress + deltaSeconds / LightSpeed, 1f);
            
            float halfWidth = _penumbra.GraphicsDevice.Viewport.Width / 2f;
                        
            float posX1 = MathHelper.SmoothStep(halfWidth - LightPaddingFromEdge, -halfWidth + LightPaddingFromEdge, _progress);                            
            float posX2 = MathHelper.SmoothStep(-halfWidth + LightPaddingFromEdge, halfWidth - LightPaddingFromEdge, _progress);                
            
            _light1.Position = new Vector2(_isMovingLeft ? posX1 : posX2, 0);
            //_light2.Position = new Vector2(_isMovingLeft ? posX2 : posX1, 0);

            if (_progress >= 1f)
            {
                _progress = 0;
                _isMovingLeft = !_isMovingLeft;
            }            
        }
    }
}
