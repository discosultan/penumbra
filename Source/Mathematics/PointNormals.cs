using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics
{
    internal struct PointNormals
    {        
        public Vector2 Normal1;
        public Vector2 Normal2;
        public bool IsConvex;

        public PointNormals(ref Vector2 normal1, ref Vector2 normal2, bool isConvex)
        {
            Normal1 = normal1;
            Normal2 = normal2;
            IsConvex = isConvex;
        }

        public override string ToString()
        {
            return $"Normal1:{Normal1} Normal2:{Normal2} IsConvex:{IsConvex}";
        }

        public static void Transform(ref PointNormals original, ref Matrix transform, out PointNormals result)
        {
            Vector2 transformedN1;
            Vector2 transformedN2;
            Vector2.TransformNormal(ref original.Normal1, ref transform, out transformedN1);
            Vector2.TransformNormal(ref original.Normal2, ref transform, out transformedN2);
            result = new PointNormals(ref transformedN1, ref transformedN2, original.IsConvex);
        }
    }
}
