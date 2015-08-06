using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Collision
{
    //internal struct LineSegmentIntersection
    //{
    //    public enum IntersectionResult
    //    {
    //        Disjoint,
    //        Intersect,
    //        Overlap           
    //    }

    //    public IntersectionResult Result;
    //    public Vector2 IntersectionPoint;
    //    public Vector2 IntersectionEndPoint;
    //}

    internal struct LineSegment2D
    {
        public Vector2 P1;
        public Vector2 P2;

        public LineSegment2D(Vector2 p1, Vector2 p2)
            : this(ref p1, ref p2)
        {            
        }

        public LineSegment2D(ref Vector2 p1, ref Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public bool Intersects(ref Line2D line)
        {
            Vector2 intersectionPoint;
            return Intersects(ref line, out intersectionPoint);
        }

        public bool Intersects(ref Line2D line, out Vector2 intersectionPoint)
        {
            return Collision.LineIntersect(ref P1, ref P2, ref line.P1, ref line.P2, true, false, out intersectionPoint);
        }

        //public bool Intersects(ref LineSegment2D lineSegment)
        //{
        //    Vector2 intersectionPoint;
        //    return Intersects(ref lineSegment, out intersectionPoint);
        //}

        //public bool Intersects(ref LineSegment2D lineSegment, out Vector2 intersectionPoint)
        //{
        //    //return Collision.LineIntersect2(ref P1, ref P2, ref lineSegment.P1, ref lineSegment.P2, true, true, out intersectionPoint);
        //    return Collision.LineIntersect2(ref P1, ref P2, ref lineSegment.P1, ref lineSegment.P2, out intersectionPoint);
        //}

        // ref: http://stackoverflow.com/a/565282/1466456
        //public LineIntersectionResult Intersects(ref LineSegment2D lineSegment, out Vector2 intersectionPoint)
        //{
        //    Vector2 p = P1;
        //    Vector2 r = P2 - P1;
        //    Vector2 q = lineSegment.P1;
        //    Vector2 s = lineSegment.P2 - lineSegment.P1;

        //    float rsX = VectorUtil.Cross(r, s);
        //    float pqrX = VectorUtil.Cross(q - p, r);

        //    if (Calc.NearZero(rsX) && Calc.NearZero(pqrX))
        //    {
        //        float rrDot = Vector2.Dot(r, r);
        //        float t0 = Vector2.Dot(q - p, r)/rrDot;
        //        //float t1 = Vector2.Dot(q + s - p, r)/rrDot;
        //        float srDot = Vector2.Dot(s, r);
        //        float t1 = t0 + srDot/rrDot;

        //        float t1mt0 = t1 - t0;

        //        if (srDot >= 0 && t1mt0 <= 1 && t1mt0 >= 0)
        //        {
        //            intersectionPoint = 
        //            return LineIntersectionResult.Collinear;
        //        }
        //    }

        //        float u = pqrX/rsX;
                                
        //}

        //public void Intersects(ref LineSegment2D segment, out LineSegmentIntersection result)
        //{
        //    result = new LineSegmentIntersection();
        //    Vector2 u = P2 - P1;
        //    Vector2 v = segment.P2 - segment.P1;
        //    Vector2 w = P1 - segment.P1;            
        //    float D = VectorUtil.Cross(u, v);

        //    // test if  they are parallel (includes either being a point)
        //    if (Math.Abs(D) < Calc.Epsilon)
        //    {           // S1 and S2 are parallel
        //        if (VectorUtil.Cross(u, w) != 0 || VectorUtil.Cross(v, w) != 0)
        //        {
        //            // they are NOT collinear
        //            result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //            return;
        //        }
        //        // they are collinear or degenerate
        //        // check if they are degenerate  points
        //        float du = Vector2.Dot(u, u);
        //        float dv = Vector2.Dot(v, v);
        //        if (du == 0 && dv == 0)
        //        {            // both segments are points
        //            if (P1 != segment.P1) // they are distinct  points
        //            {
        //                result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //                return;
        //            }                        
        //            result.IntersectionPoint = P1;                 // they are the same point
        //            result.Result = LineSegmentIntersection.IntersectionResult.Intersect;
        //            return;
        //        }
        //        if (du == 0)
        //        {                     // S1 is a single point
        //            if (!segment.inSegment(P1)) // but is not in S2
        //            {
        //                result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //                return;
        //            }                        
        //            result.IntersectionPoint = P1;
        //            result.Result = LineSegmentIntersection.IntersectionResult.Intersect;
        //            return;
        //        }
        //        if (dv == 0)
        //        {                     // S2 a single point
        //            if (!inSegment(segment.P1)) // but is not in S1
        //            {
        //                result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //                return;
        //            }                       
        //            result.IntersectionPoint = segment.P1;
        //            result.Result = LineSegmentIntersection.IntersectionResult.Intersect;
        //            return;
        //        }
        //        // they are collinear segments - get  overlap (or not)
        //        float t0, t1;                    // endpoints of S1 in eqn for S2
        //        Vector2 w2 = P2 - segment.P1;
        //        if (v.X != 0)
        //        {
        //            t0 = w.X / v.X;
        //            t1 = w2.X / v.X;
        //        }
        //        else
        //        {
        //            t0 = w.Y / v.Y;
        //            t1 = w2.Y / v.Y;
        //        }
        //        if (t0 > t1)
        //        {                   // must have t0 smaller than t1
        //            float t = t0; t0 = t1; t1 = t;    // swap if not
        //        }
        //        if (t0 > 1 || t1 < 0)
        //        {
        //            result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //            return; // NO overlap                    
        //        }
        //        t0 = t0 < 0 ? 0 : t0;               // clip to min 0
        //        t1 = t1 > 1 ? 1 : t1;               // clip to max 1
        //        if (t0 == t1)
        //        {                  // intersect is a point
        //            result.IntersectionPoint = segment.P1 + t0 * v;
        //            result.Result = LineSegmentIntersection.IntersectionResult.Intersect;
        //            return;
        //        }

        //        // they overlap in a valid subsegment
        //        result.IntersectionPoint = segment.P1 + t0 * v;
        //        result.IntersectionEndPoint = segment.P1 + t1 * v;
        //        result.Result = LineSegmentIntersection.IntersectionResult.Overlap;
        //        return;
        //    }

        //    // the segments are skew and may intersect in a point
        //    // get the intersect parameter for S1
        //    float sI = VectorUtil.Cross(v, w) / D;
        //    if (sI < 0 || sI > 1) // no intersect with S1
        //    {
        //        result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //        return;
        //    }                

        //    // get the intersect parameter for S2
        //    float tI = VectorUtil.Cross(u, w) / D;
        //    if (tI < 0 || tI > 1) // no intersect with S2
        //    {
        //        result.Result = LineSegmentIntersection.IntersectionResult.Disjoint;
        //        return;
        //    }                

        //    result.IntersectionPoint = P1 + sI * u;                // compute S1 intersect point
        //    result.Result = LineSegmentIntersection.IntersectionResult.Intersect;            
        //}

        public bool Intersects(ref Ray2D ray)
        {
            Vector2 intersectionPoint;
            return Intersects(ref ray, out intersectionPoint);
        }

        public bool Intersects(ref Ray2D ray, out Vector2 intersectionPoint)
        {
            float distance;
            bool intersects = Collision.RayIntersectsLineSegment(ref ray.Origin, ref ray.Direction, ref P1, ref P2, out distance);
            intersectionPoint = ray.GetPoint(distance);
            return intersects;
        }

        //public bool Intersects(Polygon polygon, out Vector2 intersectionPoint)
        //{

        //}

        // inSegment(): determine if a point is inside a segment
        //    Input:  a point P, and a collinear segment S
        //    Return: 1 = P is inside S
        //            0 = P is  not inside S        
        public bool inSegment(Vector2 p)
        {
            if (P1.X != P2.X)
            {    // S is not  vertical
                if (P1.X <= p.X && p.X <= P2.X)
                    return true;
                if (P1.X >= p.X && p.X >= P2.X)
                    return true;
            }
            else
            {    // S is vertical, so test y  coordinate
                if (P1.Y <= p.Y && p.Y <= P2.Y)
                    return true;
                if (P1.Y >= p.Y && p.Y >= P2.Y)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"P1:{P1} P2:{P2}";
        }
    }
}
