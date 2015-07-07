/*
* Farseer Physics Engine based on Box2D.XNA port:
* Copyright (c) 2010 Ian Qvist
* 
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using Microsoft.Xna.Framework;
//using Vertices = System.Collections.Generic.List<Microsoft.Xna.Framework.Vector2>;
using Vertices = Penumbra.Mathematics.Polygon;

namespace Penumbra.Mathematics
{
    /// <summary>
    /// An axis aligned bounding box.
    /// </summary>
    internal struct AABB
    {       
        /// <summary>
        /// The lower vertex
        /// </summary>
        public Vector2 LowerBound;

        /// <summary>
        /// The upper vertex
        /// </summary>
        public Vector2 UpperBound;

        public AABB(Vector2 min, Vector2 max)
            : this(ref min, ref max)
        {
        }

        public AABB(ref Vector2 min, ref Vector2 max)
        {
            LowerBound = min;
            UpperBound = max;
        }

        public AABB(Vector2 center, float width, float height)
        {
            LowerBound = center - new Vector2(width/2, height/2);
            UpperBound = center + new Vector2(width/2, height/2);
        }

        /// <summary>
        /// Get the center of the AABB.
        /// </summary>
        /// <value></value>
        public Vector2 Center
        {
            get { return 0.5f*(LowerBound + UpperBound); }
        }

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        /// <value></value>
        public Vector2 Extents
        {
            get { return 0.5f*(UpperBound - LowerBound); }
        }

        /// <summary>
        /// Get the perimeter length
        /// </summary>
        /// <value></value>
        public float Perimeter
        {
            get
            {
                float wx = UpperBound.X - LowerBound.X;
                float wy = UpperBound.Y - LowerBound.Y;
                return 2.0f*(wx + wy);
            }
        }

        /// <summary>
        /// Gets the vertices of the AABB.
        /// </summary>
        /// <value>The corners of the AABB</value>
        public Vertices Vertices
        {
            get
            {
                var vertices = new Vertices(WindingOrder.CounterClockwise);
                vertices.Add(LowerBound);
                vertices.Add(new Vector2(LowerBound.X, UpperBound.Y));
                vertices.Add(UpperBound);
                vertices.Add(new Vector2(UpperBound.X, LowerBound.Y));
                return vertices;
            }
        }

        /// <summary>
        /// first quadrant
        /// </summary>
        public AABB Q1 => new AABB(Center, UpperBound);

        public AABB Q2 => new AABB(new Vector2(LowerBound.X, Center.Y), new Vector2(Center.X, UpperBound.Y));

        public AABB Q3 => new AABB(LowerBound, Center);

        public AABB Q4 => new AABB(new Vector2(Center.X, LowerBound.Y), new Vector2(UpperBound.X, Center.Y));

        public Vector2[] GetVertices()
        {
            Vector2 p1 = UpperBound;
            Vector2 p2 = new Vector2(UpperBound.X, LowerBound.Y);
            Vector2 p3 = LowerBound;
            Vector2 p4 = new Vector2(LowerBound.X, UpperBound.Y);
            return new[] {p1, p2, p3, p4};
        }

        /// <summary>
        /// Combine an AABB into this one.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        public void Combine(ref AABB aabb)
        {
            LowerBound = Vector2.Min(LowerBound, aabb.LowerBound);
            UpperBound = Vector2.Max(UpperBound, aabb.UpperBound);
        }

        /// <summary>
        /// Combine two AABBs into this one.
        /// </summary>
        /// <param name="aabb1">The aabb1.</param>
        /// <param name="aabb2">The aabb2.</param>
        public void Combine(ref AABB aabb1, ref AABB aabb2)
        {
            LowerBound = Vector2.Min(aabb1.LowerBound, aabb2.LowerBound);
            UpperBound = Vector2.Max(aabb1.UpperBound, aabb2.UpperBound);
        }

        /// <summary>
        /// Does this aabb contain the provided AABB.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified aabb; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref AABB aabb)
        {
            bool result = true;
            result = result && LowerBound.X <= aabb.LowerBound.X;
            result = result && LowerBound.Y <= aabb.LowerBound.Y;
            result = result && aabb.UpperBound.X <= UpperBound.X;
            result = result && aabb.UpperBound.Y <= UpperBound.Y;
            return result;
        }

        /// <summary>
        /// Determines whether the AAABB contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified point; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref Vector2 point)
        {
            //using epsilon to try and gaurd against float rounding errors.
            if ((point.X > (LowerBound.X + Calc.Epsilon) && point.X < (UpperBound.X - Calc.Epsilon) &&
                 (point.Y > (LowerBound.Y + Calc.Epsilon) && point.Y < (UpperBound.Y - Calc.Epsilon))))
            {
                return true;
            }
            return false;
        }

        public static bool TestOverlap(AABB a, AABB b)
        {
            return TestOverlap(ref a, ref b);
        }

        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            Vector2 d1 = b.LowerBound - a.UpperBound;
            Vector2 d2 = a.LowerBound - b.UpperBound;

            if (d1.X > 0.0f || d1.Y > 0.0f)
                return false;

            if (d2.X > 0.0f || d2.Y > 0.0f)
                return false;

            return true;
        }
    }
}