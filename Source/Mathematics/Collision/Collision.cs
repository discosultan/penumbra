using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Collision
{
    internal static class Collision
    {
        // ref: farseer physics engine
        /// <summary>
        /// This method detects if two line segments (or lines) intersect,
        /// and, if so, the point of intersection. Use the <paramref name="firstIsSegment"/> and
        /// <paramref name="secondIsSegment"/> parameters to set whether the intersection point
        /// must be on the first and second line segments. Setting these
        /// both to true means you are doing a line-segment to line-segment
        /// intersection. Setting one of them to true means you are doing a
        /// line to line-segment intersection test, and so on.
        /// Note: If two line segments are coincident, then 
        /// no intersection is detected (there are actually
        /// infinite intersection points).
        /// Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="point">This is set to the intersection
        /// point if an intersection is detected.</param>
        /// <param name="firstIsSegment">Set this to true to require that the 
        /// intersection point be on the first line segment.</param>
        /// <param name="secondIsSegment">Set this to true to require that the
        /// intersection point be on the second line segment.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4,
                                         bool firstIsSegment, bool secondIsSegment,
                                         out Vector2 point)
        {
            point = new Vector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = point4.Y - point3.Y;
            float b = point2.X - point1.X;
            float c = point4.X - point3.X;
            float d = point2.Y - point1.Y;

            // denominator to solution of linear system
            float denom = (a * b) - (c * d);

            // if denominator is 0, then lines are parallel
            if (!(denom >= -Calc.Epsilon && denom <= Calc.Epsilon))
            {
                float e = point1.Y - point3.Y;
                float f = point1.X - point3.X;
                float oneOverDenom = 1.0f / denom;

                // numerator of first equation
                float ua = (c * e) - (a * f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= 0.0f && ua <= 1.0f)
                {
                    // numerator of second equation
                    float ub = (b * e) - (d * f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= 0.0f && ub <= 1.0f)
                    {
                        // check if they are coincident (no collision in this case)
                        if (ua != 0f || ub != 0f)
                        {
                            //There is an intersection
                            point.X = point1.X + ua * b;
                            point.Y = point1.Y + ua * d;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // ref: http://www.moonlight3d.eu/svn/Moonlight/trunk/mlframework/src/eu/moonlight3d/math/Ray2D.java
        public static bool RayIntersectsRay(ref Vector2 origin1, ref Vector2 direction1, ref Vector2 origin2, ref Vector2 direction2, out Vector2 intersectionPoint)
        {
            Vector2 otherDirection = direction2;

            float denominator = (-direction1.X * otherDirection.Y + direction1.Y * otherDirection.X);

            if (denominator == 0)
            {
                intersectionPoint = Vector2.Zero;
                return false; // lines are parallel - no intersection possible
            }

            float tau = (-direction1.X * origin1.Y + direction1.X * origin2.Y + direction1.Y * origin1.X - direction1.Y * origin2.X) / denominator;
            float lambda = -(-origin1.X * otherDirection.Y + origin2.X * otherDirection.Y + otherDirection.X * origin1.Y - otherDirection.X * origin2.Y) / denominator;

            if (!(0 <= tau) || !(0 <= lambda))
            {
                // calculated intersection point is outside of at least one ray - so no intersection
                intersectionPoint = Vector2.Zero;
                return false;
            }

            intersectionPoint = origin1 + direction1 * lambda;
            return true;
        }

        // ref: http://www.moonlight3d.eu/svn/Moonlight/trunk/mlframework/src/eu/moonlight3d/math/Ray2D.java
        public static bool RayIntersectsLineSegment(ref Vector2 origin, ref Vector2 direction, ref Vector2 p1, ref Vector2 p2, out float distance)
        {
            Vector2 otherDirection = p2 - p1;

            float denominator = (-direction.X * otherDirection.Y + direction.Y * otherDirection.X);

            if (denominator == 0)
            {
                distance = 0f;
                return false; // lines are parallel - no intersection possible
            }

            float tau = (-direction.X * origin.Y + direction.X * p1.Y + direction.Y * origin.X - direction.Y * p1.X) / denominator;
            float lambda = -(-origin.X * otherDirection.Y + p1.X * otherDirection.Y + otherDirection.X * origin.Y - otherDirection.X * p1.Y) / denominator;

            if (!(0 <= tau && tau <= 1) || !(0 <= lambda))
            {
                // calculated intersection point is outside of at least one line - so no intersection
                distance = 0f;
                return false;
            }

            distance = lambda;
            return true;
        }
    }
}
