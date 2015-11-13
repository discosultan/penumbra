using System;
using Microsoft.Xna.Framework;
using Penumbra.Utilities;

namespace Penumbra.Geometry
{
    internal struct LineSegment
    {
        private const float Epsilon = 1e-5f;

        public Vector2 PointA;
        public Vector2 PointB;

        public LineSegment(Vector2 pointA, Vector2 pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }

        public bool Intersects(LineSegment segment)
        {
            Vector2 intersectionPoint;
            Vector2 intersectionEndPoint;
            int result = Intersect(this, segment, out intersectionPoint, out intersectionEndPoint);
            return result != 0;
        }

        public override string ToString()
        {
            return $"{nameof(PointA)}:{PointA} {nameof(PointB)}:{PointB}";
        }

        // intersect2D_2Segments(): find the 2D intersection of 2 finite segments
        //    Input:  two finite segments S1 and S2
        //    Output: *I0 = intersect point (when it exists)
        //            *I1 =  endpoint of intersect segment [I0,I1] (when it exists)
        //    Return: 0=disjoint (no intersect)
        //            1=intersect  in unique point I0
        //            2=overlap  in segment from I0 to I1
        // Ref: http://geomalgorithms.com/a05-_intersect-1.html
        private static int Intersect(LineSegment S1, LineSegment S2, out Vector2 I0, out Vector2 I1)
        {
            I0 = Vector2.Zero;
            I1 = Vector2.Zero;

            Vector2 u = S1.PointB - S1.PointA;
            Vector2 v = S2.PointB - S2.PointA;
            Vector2 w = S1.PointA - S2.PointA;
            float D = Calculate.Cross(ref u, ref v);

            // test if  they are parallel (includes either being a point)
            if (Math.Abs(D) < Epsilon)
            {
                // S1 and S2 are parallel
                if (Calculate.Cross(ref u, ref w) != 0 || Calculate.Cross(ref v, ref w) != 0)
                {
                    return 0; // they are NOT collinear
                }
                // they are collinear or degenerate
                // check if they are degenerate  points
                float du = Vector2.Dot(u, u);
                float dv = Vector2.Dot(v, v);
                if (du == 0 && dv == 0)
                {
                    // both segments are points
                    if (S1.PointA != S2.PointA) // they are distinct  points
                        return 0;
                    I0 = S1.PointA; // they are the same point
                    return 1;
                }
                if (du == 0)
                {
                    // S1 is a single point
                    if (InSegment(S1.PointA, S2) == 0) // but is not in S2
                        return 0;
                    I0 = S1.PointA;
                    return 1;
                }
                if (dv == 0)
                {
                    // S2 a single point
                    if (InSegment(S2.PointA, S1) == 0) // but is not in S1
                        return 0;
                    I0 = S2.PointA;
                    return 1;
                }
                // they are collinear segments - get  overlap (or not)
                float t0, t1; // endpoints of S1 in eqn for S2
                Vector2 w2 = S1.PointB - S2.PointA;
                if (v.X != 0)
                {
                    t0 = w.X/v.X;
                    t1 = w2.X/v.X;
                }
                else
                {
                    t0 = w.Y/v.Y;
                    t1 = w2.Y/v.Y;
                }
                if (t0 > t1)
                {
                    // must have t0 smaller than t1
                    float t = t0;
                    t0 = t1;
                    t1 = t; // swap if not
                }
                if (t0 > 1 || t1 < 0)
                {
                    return 0; // NO overlap
                }
                t0 = t0 < 0 ? 0 : t0; // clip to min 0
                t1 = t1 > 1 ? 1 : t1; // clip to max 1
                if (t0 == t1)
                {
                    // intersect is a point
                    I0 = S2.PointA + t0*v;
                    return 1;
                }

                // they overlap in a valid subsegment
                I0 = S2.PointA + t0*v;
                I1 = S2.PointA + t1*v;
                return 2;
            }

            // the segments are skew and may intersect in a point
            // get the intersect parameter for S1
            float sI = Calculate.Cross(ref v, ref w)/D;
            if (sI < 0 || sI > 1) // no intersect with S1
                return 0;

            // get the intersect parameter for S2
            float tI = Calculate.Cross(ref u, ref w)/D;
            if (tI < 0 || tI > 1) // no intersect with S2
                return 0;

            I0 = S1.PointA + sI*u; // compute S1 intersect point
            return 1;
        }

        // inSegment(): determine if a point is inside a segment
        //    Input:  a point P, and a collinear segment S
        //    Return: 1 = P is inside S
        //            0 = P is  not inside S
        // Ref: http://geomalgorithms.com/a05-_intersect-1.html
        private static int InSegment(Vector2 P, LineSegment S)
        {
            if (S.PointA.X != S.PointB.X)
            {
                // S is not  vertical
                if (S.PointA.X <= P.X && P.X <= S.PointB.X)
                    return 1;
                if (S.PointA.X >= P.X && P.X >= S.PointB.X)
                    return 1;
            }
            else
            {
                // S is vertical, so test y coordinate
                if (S.PointA.Y <= P.Y && P.Y <= S.PointB.Y)
                    return 1;
                if (S.PointA.Y >= P.Y && P.Y >= S.PointB.Y)
                    return 1;
            }
            return 0;
        }
    }
}
