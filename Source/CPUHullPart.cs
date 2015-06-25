using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra
{
    internal class CPUHullPart
    {
        private bool _transformedNormalsDirty = true;

        private ApexNormals[] _transformedNormals;

        public CPUHullPart(HullPart hull)
        {
            Inner = hull;
            hull.Component.SetDirty += (s, e) => { _transformedNormalsDirty = true; };            

            Vector2[] points = Inner.Points;

            OriginalNormals = new ApexNormals[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPos = points[i];
                Vector2 prevPos = points.GetPreviousFrom(i);
                Vector2 nextPos = points.GetNextFrom(i);

                Vector2 n1 = VectorUtil.Rotate90CCW(currentPos - prevPos);
                Vector2 n2 = VectorUtil.Rotate90CCW(nextPos - currentPos);

                //// Ref: http://stackoverflow.com/a/25646126/1466456
                //Vector2 currentToPrev = prevPos - currentPos;
                //Vector2 currentToNext = nextPos - currentPos;                
                //float angle = Calc.Atan2(currentToNext.Y, currentToNext.X) - Calc.Atan2(currentToPrev.Y, currentToPrev.X);
                //bool isConvex = angle < MathUtil.Pi;

                OriginalNormals[i] = new ApexNormals(n1, n2);
            }
        }        

        public HullPart Inner { get; }

        // TODO: reuse similar vertex and index arrays        
        public int[] Indices => Inner.Indices;

        public ApexNormals[] OriginalNormals { get; }
        
        public ApexNormals[] TransformedNormals
        {
            get
            {
                if (_transformedNormalsDirty)
                {
                    if (_transformedNormals == null)
                        _transformedNormals = new ApexNormals[OriginalNormals.Length];

                    Matrix normalMatrix = Matrix.Identity;

                    float cos = Calc.Cos(Inner.Component.Rotation);
                    float sin = Calc.Sin(Inner.Component.Rotation);

                    // normalMatrix = scaleInv * rotation;
                    normalMatrix.M11 = (1f / Inner.Component.Scale.X) * cos;
                    normalMatrix.M12 = (1f / Inner.Component.Scale.X) * sin;
                    normalMatrix.M21 = (1f / Inner.Component.Scale.Y) * -sin;
                    normalMatrix.M22 = (1f / Inner.Component.Scale.Y) * cos;

                    for (var i = 0; i < OriginalNormals.Length; i++)
                    {
                        _transformedNormals[i] = ApexNormals.Transform(OriginalNormals[i], ref normalMatrix);
                    }

                    _transformedNormalsDirty = false;
                }
                return _transformedNormals;
            }
        }

        public static implicit operator HullPart(CPUHullPart hull)
        {
            return hull.Inner;
        }
    }
}
