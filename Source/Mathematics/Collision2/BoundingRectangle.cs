#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion




#if UseDouble
using Scalar = System.Double;
#else
using System;
using Scalar = System.Single;
#endif
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Collision2
{
    [StructLayout(LayoutKind.Sequential, Size = BoundingRectangle.Size)]    
    public struct BoundingRectangle : IEquatable<BoundingRectangle>
    {
        public const int Size = VectorUtil.Size * 2;

        public static void Transform(ref Matrix2x3 matrix,ref BoundingRectangle rect, out BoundingRectangle result)
        {
            FromVectors(ref matrix, rect.Corners(), out result);
        }

        /// <summary>
        /// Creates a new BoundingRectangle Instance from 2 Vector2s.
        /// </summary>
        /// <param name="first">the first Vector2.</param>
        /// <param name="second">the second Vector2.</param>
        /// <returns>a new BoundingRectangle</returns>
        /// <remarks>The Max and Min values are automatically determined.</remarks>
        public static BoundingRectangle FromVectors(Vector2 first, Vector2 second)
        {
            BoundingRectangle result;
            if (first.X > second.X)
            {
                result.Max.X = first.X;
                result.Min.X = second.X;
            }
            else
            {
                result.Max.X = second.X;
                result.Min.X = first.X;
            }
            if (first.Y > second.Y)
            {
                result.Max.Y = first.Y;
                result.Min.Y = second.Y;
            }
            else
            {
                result.Max.Y = second.Y;
                result.Min.Y = first.Y;
            }
            return result;
        }
        public static void FromVectors(ref Vector2 first, ref  Vector2 second, out BoundingRectangle result)
        {
            if (first.X > second.X)
            {
                result.Max.X = first.X;
                result.Min.X = second.X;
            }
            else
            {
                result.Max.X = second.X;
                result.Min.X = first.X;
            }
            if (first.Y > second.Y)
            {
                result.Max.Y = first.Y;
                result.Min.Y = second.Y;
            }
            else
            {
                result.Max.Y = second.Y;
                result.Min.Y = first.Y;
            }
        }
        /// <summary>
        /// Creates a new BoundingRectangle Instance from multiple Vector2s.
        /// </summary>
        /// <param name="vectors">the list of vectors</param>
        /// <returns>a new BoundingRectangle</returns>
        /// <remarks>The Max and Min values are automatically determined.</remarks>
        public static BoundingRectangle FromVectors(Vector2[] vectors)
        {
            BoundingRectangle result;
            FromVectors(vectors, out result);
            return result;
        }
        public static void FromVectors(Vector2[] vectors, out BoundingRectangle result)
        {
            if (vectors == null) { throw new ArgumentNullException(nameof(vectors)); }
            if (vectors.Length == 0) { throw new ArgumentOutOfRangeException(nameof(vectors)); }
            result.Max = vectors[0];
            result.Min = vectors[0];
            for (int index = 1; index < vectors.Length; ++index)
            {
                Vector2 current = vectors[index];
                if (current.X > result.Max.X)
                {
                    result.Max.X = current.X;
                }
                else if (current.X < result.Min.X)
                {
                    result.Min.X = current.X;
                }
                if (current.Y > result.Max.Y)
                {
                    result.Max.Y = current.Y;
                }
                else if (current.Y < result.Min.Y)
                {
                    result.Min.Y = current.Y;
                }
            }
        }
        public static void FromVectors(ref Matrix3x3 matrix, Vector2[] vectors, out BoundingRectangle result)
        {
            if (vectors == null) { throw new ArgumentNullException(nameof(vectors)); }
            if (vectors.Length == 0) { throw new ArgumentOutOfRangeException(nameof(vectors)); }

            Vector2 current;
            Vector2.Transform(ref matrix, ref vectors[0], out current);
            result.Max = current;
            result.Min = current;
            for (int index = 1; index < vectors.Length; ++index)
            {
                Vector2.Transform(ref matrix, ref vectors[index], out current);
                if (current.X > result.Max.X)
                {
                    result.Max.X = current.X;
                }
                else if (current.X < result.Min.X)
                {
                    result.Min.X = current.X;
                }
                if (current.Y > result.Max.Y)
                {
                    result.Max.Y = current.Y;
                }
                else if (current.Y < result.Min.Y)
                {
                    result.Min.Y = current.Y;
                }
            }
        }
        public static void FromVectors(ref Matrix2x3 matrix, Vector2[] vectors, out BoundingRectangle result)
        {
            if (vectors == null) { throw new ArgumentNullException(nameof(vectors)); }
            if (vectors.Length == 0) { throw new ArgumentOutOfRangeException(nameof(vectors)); }

            Vector2 current;
            Vector2.TransformNormal(ref matrix, ref vectors[0], out current);
            result.Max = current;
            result.Min = current;
            for (int index = 1; index < vectors.Length; ++index)
            {
                Vector2.TransformNormal(ref matrix, ref vectors[index], out current);
                if (current.X > result.Max.X)
                {
                    result.Max.X = current.X;
                }
                else if (current.X < result.Min.X)
                {
                    result.Min.X = current.X;
                }
                if (current.Y > result.Max.Y)
                {
                    result.Max.Y = current.Y;
                }
                else if (current.Y < result.Min.Y)
                {
                    result.Min.Y = current.Y;
                }
            }
            result.Max.X += matrix.m02;
            result.Max.Y += matrix.m12;
            result.Min.X += matrix.m02;
            result.Min.Y += matrix.m12;
        }
        /// <summary>
        /// Makes a BoundingRectangle that can contain the 2 BoundingRectangles passed.
        /// </summary>
        /// <param name="first">The First BoundingRectangle.</param>
        /// <param name="second">The Second BoundingRectangle.</param>
        /// <returns>The BoundingRectangle that can contain the 2 BoundingRectangles passed.</returns>
        public static BoundingRectangle FromUnion(BoundingRectangle first, BoundingRectangle second)
        {
            BoundingRectangle result;
            Vector2.Max(ref first.Max, ref second.Max, out result.Max);
            Vector2.Min(ref first.Min, ref second.Min, out result.Min);
            return result;
        }
        public static void FromUnion(ref BoundingRectangle first, ref BoundingRectangle second, out BoundingRectangle result)
        {
            Vector2.Max(ref first.Max, ref second.Max, out result.Max);
            Vector2.Min(ref first.Min, ref second.Min, out result.Min);
        }
        /// <summary>
        /// Makes a BoundingRectangle that contains the area where the BoundingRectangles Intersect.
        /// </summary>
        /// <param name="first">The First BoundingRectangle.</param>
        /// <param name="second">The Second BoundingRectangle.</param>
        /// <returns>The BoundingRectangle that can contain the 2 BoundingRectangles passed.</returns>
        public static BoundingRectangle FromIntersection(BoundingRectangle first, BoundingRectangle second)
        {
            BoundingRectangle result;
            Vector2.Min(ref first.Max, ref second.Max, out result.Max);
            Vector2.Max(ref first.Min, ref second.Min, out result.Min);
            return result;
        }
        public static void FromIntersection(ref BoundingRectangle first, ref BoundingRectangle second, out BoundingRectangle result)
        {
            Vector2.Min(ref first.Max, ref second.Max, out result.Max);
            Vector2.Max(ref first.Min, ref second.Min, out result.Min);
        }

        public static BoundingRectangle FromCircle(BoundingCircle circle)
        {
            BoundingRectangle result;
            FromCircle(ref circle, out result);
            return result;
        }
        public static void FromCircle(ref BoundingCircle circle, out BoundingRectangle result)
        {
            result.Max.X = circle.Position.X + circle.Radius;
            result.Max.Y = circle.Position.Y + circle.Radius;
            result.Min.X = circle.Position.X - circle.Radius;
            result.Min.Y = circle.Position.Y - circle.Radius;
        }

        public static BoundingRectangle FromCircle(Matrix2x3 matrix, Scalar radius)
        {
            BoundingRectangle result;
            FromCircle(ref matrix, ref radius, out result);
            return result;
        }
        public static void FromCircle(ref Matrix2x3 matrix, ref Scalar radius, out BoundingRectangle result)
        {
            Scalar xRadius = matrix.m01 * matrix.m01 + matrix.m00 * matrix.m00;
            xRadius = ((xRadius == 1) ? (radius) : (radius * MathHelper.Sqrt(xRadius)));
            Scalar yRadius = matrix.m10 * matrix.m10 + matrix.m11 * matrix.m11;
            yRadius = ((yRadius == 1) ? (radius) : (radius * MathHelper.Sqrt(yRadius)));

            result.Max.X = matrix.m02 + xRadius;
            result.Min.X = matrix.m02 - xRadius;
            result.Max.Y = matrix.m12 + yRadius;
            result.Min.Y = matrix.m12 - yRadius;
        }
                        
        public Vector2 Max;        
        public Vector2 Min;

        /// <summary>
        /// Creates a new BoundingRectangle Instance.
        /// </summary>
        /// <param name="minX">The Lower Bound on the XAxis.</param>
        /// <param name="minY">The Lower Bound on the YAxis.</param>
        /// <param name="maxX">The Upper Bound on the XAxis.</param>
        /// <param name="maxY">The Upper Bound on the YAxis.</param>
        public BoundingRectangle(Scalar minX, Scalar minY, Scalar maxX, Scalar maxY)
        {
            this.Max.X = maxX;
            this.Max.Y = maxY;
            this.Min.X = minX;
            this.Min.Y = minY;
        }
        /// <summary>
        /// Creates a new BoundingRectangle Instance from 2 Vector2s.
        /// </summary>
        /// <param name="min">The Lower Vector2.</param>
        /// <param name="max">The Upper Vector2.</param>        
        public BoundingRectangle(Vector2 min, Vector2 max)
        {
            Max = max;
            Min = min;
        }

        public Scalar Area => (Max.X - Min.X) * (Max.Y - Min.Y);

        public Scalar Perimeter => ((Max.X - Min.X) + (Max.Y - Min.Y)) * 2;

        public Vector2[] Corners()
        {
            return new Vector2[4]
            {
                Max,
                new Vector2(Min.X, Max.Y),
                Min,
                new Vector2(Max.X, Min.Y),
            };
        }

        public Scalar GetDistance(Vector2 point)
        {
            Scalar result;
            GetDistance(ref point, out result);
            return result;
        }
        public void GetDistance(ref Vector2 point, out Scalar result)
        {
            Scalar xDistance = Math.Abs(point.X - ((Max.X + Min.X) * .5f)) - (Max.X - Min.X) * .5f;
            Scalar yDistance = Math.Abs(point.Y - ((Max.Y + Min.Y) * .5f)) - (Max.Y - Min.Y) * .5f;
            if (xDistance > 0 && yDistance > 0)
            {
                result = Calc.Sqrt(xDistance * xDistance + yDistance * yDistance);
            }
            else
            {
                result = Math.Max(xDistance, yDistance);
            }
        }

        public ContainmentType Contains(Vector2 point)
        {
            if (point.X <= Max.X &&
                point.X >= Min.X &&
                point.Y <= Max.Y &&
                point.Y >= Min.Y)
            {
                return ContainmentType.Contains;
            }
            return ContainmentType.Disjoint;
        }

        public void Contains(ref Vector2 point, out ContainmentType result)
        {
            if (point.X <= Max.X &&
                point.X >= Min.X &&
                point.Y <= Max.Y &&
                point.Y >= Min.Y)
            {
                result = ContainmentType.Contains;
            }
            else
            {
                result = ContainmentType.Disjoint;
            }
        }

        public ContainmentType Contains(BoundingRectangle rect)
        {
            if (Min.X > rect.Max.X ||
                Min.Y > rect.Max.Y ||
                Max.X < rect.Min.X ||
                Max.Y < rect.Min.Y)
            {
                return ContainmentType.Disjoint;
            }
            if (Min.X <= rect.Min.X &&
                Min.Y <= rect.Min.Y &&
                Max.X >= rect.Max.X &&
                Max.Y >= rect.Max.Y)
            {
                return ContainmentType.Contains;
            }
            return ContainmentType.Intersects;
        }

        public void Contains(ref BoundingRectangle rect, out ContainmentType result)
        {
            if (Min.X > rect.Max.X ||
                Min.Y > rect.Max.Y ||
                Max.X < rect.Min.X ||
                Max.Y < rect.Min.Y)
            {
                result = ContainmentType.Disjoint;
            }
            else if (
                Min.X <= rect.Min.X &&
                Min.Y <= rect.Min.Y &&
                Max.X >= rect.Max.X &&
                Max.Y >= rect.Max.Y)
            {
                result = ContainmentType.Contains;
            }
            else
            {
                result = ContainmentType.Intersects;
            }
        }

        public ContainmentType Contains(BoundingCircle circle)
        {
             if ((circle.Position.X + circle.Radius) <= Max.X &&
                 (circle.Position.X - circle.Radius) >= Min.X &&
                 (circle.Position.Y + circle.Radius) <= Max.Y &&
                 (circle.Position.Y - circle.Radius) >= Min.Y)
            {
                return ContainmentType.Contains;
            }
            bool intersects;
            circle.Intersects(ref this, out intersects);
            return intersects ? ContainmentType.Intersects : ContainmentType.Disjoint;
        }
        public void Contains(ref BoundingCircle circle, out ContainmentType result)
        {
            if ((circle.Position.X + circle.Radius) <= Max.X &&
                (circle.Position.X - circle.Radius) >= Min.X &&
                (circle.Position.Y + circle.Radius) <= Max.Y &&
                (circle.Position.Y - circle.Radius) >= Min.Y)
            {
                result = ContainmentType.Contains;
            }
            else
            {
                bool intersects;
                circle.Intersects(ref this, out intersects);
                result = intersects ? ContainmentType.Intersects : ContainmentType.Disjoint;
            }
        }

        public ContainmentType Contains(BoundingPolygon polygon)
        {
            ContainmentType result;
            Contains(ref polygon, out result);
            return result;
        }
        public void Contains(ref BoundingPolygon polygon, out ContainmentType result)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            Vector2[] vertexes = polygon.Vertexes;
            result = ContainmentType.Disjoint;
            for (int index = 0; index < vertexes.Length && result != ContainmentType.Intersects; ++index)
            {
                ContainmentType con;
                Contains(ref vertexes[index], out con);
                result |= con;
            }
            if (result == ContainmentType.Disjoint)
            {
                bool test;
                polygon.Intersects(ref this, out test);
                if (test)
                {
                    result = ContainmentType.Intersects;
                }
            }
        }

        public Scalar Intersects(Ray ray)
        {
            Scalar result;
            Intersects(ref ray, out result);
            return result;
        }
        public bool Intersects(BoundingRectangle rect)
        {
            return
                Min.X < rect.Max.X &&
                Max.X > rect.Min.X &&
                Max.Y > rect.Min.Y &&
                Min.Y < rect.Max.Y;
        }
        public bool Intersects(BoundingCircle circle)
        {
            bool result;
            circle.Intersects(ref this, out result);
            return result;
        }
        public bool Intersects(Line line)
        {
            bool result;
            line.Intersects(ref this, out result);
            return result;
        }
        public bool Intersects(BoundingPolygon polygon)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            bool result;
            polygon.Intersects(ref this, out result);
            return result;
        }

        public void Intersects(ref Ray ray, out Scalar result)
        {

            if (Contains(ray.Origin)== ContainmentType.Contains)
            {
                result = 0;
                return;
            }
            Scalar distance;
            Scalar intersectValue;
            result = -1;
            if (ray.Origin.X < Min.X && ray.Direction.X > 0)
            {
                distance = (Min.X - ray.Origin.X) / ray.Direction.X;
                if (distance > 0)
                {
                    intersectValue = ray.Origin.Y + ray.Direction.Y * distance;
                    if (intersectValue >= Min.Y && intersectValue <= Max.Y &&
                        (result == -1 || distance < result))
                    {
                        result = distance;
                    }
                }
            }
            if (ray.Origin.X > Max.X && ray.Direction.X < 0)
            {
                distance = (Max.X - ray.Origin.X) / ray.Direction.X;
                if (distance > 0)
                {
                    intersectValue = ray.Origin.Y + ray.Direction.Y * distance;
                    if (intersectValue >= Min.Y && intersectValue <= Max.Y &&
                        (result == -1 || distance < result))
                    {
                        result = distance;
                    }
                }
            }
            if (ray.Origin.Y < Min.Y && ray.Direction.Y > 0)
            {
                distance = (Min.Y - ray.Origin.Y) / ray.Direction.Y;
                if (distance > 0)
                {
                    intersectValue = ray.Origin.X + ray.Direction.X * distance;
                    if (intersectValue >= Min.X && intersectValue <= Max.X &&
                        (result == -1 || distance < result))
                    {
                        result = distance;
                    }
                }
            }
            if (ray.Origin.Y > Max.Y && ray.Direction.Y < 0)
            {
                distance = (Max.Y - ray.Origin.Y) / ray.Direction.Y;
                if (distance > 0)
                {
                    intersectValue = ray.Origin.X + ray.Direction.X * distance;
                    if (intersectValue >= Min.X && intersectValue <= Max.X &&
                        (result == -1 || distance < result))
                    {
                        result = distance;
                    }
                }
            }
        }
        public void Intersects(ref BoundingRectangle rect, out bool result)
        {
            result =
                Min.X <= rect.Max.X &&
                Max.X >= rect.Min.X &&
                Max.Y >= rect.Min.Y &&
                Min.Y <= rect.Max.Y;
        }
        public void Intersects(ref BoundingCircle circle, out bool result)
        {
            circle.Intersects(ref this, out result);
        }
        public void Intersects(ref BoundingPolygon polygon, out bool result)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            polygon.Intersects(ref this, out result);
        }
        public void Intersects(ref Line line, out bool result)
        {
            line.Intersects(ref this, out result);
        }

        public override string ToString()
        {
            return $"{Min} < {Max}";
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingRectangle && Equals((BoundingRectangle)obj);
        }
        public bool Equals(BoundingRectangle other)
        {
            return Equals(ref this, ref other);
        }
        public static bool Equals(BoundingRectangle rect1, BoundingRectangle rect2)
        {
            return Equals(ref rect1, ref rect2);
        }        
        public static bool Equals(ref BoundingRectangle rect1, ref BoundingRectangle rect2)
        {
            return rect1.Min == rect2.Min && rect1.Max == rect2.Max;
        }
        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }
        public static bool operator ==(BoundingRectangle rect1, BoundingRectangle rect2)
        {
            return Equals(ref rect1, ref rect2);
        }
        public static bool operator !=(BoundingRectangle rect1, BoundingRectangle rect2)
        {
            return !Equals(ref rect1, ref rect2);
        }
    }
}