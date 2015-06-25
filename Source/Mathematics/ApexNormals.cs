using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics
{
    internal struct ApexNormals
    {        
        public Vector2 Normal1;
        public Vector2 Normal2;        

        public ApexNormals(Vector2 normal1, Vector2 normal2)
        {
            Normal1 = normal1;
            Normal2 = normal2;            
        }

        public override string ToString()
        {
            return $"Normal1: {Normal1} Normal2 {Normal2}";
        }

        public static ApexNormals Transform(ApexNormals original, ref Matrix transform)
        {
            Vector2 transformedN1;
            Vector2 transformedN2;
            Vector2.TransformNormal(ref original.Normal1, ref transform, out transformedN1);
            Vector2.TransformNormal(ref original.Normal2, ref transform, out transformedN2);
            return new ApexNormals(transformedN1, transformedN2);
        }
    }
}
