using Microsoft.Xna.Framework;
using Penumbra.Mathematics;

namespace Penumbra.Graphics.Builders
{
    internal struct HullPointContext
    {        
        public Vector2 Position;
        public Vector2 LightToPointDir;
        public ApexNormals Normals;
        public int Index;
        public float Dot1;
        public float Dot2;
        //public bool IsInAnotherHull;

        //public bool Concave
        //{
        //    get { return !Normals.Convex; }
        //}

        //public bool Convex
        //{
        //    get { return Normals.Convex; }
        //}

        //public static void Copy(PointProcessingContext from, PointProcessingContext to)
        //{
        //    to.Index = from.Index;
        //    to.Position = from.Position;
        //    to.LightToPointDir = from.LightToPointDir;
        //    to.Normals = from.Normals;
        //    to.Dot1 = from.Dot1;
        //    to.Dot2 = from.Dot2;
        //}
    }

    internal struct HullContext
    {
        public IntersectionType UmbraIntersectionType;        
        public Vector2 UmbraIntersectionPoint;
        public Vector2 UmbraLeftProjectedPoint;
        public Vector2 UmbraRightProjectedPoint;
        public VertexPosition2Texture UmbraIntersectionVertex;
        public VertexPosition2Texture UmbraLeftProjectedVertex;
        public VertexPosition2Texture UmbraRightProjectedVertex;
    }

    internal enum IntersectionType
    {
        None,
        IntersectsOutsideLight,
        IntersectsInsideLight
    }
}
