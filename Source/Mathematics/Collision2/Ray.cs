
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
    [StructLayout(LayoutKind.Sequential, Size = Ray.Size)]
    public struct Ray : IEquatable<Ray>
    {
        public const int Size = VectorUtil.Size * 2;
        
        public Vector2 Origin;        
        public Vector2 Direction;
        
        public Ray(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Scalar Intersects(BoundingRectangle rect)
        {
            Scalar result;
            rect.Intersects(ref this, out result);
            return result;
        }
        public Scalar Intersects(Line line)
        {
            Scalar result;
            line.Intersects(ref this, out result);
            return result;
        }
        public Scalar Intersects(LineSegment line)
        {
            Scalar result;
            line.Intersects(ref this, out result);
            return result;
        }
        public Scalar Intersects(BoundingCircle circle)
        {
            Scalar result;
            circle.Intersects(ref this, out result);
            return result;
        }
        public Scalar Intersects(BoundingPolygon polygon)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            Scalar result;
            polygon.Intersects(ref this, out result);
            return result;
        }

        public void Intersects(ref BoundingRectangle rect, out Scalar result)
        {
            rect.Intersects(ref this, out result);
        }
        public void Intersects(ref Line line, out Scalar result)
        {
            line.Intersects(ref this, out result);
        }
        public void Intersects(ref LineSegment line, out Scalar result)
        {
            line.Intersects(ref this, out result);
        }
        public void Intersects(ref BoundingCircle circle, out Scalar result)
        {
            circle.Intersects(ref this, out result);
        }
        public void Intersects(ref BoundingPolygon polygon, out Scalar result)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            polygon.Intersects(ref this, out result);
        }

        public override string ToString()
        {
            return $"O: {Origin} D: {Direction}";
        }
        public override int GetHashCode()
        {
            return Origin.GetHashCode() ^ Direction.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is Ray && Equals((Ray)obj);
        }
        public bool Equals(Ray other)
        {
            return Equals(ref this, ref other);
        }
        public static bool Equals(Ray ray1, Ray ray2)
        {
            return Equals(ref ray1, ref ray2);
        }        
        public static bool Equals(ref Ray ray1, ref Ray ray2)
        {
            return ray1.Origin == ray2.Origin && ray1.Direction == ray2.Direction;
        }

        public static bool operator ==(Ray ray1, Ray ray2)
        {
            return Equals(ref ray1, ref ray2);
        }
        public static bool operator !=(Ray ray1, Ray ray2)
        {
            return !Equals(ref ray1, ref ray2);
        }
    }
}