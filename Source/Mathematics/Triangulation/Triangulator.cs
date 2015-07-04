using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

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
    public static class Triangulator
    {
        #region Fields

        private static readonly IndexableCyclicalLinkedList<Vertex> PolygonVertices =
            new IndexableCyclicalLinkedList<Vertex>();

        private static readonly IndexableCyclicalLinkedList<Vertex> EarVertices =
            new IndexableCyclicalLinkedList<Vertex>();

        private static readonly CyclicalList<Vertex> ConvexVertices = new CyclicalList<Vertex>();
        private static readonly CyclicalList<Vertex> ReflexVertices = new CyclicalList<Vertex>();

        #endregion

        #region Public Methods

        #region Triangulate

        /// <summary>
        ///     Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
        /// </summary>
        /// <param name="inputVertices">The polygon vertices in counter-clockwise winding order.</param>
        /// <param name="outputIndicesWindingOrder">The desired output winding order for indices.</param>
        /// <param name="outputVertices">The resulting vertices that include any reversals of winding order and holes.</param>
        /// <param name="outputIndices">The resulting indices for rendering the shape as a triangle list.</param>
        public static void Triangulate(
            Vector2[] inputVertices,
            WindingOrder outputIndicesWindingOrder,
            out Vector2[] outputVertices,
            out int[] outputIndices)
        {
            Triangulate(
                inputVertices,
                DetermineWindingOrder(inputVertices),
                WindingOrder.CounterClockwise,
                outputIndicesWindingOrder,
                out outputVertices,
                out outputIndices);
        }

        /// <summary>
        ///     Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
        /// </summary>
        /// <param name="inputVertices">The polygon vertices in counter-clockwise winding order.</param>
        /// <param name="inputVerticesWindingOrder">The input winding order.</param>
        /// <param name="outputVerticesWindingOrder">The desired output winding order for vertices.</param>
        /// <param name="outputIndicesWindingOrder">The desired output winding order for indices.</param>
        /// <param name="outputVertices">The resulting vertices that include any reversals of winding order and holes.</param>
        /// <param name="outputIndices">The resulting indices for rendering the shape as a triangle list.</param>
        public static void Triangulate(
            Vector2[] inputVertices,
            WindingOrder inputVerticesWindingOrder,
            WindingOrder outputVerticesWindingOrder,
            WindingOrder outputIndicesWindingOrder,
            out Vector2[] outputVertices,
            out int[] outputIndices)
        {
            //make sure we have our vertices wound in counter clockwise order            
            if (inputVerticesWindingOrder == WindingOrder.CounterClockwise)
                outputVertices = (Vector2[]) inputVertices.Clone();
            else
                outputVertices = ReverseWindingOrder(inputVertices);

            TriangulateInner(outputIndicesWindingOrder, outputVertices, out outputIndices);

            if (outputVerticesWindingOrder == WindingOrder.Clockwise)
                ReverseWindingOrder(outputVertices, outputIndices, out outputVertices, out outputIndices);
        }

        //output vertices will be in counter clockwise order in this stage
        private static void TriangulateInner(
            WindingOrder outputIndicesWindingOrder,
            Vector2[] outputVertices,
            out int[] outputIndices)
        {
            var triangles = new List<Triangle>();

            //clear all of the lists
            PolygonVertices.Clear();
            EarVertices.Clear();
            ConvexVertices.Clear();
            ReflexVertices.Clear();

            //generate the cyclical list of vertices in the polygon
            for (var i = 0; i < outputVertices.Length; i++)
                PolygonVertices.AddLast(new Vertex(outputVertices[i], i));

            //categorize all of the vertices as convex, reflex, and ear
            FindConvexAndReflexVertices();
            FindEarVertices();

            //clip all the ear vertices
            while (PolygonVertices.Count > 3 && EarVertices.Count > 0)
                ClipNextEar(triangles);

            //if there are still three points, use that for the last triangle
            if (PolygonVertices.Count == 3)
                triangles.Add(new Triangle(
                    PolygonVertices[0].Value,
                    PolygonVertices[1].Value,
                    PolygonVertices[2].Value));

            //add all of the triangle indices to the output array
            outputIndices = new int[triangles.Count*3];

            //move the if statement out of the loop to prevent all the
            //redundant comparisons
            if (outputIndicesWindingOrder == WindingOrder.CounterClockwise)
            {
                for (var i = 0; i < triangles.Count; i++)
                {
                    outputIndices[(i*3)] = triangles[i].A.Index;
                    outputIndices[(i*3) + 1] = triangles[i].B.Index;
                    outputIndices[(i*3) + 2] = triangles[i].C.Index;
                }
            }
            else
            {
                for (var i = 0; i < triangles.Count; i++)
                {
                    outputIndices[(i*3)] = triangles[i].C.Index;
                    outputIndices[(i*3) + 1] = triangles[i].B.Index;
                    outputIndices[(i*3) + 2] = triangles[i].A.Index;
                }
            }
        }

        #endregion

        #region EnsureWindingOrder

        /// <summary>
        ///     Ensures that a set of vertices are wound in a particular order, reversing them if necessary.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="outputWindingOrder">The desired winding order.</param>
        /// <returns>A new set of vertices if the winding order didn't match; otherwise the original set.</returns>
        public static Vector2[] EnsureWindingOrder(Vector2[] vertices, WindingOrder outputWindingOrder)
        {
            return EnsureWindingOrder(vertices, DetermineWindingOrder(vertices), outputWindingOrder);
        }

        /// <summary>
        ///     Ensures that a set of vertices are wound in a particular order, reversing them if necessary.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="inputWindingOrder">The input winding order.</param>
        /// <param name="outputWindingOrder">The desired winding order.</param>
        /// <returns>A new set of vertices if the winding order didn't match; otherwise the original set.</returns>
        public static Vector2[] EnsureWindingOrder(Vector2[] vertices, WindingOrder inputWindingOrder,
            WindingOrder outputWindingOrder)
        {
            if (inputWindingOrder != outputWindingOrder)
            {
                return ReverseWindingOrder(vertices);
            }


            return vertices;
        }

        #endregion

        #region ReverseWindingOrder

        /// <summary>
        ///     Reverses the winding order for a set of vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The new vertices for the polygon with the opposite winding order.</returns>
        public static Vector2[] ReverseWindingOrder(Vector2[] vertices)
        {
            var newVerts = new Vector2[vertices.Length];

            newVerts[0] = vertices[0];
            for (var i = 1; i < newVerts.Length; i++)
                newVerts[i] = vertices[vertices.Length - i];

            return newVerts;
        }

        /// <summary>
        ///     Reverses the winding order for a set of vertices and changes indices accordingly.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="indices">The indices of the polygon.</param>
        /// <param name="outputVertices">The new vertices for the polygon with the opposite winding order.</param>
        /// <param name="outputIndices">The new indices for the reversed vertices.</param>
        /// <returns></returns>
        public static void ReverseWindingOrder(Vector2[] vertices, int[] indices, out Vector2[] outputVertices,
            out int[] outputIndices)
        {
            outputVertices = ReverseWindingOrder(vertices);
            outputIndices = new int[indices.Length];
            for (var i = 1; i < vertices.Length; i++)
            {
                var oldVertexIndex = i;
                var newVertexIndex = vertices.Length - i;

                for (var j = 0; j < indices.Length; j++)
                {
                    if (indices[j] == oldVertexIndex)
                        outputIndices[j] = newVertexIndex;
                }
            }
        }

        #endregion

        #region DetermineWindingOrder

        /// <summary>
        ///     Determines the winding order of a polygon given a set of vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The calculated winding order of the polygon.</returns>
        public static WindingOrder DetermineWindingOrder(Vector2[] vertices)
        {
            var clockWiseCount = 0;
            var counterClockWiseCount = 0;
            var p1 = vertices[0];

            for (var i = 1; i < vertices.Length; i++)
            {
                var p2 = vertices[i];
                var p3 = vertices[(i + 1)%vertices.Length];

                var e1 = p1 - p2;
                var e2 = p3 - p2;

                if (e1.X*e2.Y - e1.Y*e2.X >= 0)
                    clockWiseCount++;
                else
                    counterClockWiseCount++;

                p1 = p2;
            }

            return (clockWiseCount > counterClockWiseCount)
                ? WindingOrder.Clockwise
                : WindingOrder.CounterClockwise;
        }

        #endregion

        #region IsConvex

        // Ref: http://farseerphysics.codeplex.com/SourceControl/latest#SourceFiles/Common/Vertices.cs

        /// <summary>
        ///     Determines whether the polygon is convex.
        ///     O(n^2) running time.
        ///     Assumptions:
        ///     - The polygon has no overlapping edges
        /// </summary>
        /// <returns>
        ///     <c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConvex(Vector2[] vertices)
        {
            return IsConvex(vertices, DetermineWindingOrder(vertices));
        }

        /// <summary>
        ///     Determines whether the polygon is convex.
        ///     O(n^2) running time.
        ///     Assumptions:
        ///     - The polygon has no overlapping edges
        /// </summary>
        /// <returns>
        ///     <c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConvex(Vector2[] vertices, WindingOrder verticesWindingOrder)
        {
            // Ensure vertices are wound in counter clockwise order.
            vertices = EnsureWindingOrder(vertices, verticesWindingOrder, WindingOrder.CounterClockwise);

            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (vertices.Length < 3)
                return false;

            //Triangles are always convex
            if (vertices.Length == 3)
                return true;

            // Checks the polygon is convex and the interior is to the left of each edge.
            for (var i = 0; i < vertices.Length; ++i)
            {
                var next = i + 1 < vertices.Length ? i + 1 : 0;
                var edge = vertices[next] - vertices[i];

                for (var j = 0; j < vertices.Length; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i || j == next)
                        continue;

                    var r = vertices[j] - vertices[i];

                    var s = edge.X*r.Y - edge.Y*r.X;

                    if (s <= 0.0f)
                        return false;
                }
            }
            return true;
        }

        #endregion

        #region DecomposeIntoConvex

        /// <summary>
        ///     Decompose the polygon into several smaller non-concave polygon.
        ///     If the polygon is already convex, it will return the original polygon, unless it is over 8 vertices.
        /// </summary>
        /// <param name="vertices">Vertices of a simple polygon.</param>
        /// <returns>A list of polygons with vertices wound in counter clockwise order.</returns>
        public static Vector2[][] DecomposeIntoConvex(Vector2[] vertices)
        {
            return DecomposeIntoConvex(vertices, DetermineWindingOrder(vertices), WindingOrder.CounterClockwise);
        }

        /// <summary>
        ///     Decompose the polygon into several smaller non-concave polygon.
        ///     If the polygon is already convex, it will return the original polygon, unless it is over 8 vertices.
        /// </summary>
        /// <param name="vertices">Vertices of a simple polygon.</param>
        /// <param name="inputWindingOrder">The winding order of input vertices.</param>
        /// <param name="outputWindingOrder">The desired winding order of output vertices.</param>
        /// <returns>A list of polygons with vertices wound in counter clockwise order.</returns>
        public static Vector2[][] DecomposeIntoConvex(Vector2[] vertices, WindingOrder inputWindingOrder,
            WindingOrder outputWindingOrder)
        {
            vertices = EnsureWindingOrder(vertices, inputWindingOrder, WindingOrder.CounterClockwise);
            var convexPolygons = BayazitDecomposer.ConvexPartition(vertices.ToList());

            if (outputWindingOrder == WindingOrder.Clockwise)
            {
                return convexPolygons.Select(polygon => ReverseWindingOrder(polygon.ToArray())).ToArray();
            }
            return convexPolygons.Select(polygon => polygon.ToArray()).ToArray();
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