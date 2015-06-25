using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Triangulation
{
	/// <summary>
	/// A static class exposing methods for triangulating 2D polygons. This is the sole public
	/// class in the entire library; all other classes/structures are intended as internal-only
	/// objects used only to assist in triangulation.
	/// 
	/// This class makes use of the DEBUG conditional and produces quite verbose output when built
	/// in Debug mode. This is quite useful for debugging purposes, but can slow the process down
	/// quite a bit. For optimal performance, build the library in Release mode.
	/// 
	/// The triangulation is also not optimized for garbage sensitive processing. The point of the
	/// library is a robust, yet simple, system for triangulating 2D shapes. It is intended to be
	/// used as part of your content pipeline or at load-time. It is not something you want to be
	/// using each and every frame unless you really don't care about garbage.
	/// </summary>
	public static class Triangulator
	{
		#region Fields

		static readonly IndexableCyclicalLinkedList<Vertex> PolygonVertices = new IndexableCyclicalLinkedList<Vertex>();
		static readonly IndexableCyclicalLinkedList<Vertex> EarVertices = new IndexableCyclicalLinkedList<Vertex>();
		static readonly CyclicalList<Vertex> ConvexVertices = new CyclicalList<Vertex>();
		static readonly CyclicalList<Vertex> ReflexVertices = new CyclicalList<Vertex>();

		#endregion

		#region Public Methods

		#region Triangulate

	    /// <summary>
	    /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
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
	    /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
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
			Log("\nBeginning triangulation...");

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
            List<Triangle> triangles = new List<Triangle>();

            //clear all of the lists
            PolygonVertices.Clear();
            EarVertices.Clear();
            ConvexVertices.Clear();
            ReflexVertices.Clear();       

            //generate the cyclical list of vertices in the polygon
            for (int i = 0; i < outputVertices.Length; i++)
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
            outputIndices = new int[triangles.Count * 3];            

            //move the if statement out of the loop to prevent all the
            //redundant comparisons
            if (outputIndicesWindingOrder == WindingOrder.CounterClockwise)
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    outputIndices[(i * 3)] = triangles[i].A.Index;
                    outputIndices[(i * 3) + 1] = triangles[i].B.Index;
                    outputIndices[(i * 3) + 2] = triangles[i].C.Index;
                }
            }
            else
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    outputIndices[(i * 3)] = triangles[i].C.Index;
                    outputIndices[(i * 3) + 1] = triangles[i].B.Index;
                    outputIndices[(i * 3) + 2] = triangles[i].A.Index;
                }
            }            
        }

	    #endregion

		#region CutHoleInShape

	    /// <summary>
	    /// Cuts a hole into a shape.
	    /// </summary>
	    /// <param name="shapeVertices">An array of vertices for the primary shape.</param>	    
	    /// <param name="holeVertices">An array of vertices for the hole to be cut. It is assumed that these vertices lie completely within the shape verts.</param>	    
	    /// <returns>The new array of vertices in counter clockwise order that can be passed to Triangulate to properly triangulate the shape with the hole.</returns>
	    public static Vector2[] CutHoleInShape(Vector2[] shapeVertices, Vector2[] holeVertices)
	    {
	        return CutHoleInShape(
	            EnsureWindingOrder(shapeVertices, WindingOrder.CounterClockwise),
	            WindingOrder.CounterClockwise,
	            EnsureWindingOrder(holeVertices, WindingOrder.Clockwise),
	            WindingOrder.Clockwise);
	    }

	    /// <summary>
	    /// Cuts a hole into a shape.
	    /// </summary>
	    /// <param name="shapeVertices">An array of vertices for the primary shape.</param>
	    /// <param name="shapeWindingOrder">The shape vertices winding order.</param>
	    /// <param name="holeVertices">An array of vertices for the hole to be cut. It is assumed that these vertices lie completely within the shape verts.</param>
	    /// <param name="holeWindingOrder">The hole vertices winding order.</param>
        /// <param name="desiredWindingOrder">The desired winding order of output vertices.</param>
	    /// <returns>The new array of vertices that can be passed to Triangulate to properly triangulate the shape with the hole.</returns>
	    public static Vector2[] CutHoleInShape(
            Vector2[] shapeVertices, 
            WindingOrder shapeWindingOrder, 
            Vector2[] holeVertices, 
            WindingOrder holeWindingOrder,
            WindingOrder desiredWindingOrder = WindingOrder.CounterClockwise)
		{
			Log("\nCutting hole into shape...");

			//make sure the shape vertices are wound counter clockwise and the hole vertices clockwise
	        shapeVertices = EnsureWindingOrder(shapeVertices, shapeWindingOrder, WindingOrder.CounterClockwise);
	        holeVertices = EnsureWindingOrder(holeVertices, holeWindingOrder, WindingOrder.Clockwise);		    

			Vector2[] outputVertices = CutHoleInShapeInner(shapeVertices, holeVertices);

	        outputVertices = EnsureWindingOrder(outputVertices, WindingOrder.CounterClockwise, desiredWindingOrder);	        

	        return outputVertices;
		}

	    private static Vector2[] CutHoleInShapeInner(Vector2[] shapeVerts, Vector2[] holeVerts)
	    {
            //clear all of the lists
	        PolygonVertices.Clear();
	        EarVertices.Clear();
	        ConvexVertices.Clear();
	        ReflexVertices.Clear();

	        //generate the cyclical list of vertices in the polygon
	        for (int i = 0; i < shapeVerts.Length; i++)
	            PolygonVertices.AddLast(new Vertex(shapeVerts[i], i));

	        var holePolygon = new CyclicalList<Vertex>();
	        for (int i = 0; i < holeVerts.Length; i++)
	            holePolygon.Add(new Vertex(holeVerts[i], i + PolygonVertices.Count));

#if DEBUG
	        var vString = new StringBuilder();
	        foreach (Vertex v in PolygonVertices)
	            vString.Append(string.Format("{0}, ", v));
	        Log("Shape Vertices: {0}", vString);

	        vString = new StringBuilder();
	        foreach (Vertex v in holePolygon)
	            vString.Append(string.Format("{0}, ", v));
	        Log("Hole Vertices: {0}", vString);
#endif

	        FindConvexAndReflexVertices();
	        FindEarVertices();

	        //find the hole vertex with the largest X value
	        Vertex rightMostHoleVertex = holePolygon[0];
	        foreach (Vertex v in holePolygon)
	            if (v.Position.X > rightMostHoleVertex.Position.X)
	                rightMostHoleVertex = v;

	        //construct a list of all line segments where at least one vertex
	        //is to the right of the rightmost hole vertex with one vertex
	        //above the hole vertex and one below
	        var segmentsToTest = new List<LineSegment>();
	        for (int i = 0; i < PolygonVertices.Count; i++)
	        {
	            Vertex a = PolygonVertices[i].Value;
	            Vertex b = PolygonVertices[i + 1].Value;

	            if ((a.Position.X > rightMostHoleVertex.Position.X || b.Position.X > rightMostHoleVertex.Position.X) &&
	                ((a.Position.Y >= rightMostHoleVertex.Position.Y && b.Position.Y <= rightMostHoleVertex.Position.Y) ||
	                 (a.Position.Y <= rightMostHoleVertex.Position.Y && b.Position.Y >= rightMostHoleVertex.Position.Y)))
	                segmentsToTest.Add(new LineSegment(a, b));
	        }

	        //now we try to find the closest intersection point heading to the right from
	        //our hole vertex.
	        float? closestPoint = null;
	        var closestSegment = new LineSegment();
	        foreach (LineSegment segment in segmentsToTest)
	        {
	            float? intersection = segment.IntersectsWithRay(rightMostHoleVertex.Position, Vector2.UnitX);
	            if (intersection != null)
	            {
	                if (closestPoint == null || closestPoint.Value > intersection.Value)
	                {
	                    closestPoint = intersection;
	                    closestSegment = segment;
	                }
	            }
	        }

	        //if closestPoint is null, there were no collisions (likely from improper input data),
	        //but we'll just return without doing anything else
	        if (closestPoint == null)
	            return shapeVerts;

	        //otherwise we can find our mutually visible vertex to split the polygon
	        Vector2 I = rightMostHoleVertex.Position + Vector2.UnitX * closestPoint.Value;
	        Vertex p = (closestSegment.A.Position.X > closestSegment.B.Position.X)
	            ? closestSegment.A
	            : closestSegment.B;

	        //construct triangle MIP
	        var mip = new Triangle(rightMostHoleVertex, new Vertex(I, 1), p);

	        //see if any of the reflex vertices lie inside of the MIP triangle
	        var interiorReflexVertices = new List<Vertex>();
	        foreach (Vertex v in ReflexVertices)
	            if (mip.ContainsPoint(v))
	                interiorReflexVertices.Add(v);

	        //if there are any interior reflex vertices, find the one that, when connected
	        //to our rightMostHoleVertex, forms the line closest to Vector2.UnitX
	        if (interiorReflexVertices.Count > 0)
	        {
	            float closestDot = -1f;
	            foreach (Vertex v in interiorReflexVertices)
	            {
	                //compute the dot product of the vector against the UnitX
	                Vector2 d = Vector2.Normalize(v.Position - rightMostHoleVertex.Position);
	                float dot = Vector2.Dot(Vector2.UnitX, d);

	                //if this line is the closest we've found
	                if (dot > closestDot)
	                {
	                    //save the value and save the vertex as P
	                    closestDot = dot;
	                    p = v;
	                }
	            }
	        }

	        //now we just form our output array by injecting the hole vertices into place
	        //we know we have to inject the hole into the main array after point P going from
	        //rightMostHoleVertex around and then back to P.
	        int mIndex = holePolygon.IndexOf(rightMostHoleVertex);
	        int injectPoint = PolygonVertices.IndexOf(p);

	        Log("Inserting hole at injection point {0} starting at hole vertex {1}.",
	            p,
	            rightMostHoleVertex);
	        for (int i = mIndex; i <= mIndex + holePolygon.Count; i++)
	        {
	            Log("Inserting vertex {0} after vertex {1}.", holePolygon[i], PolygonVertices[injectPoint].Value);
	            PolygonVertices.AddAfter(PolygonVertices[injectPoint++], holePolygon[i]);
	        }
	        PolygonVertices.AddAfter(PolygonVertices[injectPoint], p);

#if DEBUG
	        vString = new StringBuilder();
	        foreach (Vertex v in PolygonVertices)
	            vString.Append(string.Format("{0}, ", v));
	        Log("New Shape Vertices: {0}\n", vString);
#endif

	        //finally we write out the new polygon vertices and return them out
	        var newShapeVerts = new Vector2[PolygonVertices.Count];
	        for (int i = 0; i < PolygonVertices.Count; i++)
	            newShapeVerts[i] = PolygonVertices[i].Value.Position;

	        return newShapeVerts;
	    }

	    #endregion

		#region EnsureWindingOrder

		/// <summary>
		/// Ensures that a set of vertices are wound in a particular order, reversing them if necessary.
		/// </summary>
		/// <param name="vertices">The vertices of the polygon.</param>
		/// <param name="outputWindingOrder">The desired winding order.</param>
		/// <returns>A new set of vertices if the winding order didn't match; otherwise the original set.</returns>
		public static Vector2[] EnsureWindingOrder(Vector2[] vertices, WindingOrder outputWindingOrder)
		{
		    return EnsureWindingOrder(vertices, DetermineWindingOrder(vertices), outputWindingOrder);
		}

	    /// <summary>
	    /// Ensures that a set of vertices are wound in a particular order, reversing them if necessary.
	    /// </summary>
	    /// <param name="vertices">The vertices of the polygon.</param>	    
	    /// <param name="inputWindingOrder">The input winding order.</param>
        /// <param name="outputWindingOrder">The desired winding order.</param>
	    /// <returns>A new set of vertices if the winding order didn't match; otherwise the original set.</returns>
	    public static Vector2[] EnsureWindingOrder(Vector2[] vertices, WindingOrder inputWindingOrder, WindingOrder outputWindingOrder)
        {
            Log("\nEnsuring winding order of {0}...", outputWindingOrder);
            if (inputWindingOrder != outputWindingOrder)
            {
                Log("Reversing vertices...");
                return ReverseWindingOrder(vertices);
            }

            Log("No reversal needed.");
            return vertices;
        }

		#endregion

		#region ReverseWindingOrder

	    /// <summary>
	    /// Reverses the winding order for a set of vertices.
	    /// </summary>
	    /// <param name="vertices">The vertices of the polygon.</param>
	    /// <returns>The new vertices for the polygon with the opposite winding order.</returns>
	    public static Vector2[] ReverseWindingOrder(Vector2[] vertices)
	    {
            Log("\nReversing winding order...");
            var newVerts = new Vector2[vertices.Length];

#if DEBUG
            var vString = new StringBuilder();
            foreach (Vector2 v in vertices)
                vString.Append(string.Format("{0}, ", v));
            Log("Original Vertices: {0}", vString);
#endif

            newVerts[0] = vertices[0];
            for (int i = 1; i < newVerts.Length; i++)
                newVerts[i] = vertices[vertices.Length - i];

#if DEBUG
            vString = new StringBuilder();
            foreach (Vector2 v in newVerts)
                vString.Append(string.Format("{0}, ", v));
            Log("New Vertices After Reversal: {0}\n", vString);
#endif

            return newVerts;
		}

	    /// <summary>
	    /// Reverses the winding order for a set of vertices and changes indices accordingly.
	    /// </summary>
	    /// <param name="vertices">The vertices of the polygon.</param>
	    /// <param name="indices">The indices of the polygon.</param>
        /// <param name="outputVertices">The new vertices for the polygon with the opposite winding order.</param>
        /// <param name="outputIndices">The new indices for the reversed vertices.</param>
	    /// <returns></returns>
	    public static void ReverseWindingOrder(Vector2[] vertices, int[] indices, out Vector2[] outputVertices, out int[] outputIndices)
	    {
            outputVertices = ReverseWindingOrder(vertices);
            outputIndices = new int[indices.Length];
            for (int i = 1; i < vertices.Length; i++)
            {
                int oldVertexIndex = i;
                int newVertexIndex = vertices.Length - i;

                for (int j = 0; j < indices.Length; j++)
                {
                    if (indices[j] == oldVertexIndex)
                        outputIndices[j] = newVertexIndex;
                }
            }
	    }

		#endregion

		#region DetermineWindingOrder

		/// <summary>
		/// Determines the winding order of a polygon given a set of vertices.
		/// </summary>
		/// <param name="vertices">The vertices of the polygon.</param>
		/// <returns>The calculated winding order of the polygon.</returns>
		public static WindingOrder DetermineWindingOrder(Vector2[] vertices)
		{
			int clockWiseCount = 0;
			int counterClockWiseCount = 0;
			Vector2 p1 = vertices[0];

			for (int i = 1; i < vertices.Length; i++)
			{
				Vector2 p2 = vertices[i];
				Vector2 p3 = vertices[(i + 1) % vertices.Length];

				Vector2 e1 = p1 - p2;
				Vector2 e2 = p3 - p2;

				if (e1.X * e2.Y - e1.Y * e2.X >= 0)
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
        /// Determines whether the polygon is convex.
        /// O(n^2) running time.
        /// 
        /// Assumptions:        
        /// - The polygon has no overlapping edges
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConvex(Vector2[] vertices)
        {
            return IsConvex(vertices, DetermineWindingOrder(vertices));
        }
        
        /// <summary>
        /// Determines whether the polygon is convex.
        /// O(n^2) running time.
        /// 
        /// Assumptions:
        /// - The polygon has no overlapping edges
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
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
            for (int i = 0; i < vertices.Length; ++i)
            {
                int next = i + 1 < vertices.Length ? i + 1 : 0;
                Vector2 edge = vertices[next] - vertices[i];

                for (int j = 0; j < vertices.Length; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i || j == next)
                        continue;

                    Vector2 r = vertices[j] - vertices[i];

                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0.0f)
                        return false;
                }
            }
            return true;
        }

        #endregion

        #region DecomposeIntoConvex

        /// <summary>
        /// Decompose the polygon into several smaller non-concave polygon.
        /// If the polygon is already convex, it will return the original polygon, unless it is over 8 vertices.
        /// </summary>
        /// <param name="vertices">Vertices of a simple polygon.</param>
        /// <returns>A list of polygons with vertices wound in counter clockwise order.</returns>
        public static Vector2[][] DecomposeIntoConvex(Vector2[] vertices)
        {            
            return DecomposeIntoConvex(vertices, DetermineWindingOrder(vertices), WindingOrder.CounterClockwise);
        }

	    /// <summary>
	    /// Decompose the polygon into several smaller non-concave polygon.
	    /// If the polygon is already convex, it will return the original polygon, unless it is over 8 vertices.
	    /// </summary>
	    /// <param name="vertices">Vertices of a simple polygon.</param>
	    /// <param name="inputWindingOrder">The winding order of input vertices.</param>
	    /// <param name="outputWindingOrder">The desired winding order of output vertices.</param>
	    /// <returns>A list of polygons with vertices wound in counter clockwise order.</returns>
	    public static Vector2[][] DecomposeIntoConvex(Vector2[] vertices, WindingOrder inputWindingOrder, WindingOrder outputWindingOrder)
        {
            vertices = EnsureWindingOrder(vertices, inputWindingOrder, WindingOrder.CounterClockwise);
            List<List<Vector2>> convexPolygons = BayazitDecomposer.ConvexPartition(vertices.ToList());

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
			Vertex ear = EarVertices[0].Value;
			Vertex prev = PolygonVertices[PolygonVertices.IndexOf(ear) - 1].Value;
			Vertex next = PolygonVertices[PolygonVertices.IndexOf(ear) + 1].Value;
			triangles.Add(new Triangle(ear, next, prev));

			//remove the ear from the shape
			EarVertices.RemoveAt(0);
			PolygonVertices.RemoveAt(PolygonVertices.IndexOf(ear));
			Log("\nRemoved Ear: {0}", ear);

			//validate the neighboring vertices
			ValidateAdjacentVertex(prev);
			ValidateAdjacentVertex(next);

			//write out the states of each of the lists
#if DEBUG
			var rString = new StringBuilder();
			foreach (Vertex v in ReflexVertices)
				rString.Append(string.Format("{0}, ", v.Index));
			Log("Reflex Vertices: {0}", rString);

			var cString = new StringBuilder();
			foreach (Vertex v in ConvexVertices)
				cString.Append(string.Format("{0}, ", v.Index));
			Log("Convex Vertices: {0}", cString);

			var eString = new StringBuilder();
			foreach (Vertex v in EarVertices)
				eString.Append(string.Format("{0}, ", v.Index));
			Log("Ear Vertices: {0}", eString);
#endif
		}

		#endregion

		#region ValidateAdjacentVertex

		private static void ValidateAdjacentVertex(Vertex vertex)
		{
			Log("Validating: {0}...", vertex);

			if (ReflexVertices.Contains(vertex))
			{
				if (IsConvex(vertex))
				{
					ReflexVertices.Remove(vertex);
					ConvexVertices.Add(vertex);
					Log("Vertex: {0} now convex", vertex);
				}
				else
				{
					Log("Vertex: {0} still reflex", vertex);
				}
			}

			if (ConvexVertices.Contains(vertex))
			{
				bool wasEar = EarVertices.Contains(vertex);
				bool isEar = IsEar(vertex);

				if (wasEar && !isEar)
				{
					EarVertices.Remove(vertex);
					Log("Vertex: {0} no longer ear", vertex);
				}
				else if (!wasEar && isEar)
				{
					EarVertices.AddFirst(vertex);
					Log("Vertex: {0} now ear", vertex);
				}
				else
				{
					Log("Vertex: {0} still ear", vertex);
				}
			}
		}

		#endregion

		#region FindConvexAndReflexVertices

		private static void FindConvexAndReflexVertices()
		{
			for (int i = 0; i < PolygonVertices.Count; i++)
			{
				Vertex v = PolygonVertices[i].Value;

				if (IsConvex(v))
				{
					ConvexVertices.Add(v);
					Log("Convex: {0}", v);
				}
				else
				{
					ReflexVertices.Add(v);
					Log("Reflex: {0}", v);
				}
			}
		}

		#endregion

		#region FindEarVertices

		private static void FindEarVertices()
		{
			for (int i = 0; i < ConvexVertices.Count; i++)
			{
				Vertex c = ConvexVertices[i];

				if (IsEar(c))
				{
					EarVertices.AddLast(c);
					Log("Ear: {0}", c);
				}
			}
		}

		#endregion

		#region IsEar

		private static bool IsEar(Vertex c)
		{
			Vertex p = PolygonVertices[PolygonVertices.IndexOf(c) - 1].Value;
			Vertex n = PolygonVertices[PolygonVertices.IndexOf(c) + 1].Value;

			Log("Testing vertex {0} as ear with triangle {1}, {0}, {2}...", c, p, n);

			foreach (Vertex t in ReflexVertices)
			{
				if (t.Equals(p) || t.Equals(c) || t.Equals(n))
					continue;

				if (Triangle.ContainsPoint(p, c, n, t))
				{
					Log("\tTriangle contains vertex {0}...", t);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region IsConvex

		private static bool IsConvex(Vertex c)
		{
			Vertex p = PolygonVertices[PolygonVertices.IndexOf(c) - 1].Value;
			Vertex n = PolygonVertices[PolygonVertices.IndexOf(c) + 1].Value; 
			
			Vector2 d1 = Vector2.Normalize(c.Position - p.Position);
			Vector2 d2 = Vector2.Normalize(n.Position - c.Position);
			Vector2 n2 = new Vector2(-d2.Y, d2.X);

			return (Vector2.Dot(d1, n2) <= 0f);
		}

		#endregion

		#region IsReflex

		private static bool IsReflex(Vertex c)
		{
			return !IsConvex(c);
		}

		#endregion

		#region Log

		[Conditional("DEBUG")]
		private static void Log(string format, params object[] parameters)
		{
			Debug.WriteLine(format, parameters);
		}

		#endregion

		#endregion
	}
}