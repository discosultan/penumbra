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

    [StructLayout(LayoutKind.Sequential, Size = BoundingCircle.Size)]
    public struct BoundingCircle : IEquatable<BoundingCircle>
    {
        public const int Size = VectorUtil.Size + sizeof(Scalar);

        public static BoundingCircle FromRectangle(BoundingRectangle rect)
        {
            BoundingCircle result;
            FromRectangle(ref rect, out result);
            return result;
        }
        public static void FromRectangle(ref BoundingRectangle rect, out BoundingCircle result)
        {
            result.Position.X = (rect.Min.X + rect.Max.X) * .5f;
            result.Position.Y = (rect.Min.Y + rect.Max.Y) * .5f;
            Scalar xRadius = (rect.Max.X - rect.Min.X) * .5f;
            Scalar yRadius = (rect.Max.Y - rect.Min.Y) * .5f;
            result.Radius = Calc.Sqrt(xRadius * xRadius + yRadius * yRadius);
        }
        public static BoundingCircle FromVectors(Vector2[] vertexes)
        {
            BoundingCircle result;
            FromVectors(vertexes, out result);
            return result;
        }
        public static void FromVectors(Vector2[] vertexes, out BoundingCircle result)
        {
            BoundingPolygon.GetCentroid(vertexes, out result.Position);
            result.Radius = -1;
            for (int index = 0; index < vertexes.Length; ++index)
            {
                Scalar distSq;
                Vector2.DistanceSquared(ref result.Position, ref vertexes[index], out distSq);
                if (result.Radius == -1 || (distSq < result.Radius))
                {
                    result.Radius = distSq;
                }
            }
            result.Radius = Calc.Sqrt(result.Radius);
        }


        public Vector2 Position;

        public Scalar Radius;

        public BoundingCircle(Vector2 position, Scalar radius)
        {
            this.Position = position;
            this.Radius = radius;
        }
        public BoundingCircle(Scalar x, Scalar y, Scalar radius)
        {
            this.Position.X = x;
            this.Position.Y = y;
            this.Radius = radius;
        }

        public Scalar Area => MathHelper.Pi * Radius * Radius;

        public Scalar Perimeter => MathHelper.TwoPi * Radius;

        public Scalar GetDistance(Vector2 point)
        {
            Scalar result;
            GetDistance(ref point, out result);
            return result;
        }
        public void GetDistance(ref Vector2 point, out Scalar result)
        {
            Vector2 diff;
            Vector2.Subtract(ref point, ref Position, out diff);
            result = diff.Length();
            result -= Radius;
        }

        public ContainmentType Contains(Vector2 point)
        {
            Scalar distance;
            GetDistance(ref point, out distance);
            return ((distance <= 0) ? (ContainmentType.Contains) : (ContainmentType.Disjoint));
        }
        public void Contains(ref Vector2 point, out ContainmentType result)
        {
            Scalar distance;
            GetDistance(ref point, out distance);
            result = ((distance <= 0) ? (ContainmentType.Contains) : (ContainmentType.Disjoint));
        }

        public ContainmentType Contains(BoundingCircle circle)
        {
            Scalar distance;
            GetDistance(ref circle.Position, out distance);
            if (-distance >= circle.Radius)
            {
                return ContainmentType.Contains;
            }
            else if (distance > circle.Radius)
            {
                return ContainmentType.Disjoint;
            }
            else
            {
                return ContainmentType.Intersects;
            }
        }
        public void Contains(ref BoundingCircle circle, out ContainmentType result)
        {
            Scalar distance;
            GetDistance(ref circle.Position, out distance);
            if (-distance >= circle.Radius)
            {
                result = ContainmentType.Contains;
            }
            else if (distance > circle.Radius)
            {
                result = ContainmentType.Disjoint;
            }
            else
            {
                result = ContainmentType.Intersects;
            }
        }

        public ContainmentType Contains(BoundingRectangle rect)
        {
            ContainmentType result;
            Contains(ref rect, out result);
            return result;
        }
        public void Contains(ref BoundingRectangle rect, out ContainmentType result)
        {
            Vector2 maxDistance,minDistance;
            Calc.Sort(rect.Max.X - Position.X, Position.X - rect.Min.X, out maxDistance.X,out minDistance.X);
            Calc.Sort(rect.Max.Y - Position.Y, Position.Y - rect.Min.Y, out maxDistance.Y,out minDistance.Y);            
            var mag = maxDistance.Length();
            if (mag <= Radius)
            {
                result = ContainmentType.Contains;
            }
            else
            {
                mag = minDistance.Length();
                result = mag <= Radius ? ContainmentType.Intersects : ContainmentType.Disjoint;
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
            Intersects(ref ray, true, out result);
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
            Intersects(ref circle, out result);
            return result;
        }
        public bool Intersects(BoundingPolygon polygon)
        {
            bool result;
            polygon.Intersects(ref this, out result);
            return result;
        }
        public bool Intersects(LineSegment line)
        {
            bool result;
            Intersects(ref line, out result);
            return result;
        }
        public bool Intersects(Line line)
        {
            bool result;
            Intersects(ref line, out result);
            return result;
        }

        public void Intersects(ref Ray ray, out Scalar result)
        {
            Intersects(ref ray, true, out result);
        }
        public void Intersects(ref Ray ray, bool discardInside, out Scalar result)
        {
            Vector2 rayOriginRelativeToCircle2D;
            Vector2.Subtract(ref ray.Origin, ref Position, out rayOriginRelativeToCircle2D);
            Scalar radiusSq = this.Radius * this.Radius;
            Scalar MagSq = rayOriginRelativeToCircle2D.LengthSquared();

            if ((MagSq <= radiusSq) && !discardInside)
            {
                result = 0;
                return;
            }
            Scalar a = ray.Direction.LengthSquared();            
            Scalar b = Vector2.Dot(rayOriginRelativeToCircle2D, ray.Direction) * 2;
            Scalar c = MagSq - radiusSq;
            Scalar minus;
            Scalar plus;
            if (Calc.TrySolveQuadratic(a, b, c, out plus, out minus))
            {
                if (minus < 0)
                {
                    if (plus > 0)
                    {
                        result = plus;
                        return;
                    }
                }
                else
                {
                    result = minus;
                    return;
                }
            }
            result = -1;
        }
        public void Intersects(ref LineSegment line, out bool result)
        {
            Scalar distance;
            line.GetDistance(ref Position, out distance);
            result = Math.Abs(distance) <= Radius;
        }
        public void Intersects(ref Line line, out bool result)
        {
            Scalar distance;
            Vector2.Dot(ref line.Normal, ref Position, out distance);
            result = (distance + line.D) <= Radius;
        }
        public void Intersects(ref BoundingRectangle rect, out bool result)
        {
            Vector2 proj;
            Vector2.Clamp(ref Position,ref rect.Min,ref rect.Max, out proj);
            Scalar distSq;
            Vector2.DistanceSquared(ref Position, ref proj, out distSq);
            result = distSq <= Radius * Radius;
        }
        public void Intersects(ref BoundingCircle circle, out bool result)
        {
            Scalar distSq;
            Vector2.DistanceSquared(ref Position, ref circle.Position, out distSq);
            result = distSq <= (Radius * Radius + circle.Radius * circle.Radius);
        }
        public void Intersects(ref BoundingPolygon polygon, out bool result)
        {
            polygon.Intersects(ref this, out result);
        }



        public override string ToString()
        {
            return $"P: {Position} R: {Radius}";
        }
        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Radius.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is BoundingCircle && Equals((BoundingCircle)obj);
        }
        public bool Equals(BoundingCircle other)
        {
            return Equals(ref this, ref other);
        }
        public static bool Equals(BoundingCircle circle1, BoundingCircle circle2)
        {
            return Equals(ref circle1, ref circle2);
        }        
        public static bool Equals(ref BoundingCircle circle1, ref BoundingCircle circle2)
        {
            return circle1.Position == circle2.Position && circle1.Radius == circle2.Radius;
        }
        public static bool operator ==(BoundingCircle circle1, BoundingCircle circle2)
        {
            return Equals(ref circle1, ref circle2);
        }
        public static bool operator !=(BoundingCircle circle1, BoundingCircle circle2)
        {
            return !Equals(ref circle1, ref circle2);
        }
    }
}