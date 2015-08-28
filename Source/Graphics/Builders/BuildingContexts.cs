using System;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    // TODO: prefer ctor init to property init

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

    struct AntumbraSide
    {
        public VertexPosition2Texture Item1;
        public VertexPosition2Texture Item2;
        public VertexPosition2Texture Item3;

        public static AntumbraSide Create(VertexPosition2Texture i1, VertexPosition2Texture i2, VertexPosition2Texture i3)
        {
            return new AntumbraSide { Item1 = i1, Item2 = i2, Item3 = i3 };
        }
    }    

    internal class UmbraIntersectionContext
    {
        public Vector2 UmbraIntersectionPoint;
        public Vector2 UmbraLeftProjectedPoint;
        public Vector2 UmbraRightProjectedPoint;
        public FastList<VertexPosition2Texture> UmbraIntersectionVertices = new FastList<VertexPosition2Texture>();
        public FastList<VertexPosition2Texture> UmbraLeftProjectedVertices = new FastList<VertexPosition2Texture>();
        public FastList<VertexPosition2Texture> UmbraRightProjectedVertices = new FastList<VertexPosition2Texture>();
        //public VertexPosition2Texture UmbraIntersectionVertex;
        //public VertexPosition2Texture UmbraLeftProjectedVertex;
        //public VertexPosition2Texture UmbraRightProjectedVertex;

        public FastList<AntumbraSide> LeftVertices = new FastList<AntumbraSide>();
        public FastList<AntumbraSide> RightVertices = new FastList<AntumbraSide>();
    }

    internal struct PenumbraIntersectionContext
    {
        public VertexPosition2Texture PenumbraIntersectonVertex;
        public VertexPosition2Texture PenumbraProjectedVertex;
        public Side Side;
    }

    internal class HullContext
    {
        //public FastList<FastList<PenumbraIntersectionContext>> PenumbraIntersectionContexts = new FastList<FastList<PenumbraIntersectionContext>>();
        public FastList<UmbraIntersectionContext> UmbraIntersectionContexts = new FastList<UmbraIntersectionContext>();
        public FastList<HullPointContext> PointContexts = new FastList<HullPointContext>();
        public bool IsConvex;

        public void Clear()
        {
            //PenumbraIntersectionContexts.Clear(true);
            UmbraIntersectionContexts.Clear(true);
            PointContexts.Clear(true);
        }   
    }
}
