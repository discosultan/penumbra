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

namespace Penumbra.Geometry
{
    /// <summary>
    /// An axis aligned bounding box.
    /// </summary>
    internal struct BoundingRectangle
    {
        private const float Epsilon = 1e-5f;

        /// <summary>
        /// The lower vertex
        /// </summary>
        public Vector2 Min;

        /// <summary>
        /// The upper vertex
        /// </summary>
        public Vector2 Max;

        public BoundingRectangle(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Get the center of the AABB.
        /// </summary>
        /// <value></value>
        public Vector2 Center => 0.5f*(Min + Max);

        /// <summary>
        /// Get the extents of the AABB (half-widths).
        /// </summary>
        /// <value></value>
        public Vector2 Extents => 0.5f*(Max - Min);

        /// <summary>
        /// Get the perimeter length
        /// </summary>
        /// <value></value>
        public float Perimeter
        {
            get
            {
                float wx = Max.X - Min.X;
                float wy = Max.Y - Min.Y;
                return 2.0f*(wx + wy);
            }
        }

        /// <summary>
        /// first quadrant
        /// </summary>
        public BoundingRectangle Q1 => new BoundingRectangle(Center, Max);

        public BoundingRectangle Q2 => new BoundingRectangle(new Vector2(Min.X, Center.Y), new Vector2(Center.X, Max.Y));

        public BoundingRectangle Q3 => new BoundingRectangle(Min, Center);

        public BoundingRectangle Q4 => new BoundingRectangle(new Vector2(Center.X, Min.Y), new Vector2(Max.X, Center.Y));

        /// <summary>
        /// Combine an AABB into this one.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        public void Combine(ref BoundingRectangle aabb)
        {
            Min = Vector2.Min(Min, aabb.Min);
            Max = Vector2.Max(Max, aabb.Max);
        }

        /// <summary>
        /// Does this aabb contain the provided AABB.
        /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <returns>
        /// 	<c>true</c> if it contains the specified aabb; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(ref BoundingRectangle aabb)
        {
            bool result = true;
            result = result && Min.X <= aabb.Min.X;
            result = result && Min.Y <= aabb.Min.Y;
            result = result && aabb.Max.X <= Max.X;
            result = result && aabb.Max.Y <= Max.Y;
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
            if ((point.X > (Min.X + Epsilon) && point.X < (Max.X - Epsilon) &&
                 (point.Y > (Min.Y + Epsilon) && point.Y < (Max.Y - Epsilon))))
            {
                return true;
            }
            return false;
        }

        public bool Intersects(BoundingRectangle other)
        {
            return Intersects(ref other);
        }

        public bool Intersects(ref BoundingRectangle other)
        {
 
            Vector2 d1 = other.Min - Max;
            Vector2 d2 = Min - other.Max;

            if (d1.X > 0.0f || d1.Y > 0.0f)
                return false;

            if (d2.X > 0.0f || d2.Y > 0.0f)
                return false;

            return true;
        }        
    }
}