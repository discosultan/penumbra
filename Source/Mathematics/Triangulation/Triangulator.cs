// Ref: Nick Gravelyn's Triangulator port from XNA. http://triangulator.codeplex.com/. Modified to avoid allocations.
// NOT thread safe.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vertices = Penumbra.Mathematics.Polygon;
using Indices = System.Collections.Generic.List<int>;

namespace Penumbra.Mathematics.Triangulation
{
    /// <summary>
    ///     A static class exposing methods for triangulating 2D polygons. This is the sole public
    ///     class in the entire library; all other classes/structures are intended as internal-only
    ///     objects used only to assist in triangulation.
    ///     This class makes use of the DEBUG conditional and produces quite verbose output when built
    ///     in Debug mode. This is quite useful for debugging purposes, but can slow the process down
    ///     quite a bit. For optimal performance, build the library in Release mode.
    ///     The triangulation is also not optimized for garbage sensitive processing. The point of the
    ///     library is a robust, yet simple, system for triangulating 2D shapes. It is intended to be
    ///     used as part of your content pipeline or at load-time. It is not something you want to be
    ///     using each and every frame unless you really don't care about garbage.
    /// </summary>
    internal static class Triangulator
    {
        #region Fields

        private static readonly IndexableCyclicalLinkedList<Vertex> PolygonVertices =
            new IndexableCyclicalLinkedList<Vertex>();

        private static readonly IndexableCyclicalLinkedList<Vertex> EarVertices =
            new IndexableCyclicalLinkedList<Vertex>();

        private static readonly CyclicalList<Vertex> ConvexVertices = new CyclicalList<Vertex>();
        private static readonly CyclicalList<Vertex> ReflexVertices = new CyclicalList<Vertex>();

        private static readonly List<Triangle> Triangles = new List<Triangle>();

        #endregion

        #region Public Methods

        #region Triangulate

        /// <summary>
        ///     Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
        /// </summary>
        public static void Triangulate(
            Vertices vertices,
            Indices indices,
            WindingOrder indicesWindingOrder)
        {
            vertices.EnsureWindingOrder(WindingOrder.CounterClockwise);
            TriangulateInner(indicesWindingOrder, vertices, indices);
        }

        //output vertices will be in counter clockwise order in this stage
        private static void TriangulateInner(
            WindingOrder outputIndicesWindingOrder,
            Vertices outputVertices,
            Indices outputIndices)
        {            
            Triangles.Clear();

            //clear all of the lists
            PolygonVertices.Clear();
            EarVertices.Clear();
            ConvexVertices.Clear();
            ReflexVertices.Clear();

            //generate the cyclical list of vertices in the polygon
            for (var i = 0; i < outputVertices.Count; i++)
                PolygonVertices.AddLast(new Vertex(outputVertices[i], i));

            //categorize all of the vertices as convex, reflex, and ear
            FindConvexAndReflexVertices();
            FindEarVertices();

            //clip all the ear vertices
            while (PolygonVertices.Count > 3 && EarVertices.Count > 0)
                ClipNextEar(Triangles);

            //if there are still three points, use that for the last triangle
            if (PolygonVertices.Count == 3)
                Triangles.Add(new Triangle(
                    PolygonVertices[0].Value,
                    PolygonVertices[1].Value,
                    PolygonVertices[2].Value));

            //add all of the triangle indices to the output array
            outputIndices.Clear();

            //move the if statement out of the loop to prevent all the
            //redundant comparisons
            if (outputIndicesWindingOrder == WindingOrder.CounterClockwise)
            {
                for (var i = 0; i < Triangles.Count; i++)
                {
                    outputIndices.Add(Triangles[i].A.Index);
                    outputIndices.Add(Triangles[i].B.Index);
                    outputIndices.Add(Triangles[i].C.Index);
                }
            }
            else
            {
                for (var i = 0; i < Triangles.Count; i++)
                {
                    outputIndices.Add(Triangles[i].C.Index);
                    outputIndices.Add(Triangles[i].B.Index);
                    outputIndices.Add(Triangles[i].A.Index);
                }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region ClipNextEar

        private static void ClipNextEar(ICollection<Triangle> triangles)
        {
            //find the triangle
            var ear = EarVertices[0].Value;
            var prev = PolygonVertices[PolygonVertices.IndexOf(ear) - 1].Value;
            var next = PolygonVertices[PolygonVertices.IndexOf(ear) + 1].Value;
            triangles.Add(new Triangle(ear, next, prev));

            //remove the ear from the shape
            EarVertices.RemoveAt(0);
            PolygonVertices.RemoveAt(PolygonVertices.IndexOf(ear));


            //validate the neighboring vertices
            ValidateAdjacentVertex(prev);
            ValidateAdjacentVertex(next);
        }

        #endregion

        #region ValidateAdjacentVertex

        private static void ValidateAdjacentVertex(Vertex vertex)
        {
            if (ReflexVertices.Contains(vertex))
            {
                if (IsConvex(vertex))
                {
                    ReflexVertices.Remove(vertex);
                    ConvexVertices.Add(vertex);
                }
            }

            if (ConvexVertices.Contains(vertex))
            {
                var wasEar = EarVertices.Contains(vertex);
                var isEar = IsEar(vertex);

                if (wasEar && !isEar)
                {
                    EarVertices.Remove(vertex);
                }
                else if (!wasEar && isEar)
                {
                    EarVertices.AddFirst(vertex);
                }
            }
        }

        #endregion

        #region FindConvexAndReflexVertices

        private static void FindConvexAndReflexVertices()
        {
            for (var i = 0; i < PolygonVertices.Count; i++)
            {
                var v = PolygonVertices[i].Value;

                if (IsConvex(v))
                {
                    ConvexVertices.Add(v);
                }
                else
                {
                    ReflexVertices.Add(v);
                }
            }
        }

        #endregion

        #region FindEarVertices

        private static void FindEarVertices()
        {
            for (var i = 0; i < ConvexVertices.Count; i++)
            {
                var c = ConvexVertices[i];

                if (IsEar(c))
                {
                    EarVertices.AddLast(c);
                }
            }
        }

        #endregion

        #region IsEar

        private static bool IsEar(Vertex c)
        {
            var p = PolygonVertices[PolygonVertices.IndexOf(c) - 1].Value;
            var n = PolygonVertices[PolygonVertices.IndexOf(c) + 1].Value;

            foreach (var t in ReflexVertices)
            {
                if (t.Equals(p) || t.Equals(c) || t.Equals(n))
                    continue;

                if (Triangle.ContainsPoint(p, c, n, t))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IsConvex

        private static bool IsConvex(Vertex c)
        {
            var p = PolygonVertices[PolygonVertices.IndexOf(c) - 1].Value;
            var n = PolygonVertices[PolygonVertices.IndexOf(c) + 1].Value;

            var d1 = Vector2.Normalize(c.Position - p.Position);
            var d2 = Vector2.Normalize(n.Position - c.Position);
            var n2 = new Vector2(-d2.Y, d2.X);

            return (Vector2.Dot(d1, n2) <= 0f);
        }

        #endregion

        #endregion
    }
}