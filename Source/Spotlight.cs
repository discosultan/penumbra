using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Renderers;
using Penumbra.Utilities;

namespace Penumbra
{
    public sealed class Spotlight : Light
    {
        private float _range = 100.0f;
        /// <summary>
        /// Gets or sets how far from the position the light reaches (falls off).
        /// </summary>
        public float Range
        {
            get { return _range; }
            set
            {
                if (_range != value)
                {
                    _range = value;
                    _worldDirty = true;
                }
            }
        }

        private Vector2 _coneDirection = Vector2.UnitY;

        public Vector2 ConeDirection
        {
            get { return _coneDirection; }
            set
            {
                if (value == Vector2.Zero)
                    value = Vector2.UnitY;                
                value.Normalize();            
                _coneDirection = value;
                _worldDirty = true;
            }
        }
        public float ConeAngle { get; set; } = MathHelper.PiOver2;
        public float ConeDecay { get; set; } = 0.5f;

        internal override void CalculateLocalToWorld(out Matrix transform)
        {
            transform = Matrix.Identity;
            // Scaling.
            transform.M11 = Range;
            transform.M22 = Range;
            // Translation.
            transform.M41 = Position.X;
            transform.M42 = Position.Y;
        }

        internal override void CalculateBounds(out BoundingRectangle bounds)
        {
            var rangeVector = new Vector2(Range);
            Vector2 min = Position - rangeVector;
            Vector2 max = Position + rangeVector;
            bounds = new BoundingRectangle(min, max);
        }

        //internal override void CalculateBounds(out BoundingRectangle bounds)
        //{
        //    float halfAngle = ConeAngle*0.5f;

        //    //Vector2 pos;
        //    //Vector2.Multiply(ref _coneDirection, Range, out pos);
        //    //Vector2.Add(ref _position, ref pos, out pos);

        //    var magnitude = (float)(Range/Math.Cos(halfAngle));

        //    Vector2 xDir;
        //    VectorUtil.Rotate(ref _coneDirection, halfAngle, out xDir);

        //    Vector2 x = Position + xDir*magnitude;

        //    Vector2 yDir;
        //    VectorUtil.Rotate(ref _coneDirection, -halfAngle, out yDir);

        //    Vector2 y = Position + yDir*magnitude;

        //    Vector2 min = Vector2.Min(Position, Vector2.Min(x, y));
        //    Vector2 max = Vector2.Max(Position, Vector2.Max(x, y));

        //    bounds = new BoundingRectangle(min, max);
        //}

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            renderer._fxLightParamConeAngle.SetValue(ConeAngle);
            renderer._fxLightParamConeDecay.SetValue(ConeDecay);
            renderer._fxLightParamConeDirection.SetValue(ConeDirection);

            return renderer._fxSpotLightTech;
        }
    }
}
