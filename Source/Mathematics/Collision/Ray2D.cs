using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace Penumbra.Mathematics.Collision
{
    internal struct Ray2D
    {
        public Vector2 Origin;
        public Vector2 Direction;

        public Ray2D(Vector2 origin, Vector2 direction)
            : this(ref origin, ref direction)
        {            
        }

        public Ray2D(ref Vector2 origin, ref Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector2 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        public bool Intersects(ref Ray2D ray)
        {
            float distance;
            return Intersects(ref ray, out distance);
        }

        public bool Intersects(ref Ray2D ray, out float distance)
        {
            return Collision.RayIntersectsRay(ref Origin, ref Direction, ref ray.Origin, ref ray.Direction, out distance);
        }

        public bool Intersects(ref LineSegment2D line)
        {            
            float distance;
            return Intersects(ref line, out distance);
        }

        public bool Intersects(ref LineSegment2D line, out float distance)
        {
            return Collision.RayIntersectsLineSegment(ref Origin, ref Direction, ref line.P1, ref line.P2, out distance);
        }

        // ref: http://rosettacode.org/wiki/Ray-casting_algorithm
        public bool Intersects(Polygon polygon, out float distance)
        {            
            // TODO: Test against AABB first to increase performance?

            distance = float.MaxValue;
            Vector2 pt = Origin;

            // temp holder for segment distance
            float tempDistance;
            //int crossings = 0;

            for (int j = polygon.Count - 1, i = 0; i < polygon.Count; j = i, i++)
            {
                var segment = new LineSegment2D(polygon[i], polygon[j]);                
                if (Intersects(ref segment, out tempDistance))
                {
                    distance = Math.Min(distance, tempDistance);
                    return true;
                    //crossings++;                    
                }
            }
            //return crossings > 0 && crossings % 2 == 0;
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Origin:{0} Dir:{1}", Origin, Direction);
        }
    }
}
