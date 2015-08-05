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
    [StructLayout(LayoutKind.Sequential, Size = Line.Size)]
    public struct Line : IEquatable<Line>
    {
        public static Line Transform(Matrix3x3 matrix, Line line)
        {
            Line result;
            Transform(ref  matrix, ref line, out result);
            return result;
        }
        public static void Transform(ref Matrix3x3 matrix, ref Line line, out Line result)
        {
            Vector2 point;
            Vector2 origin = Vector2.Zero;
            VectorUtil.Multiply(ref line.Normal, ref line.D, out  point);
            VectorUtil.Transform(ref matrix, ref point, out point);
            VectorUtil.Transform(ref matrix, ref origin, out origin);
            Vector2.Subtract(ref point, ref origin, out result.Normal);
            Vector2.Normalize(ref result.Normal, out result.Normal);
            Vector2.Dot(ref point, ref result.Normal, out result.D);
        }
        public static Line Transform(Matrix2x3 matrix, Line line)
        {
            Line result;
            Transform(ref  matrix, ref line, out result);
            return result;
        }
        public static void Transform(ref Matrix2x3 matrix, ref Line line, out Line result)
        {
            Vector2 point;
            Vector2 origin = Vector2.Zero;
            VectorUtil.Multiply(ref line.Normal, ref line.D, out  point);
            VectorUtil.Transform(ref matrix, ref point, out point);
            VectorUtil.Transform(ref matrix, ref origin, out origin);
            Vector2.Subtract(ref point, ref origin, out result.Normal);
            Vector2.Normalize(ref result.Normal, out result.Normal);
            Vector2.Dot(ref point, ref result.Normal, out result.D);
        }


        public const int Size = sizeof(Scalar) + VectorUtil.Size;        
        public Vector2 Normal;        
        public Scalar D;        
        public Line(Vector2 normal, Scalar d)
        {
            Normal = normal;
            D = d;
        }
        public Line(Scalar nX, Scalar nY, Scalar d)
        {
            this.Normal.X = nX;
            this.Normal.Y = nY;
            this.D = d;
        }
        public Line(Vector2 point1, Vector2 point2)
        {
            Scalar x = point1.X - point2.X;
            Scalar y = point1.Y - point2.Y;
            Scalar magInv = 1 / Calc.Sqrt(x * x + y * y);
            this.Normal.X = -y * magInv;
            this.Normal.Y = x * magInv;
            this.D = point1.X * this.Normal.X + point1.Y * this.Normal.Y;
        }

        public Scalar GetDistance(Vector2 point)
        {
            Scalar result;
            GetDistance(ref point, out result);
            return result;
        }
        public void GetDistance(ref Vector2 point, out Scalar result)
        {
            Vector2.Dot(ref point, ref Normal, out result);
            result -= D;
        }

        public Scalar Intersects(Ray ray)
        {
            Scalar result;
            Intersects(ref ray, out result);
            return result;
        }
        public bool Intersects(BoundingRectangle rect)
        {
            bool result;
            Intersects(ref rect, out result);
            return result;
        }
        public bool Intersects(BoundingCircle circle)
        {
            bool result;
            circle.Intersects(ref this, out result);
            return result;
        }
        public bool Intersects(BoundingPolygon polygon)
        {
            bool result;
            Intersects(ref polygon, out result);
            return result;
        }

        public void Intersects(ref Ray ray, out Scalar result)
        {
            Scalar dir;
            Vector2.Dot(ref Normal, ref ray.Direction, out dir);
            if (-dir > 0)
            {
                Scalar distanceFromOrigin;
                Vector2.Dot(ref Normal, ref ray.Origin, out distanceFromOrigin);
                distanceFromOrigin = -((distanceFromOrigin + D) / dir);
                if (distanceFromOrigin >= 0)
                {
                    result = distanceFromOrigin;
                    return;
                }
            }
            result = -1;
        }
        public void Intersects(ref BoundingRectangle box, out bool result)
        {
            Vector2[] vertexes = box.Corners();
            Scalar distance;
            GetDistance(ref  vertexes[0], out distance);

            int sign = Math.Sign(distance);
            result = false;
            for (int index = 1; index < vertexes.Length; ++index)
            {
                GetDistance(ref  vertexes[index], out distance);

                if (Math.Sign(distance) != sign)
                {
                    result = true;
                    break;
                }
            }
        }
        public void Intersects(ref BoundingCircle circle, out bool result)
        {
            circle.Intersects(ref this, out result);
        }
        public void Intersects(ref BoundingPolygon polygon, out bool result)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            Vector2[] vertexes = polygon.Vertexes;
            Scalar distance;
            GetDistance(ref  vertexes[0], out distance);

            int sign = Math.Sign(distance);
            result = false;
            for (int index = 1; index < vertexes.Length; ++index)
            {
                GetDistance(ref  vertexes[index], out distance);

                if (Math.Sign(distance) != sign)
                {
                    result = true;
                    break;
                }
            }
        }


        public override string ToString()
        {
            return $"N: {Normal} D: {D}";
        }
        public override int GetHashCode()
        {
            return Normal.GetHashCode() ^ D.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is Line && Equals((Line)obj);
        }
        public bool Equals(Line other)
        {
            return Equals(ref this, ref other);
        }
        public static bool Equals(Line line1, Line line2)
        {
            return Equals(ref line1, ref line2);
        }        
        public static bool Equals(ref Line line1, ref Line line2)
        {
            return line1.Normal == line2.Normal && line1.D == line2.D;
        }

        public static bool operator ==(Line line1, Line line2)
        {
            return Equals(ref line1, ref line2);
        }
        public static bool operator !=(Line line1, Line line2)
        {
            return !Equals(ref line1, ref line2);
        }
    }
}
