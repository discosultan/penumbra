using System;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Geometry
{
    enum LineSegmentIntersectionType
    {
        NoIntersection = 0,
        Intersection = 1,
        CollinearIntersection = 2
    }

    struct LineSegmentIntersection
    {
        public LineSegmentIntersectionType Type;
        public Vector2 IntersectionPoint;
        public Vector2 IntersectionEndPoint;

        public static implicit operator bool(LineSegmentIntersection intersection)
        {
            return intersection.Type != LineSegmentIntersectionType.NoIntersection;
        }
    }

    struct LineSegment2D : IEquatable<LineSegment2D>
    {
        private const float Epsilon = Calc.Epsilon;

        public Vector2 P0;
        public Vector2 P1;

        public LineSegment2D(Vector2 p0, Vector2 p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public LineSegmentIntersection Intersects(LineSegment2D segment)
        {
            var result = new LineSegmentIntersection();
            result.Type = (LineSegmentIntersectionType)Intersect(this, segment, out result.IntersectionPoint, out result.IntersectionEndPoint);
            return result;
        }

        public bool Equals(LineSegment2D other)
        {
            return P0 == other.P0 && P1 == other.P1;
        }

        // intersect2D_2Segments(): find the 2D intersection of 2 finite segments
        //    Input:  two finite segments S1 and S2
        //    Output: *I0 = intersect point (when it exists)
        //            *I1 =  endpoint of intersect segment [I0,I1] (when it exists)
        //    Return: 0=disjoint (no intersect)
        //            1=intersect  in unique point I0
        //            2=overlap  in segment from I0 to I1
        private static int Intersect(LineSegment2D S1, LineSegment2D S2, out Vector2 I0, out Vector2 I1)
        {
            I0 = Vector2.Zero;
            I1 = Vector2.Zero;

            Vector2 u = S1.P1 - S1.P0;
            Vector2 v = S2.P1 - S2.P0;
            Vector2 w = S1.P0 - S2.P0;
            float D = VectorUtil.Cross(u, v);

            // test if  they are parallel (includes either being a point)
            if (Math.Abs(D) < Epsilon)
            {           // S1 and S2 are parallel
                if (VectorUtil.Cross(u, w) != 0 || VectorUtil.Cross(v, w) != 0)
                {
                    return 0;                    // they are NOT collinear
                }
                // they are collinear or degenerate
                // check if they are degenerate  points
                float du = Vector2.Dot(u, u);
                float dv = Vector2.Dot(v, v);
                if (du == 0 && dv == 0)
                {            // both segments are points
                    if (S1.P0 != S2.P0)         // they are distinct  points
                        return 0;
                    I0 = S1.P0;                 // they are the same point
                    return 1;
                }
                if (du == 0)
                {                     // S1 is a single point
                    if (InSegment(S1.P0, S2) == 0)  // but is not in S2
                        return 0;
                    I0 = S1.P0;
                    return 1;
                }
                if (dv == 0)
                {                     // S2 a single point
                    if (InSegment(S2.P0, S1) == 0)  // but is not in S1
                        return 0;
                    I0 = S2.P0;
                    return 1;
                }
                // they are collinear segments - get  overlap (or not)
                float t0, t1;                    // endpoints of S1 in eqn for S2
                Vector2 w2 = S1.P1 - S2.P0;
                if (v.X != 0)
                {
                    t0 = w.X / v.X;
                    t1 = w2.X / v.X;
                }
                else
                {
                    t0 = w.Y / v.Y;
                    t1 = w2.Y / v.Y;
                }
                if (t0 > t1)
                {                   // must have t0 smaller than t1
                    float t = t0; t0 = t1; t1 = t;    // swap if not
                }
                if (t0 > 1 || t1 < 0)
                {
                    return 0;      // NO overlap
                }
                t0 = t0 < 0 ? 0 : t0;               // clip to min 0
                t1 = t1 > 1 ? 1 : t1;               // clip to max 1
                if (t0 == t1)
                {                  // intersect is a point
                    I0 = S2.P0 + t0 * v;
                    return 1;
                }

                // they overlap in a valid subsegment
                I0 = S2.P0 + t0 * v;
                I1 = S2.P0 + t1 * v;
                return 2;
            }

            // the segments are skew and may intersect in a point
            // get the intersect parameter for S1
            float sI = VectorUtil.Cross(v, w) / D;
            if (sI < 0 || sI > 1)                // no intersect with S1
                return 0;

            // get the intersect parameter for S2
            float tI = VectorUtil.Cross(u, w) / D;
            if (tI < 0 || tI > 1)                // no intersect with S2
                return 0;

            I0 = S1.P0 + sI * u;                // compute S1 intersect point
            return 1;
        }

        // inSegment(): determine if a point is inside a segment
        //    Input:  a point P, and a collinear segment S
        //    Return: 1 = P is inside S
        //            0 = P is  not inside S
        private static int InSegment(Vector2 P, LineSegment2D S)
        {
            if (S.P0.X != S.P1.X)
            {    // S is not  vertical
                if (S.P0.X <= P.X && P.X <= S.P1.X)
                    return 1;
                if (S.P0.X >= P.X && P.X >= S.P1.X)
                    return 1;
            }
            else
            {    // S is vertical, so test y  coordinate
                if (S.P0.Y <= P.Y && P.Y <= S.P1.Y)
                    return 1;
                if (S.P0.Y >= P.Y && P.Y >= S.P1.Y)
                    return 1;
            }
            return 0;
        }
    }
}
