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
    [StructLayout(LayoutKind.Sequential, Size = LineSegment.Size)]
    public struct LineSegment : IEquatable<LineSegment>
    {
        public const Scalar Tolerance = 0.000000001f;
        public const int Size = VectorUtil.Size * 2;

        public static bool Intersects2(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            var CmP = new Vector2(C.X - A.X, C.Y - A.Y);
            var r = new Vector2(B.X - A.X, B.Y - A.Y);
            var s = new Vector2(D.X - C.X, D.Y - C.Y);

            float CmPxr = CmP.X * r.Y - CmP.Y * r.X;
            float CmPxs = CmP.X * s.Y - CmP.Y * s.X;
            float rxs = r.X * s.Y - r.Y * s.X;

            if (CmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((C.X - A.X < 0f) != (C.X - B.X < 0f))
                    || ((C.Y - A.Y < 0f) != (C.Y - B.Y < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            float rxsr = 1f / rxs;
            float t = CmPxs * rxsr;
            float u = CmPxr * rxsr;

            return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
        }

        public static bool Intersects(ref Vector2 v1, ref Vector2 v2, ref Vector2 v3, ref Vector2 v4)
        {
            var div = 1 / ((v4.Y - v3.Y) * (v2.X - v1.X) - (v4.X - v3.X) * (v2.Y - v1.Y));
            var ua = ((v4.X - v3.X) * (v1.Y - v3.Y) - (v4.Y - v3.Y) * (v1.X - v3.X)) * div;
            var ub = ((v2.X - v1.X) * (v1.Y - v3.Y) - (v2.Y - v1.Y) * (v1.X - v3.X)) * div;
            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
        }
        public static bool Intersects(ref Vector2 v1, ref Vector2 v2, ref Vector2 v3, ref Vector2 v4, out Vector2 result)
        {
            var div = 1 / ((v4.Y - v3.Y) * (v2.X - v1.X) - (v4.X - v3.X) * (v2.Y - v1.Y));
            var ua = ((v4.X - v3.X) * (v1.Y - v3.Y) - (v4.Y - v3.Y) * (v1.X - v3.X)) * div;
            var ub = ((v2.X - v1.X) * (v1.Y - v3.Y) - (v2.Y - v1.Y) * (v1.X - v3.X)) * div;
            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                Vector2.Lerp(ref v1, ref v2, ua, out result);
                return true;
            }
            result = Vector2.Zero;
            return false;
        }


        public static void Intersects(ref Vector2 vertex1, ref Vector2 vertex2, ref Ray ray, out Scalar result)
        {
            Vector2 tanget, normal;
            Scalar edgeMagnitude;
            Vector2.Subtract(ref vertex1, ref vertex2, out tanget);
            VectorUtil.Normalize(ref tanget, out edgeMagnitude, out tanget);            
            VectorUtil.Rotate90CCW(ref tanget, out normal);

            Scalar dir;
            Vector2.Dot(ref normal, ref ray.Direction, out dir);
            if (Math.Abs(dir) >= Tolerance)
            {
                Vector2 originDiff;
                Vector2.Subtract(ref ray.Origin, ref vertex2, out originDiff);
                Scalar actualDistance;
                Vector2.Dot(ref normal, ref originDiff, out actualDistance);
                Scalar distanceFromOrigin = -(actualDistance / dir);
                if (distanceFromOrigin >= 0)
                {
                    Vector2 intersectPos;
                    Vector2.Multiply(ref ray.Direction, distanceFromOrigin, out intersectPos);
                    Vector2.Add(ref intersectPos, ref originDiff, out intersectPos);

                    Scalar distanceFromSecond;
                    Vector2.Dot(ref intersectPos, ref tanget, out distanceFromSecond);

                    if (distanceFromSecond >= 0 && distanceFromSecond <= edgeMagnitude)
                    {
                        result = distanceFromOrigin;
                        return;
                    }
                }
            }
            result = -1;
        }

        public static void GetDistance(ref Vector2 vertex1, ref Vector2 vertex2, ref Vector2 point, out Scalar result)
        {
            Scalar edgeLength;
            Vector2 edge, local;

            Vector2.Subtract(ref point, ref vertex2, out local);
            Vector2.Subtract(ref vertex1, ref vertex2, out edge);
            VectorUtil.Normalize(ref edge, out edgeLength, out edge);

            Scalar nProj = local.Y * edge.X - local.X * edge.Y;
            Scalar tProj = local.X * edge.X + local.Y * edge.Y;
            if (tProj < 0)
            {
                result = Calc.Sqrt(tProj * tProj + nProj * nProj);
            }
            else if (tProj > edgeLength)
            {
                tProj -= edgeLength;
                result = Calc.Sqrt(tProj * tProj + nProj * nProj);
            }
            else
            {
                result = Math.Abs(nProj);
            }
        }
        public static void GetDistanceSq(ref Vector2 vertex1, ref Vector2 vertex2, ref Vector2 point, out Scalar result)
        {
            Scalar edgeLength;
            Vector2 edge, local;

            Vector2.Subtract(ref point, ref vertex2, out local);
            Vector2.Subtract(ref vertex1, ref vertex2, out edge);
            VectorUtil.Normalize(ref edge, out edgeLength, out edge);

            Scalar nProj = local.Y * edge.X - local.X * edge.Y;
            Scalar tProj = local.X * edge.X + local.Y * edge.Y;
            if (tProj < 0)
            {
                result = tProj * tProj + nProj * nProj;
            }
            else if (tProj > edgeLength)
            {
                tProj -= edgeLength;
                result = tProj * tProj + nProj * nProj;
            }
            else
            {
                result = nProj * nProj;
            }
        }
        
        public Vector2 Vertex1;        
        public Vector2 Vertex2;
        
        public LineSegment(Vector2 vertex1, Vector2 vertex2)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
        }

        public Scalar GetDistance(Vector2 point)
        {
            Scalar result;
            GetDistance(ref point, out result);
            return result;
        }
        public void GetDistance(ref Vector2 point, out Scalar result)
        {
            GetDistance(ref Vertex1, ref Vertex2, ref point, out result);
        }

        public Scalar Intersects(Ray ray)
        {
            Scalar result;
            Intersects(ref ray, out result);
            return result;
        }
        public void Intersects(ref Ray ray, out Scalar result)
        {
            Intersects(ref Vertex1, ref Vertex2, ref ray, out result);
        }



        public override string ToString()
        {
            return $"V1: {Vertex1} V2: {Vertex2}";
        }
        public override int GetHashCode()
        {
            return Vertex1.GetHashCode() ^ Vertex2.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is LineSegment && Equals((LineSegment)obj);
        }
        public bool Equals(LineSegment other)
        {
            return Equals(ref this, ref other);
        }
        public static bool Equals(LineSegment line1, LineSegment line2)
        {
            return Equals(ref line1, ref line2);
        }        
        public static bool Equals(ref LineSegment line1, ref LineSegment line2)
        {
            return line1.Vertex1 == line2.Vertex1 && line1.Vertex2 == line2.Vertex2;
        }

        public static bool operator ==(LineSegment line1, LineSegment line2)
        {
            return Equals(ref line1, ref line2);
        }
        public static bool operator !=(LineSegment line1, LineSegment line2)
        {
            return !Equals(ref line1, ref line2);
        }
    }
}