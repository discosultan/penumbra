using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal struct HullPointContext
    {        
        public Vector2 Point;
        public Vector2 LightToPointDir;
        public Side Side;
        public float Dot1;
        public float Dot2;

        public Vector2 LightRight;
        public Vector2 LightRightToPointDir;
        public Side RightSide;
        public float RightDot1;
        public float RightDot2;

        public Vector2 LightLeft;
        public Vector2 LightLeftToPointDir;
        public Side LeftSide;
        public float LeftDot1;
        public float LeftDot2;

        public PointNormals Normals;
        public int Index;
        public bool IsConvex => Normals.IsConvex;

        public override string ToString()
        {
            return Point.ToString();
        }
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
        public FastList<HullPointContext> PointContexts;
        public bool IsConvex;
    }

    internal enum IntersectionType
    {
        None,
        IntersectsOutsideLight,
        IntersectsInsideLight
    }
}
