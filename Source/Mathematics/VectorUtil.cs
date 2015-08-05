using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics
{
    internal static class VectorUtil
    {
        public const int Size = 8;

        // NB! We are using inverted y axis (y runs from top to bottom).

        public static Vector2 Rotate90CW(Vector2 v)
        {
            Vector2 result;
            Rotate90CW(ref v, out result);
            return result;
        }

        public static void Rotate90CW(ref Vector2 v, out Vector2 result)
        {
            float sourceX = v.X;
            result.X = v.Y;
            result.Y = -sourceX;
        }

        public static Vector2 Rotate90CCW(Vector2 v)
        {                        
            Vector2 result;
            Rotate90CCW(ref v, out result);
            return result;
        }

        public static void Rotate90CCW(ref Vector2 v, out Vector2 result)
        {
            float sourceX = v.X;
            result.X = -v.Y;
            result.Y = sourceX;
        }

        // Assumes a polygon where no two edges intersect.
        public static Vector2 CalculateCentroid(this Vector2[] points)
        {
            float area = 0.0f;
            float cx = 0.0f;
            float cy = 0.0f;

            for (int i = 0; i < points.Length; i++)
            {
                var k = (i + 1) % (points.Length);
                var tmp = points[i].X * points[k].Y -
                            points[k].X * points[i].Y;
                area += tmp;
                cx += (points[i].X + points[k].X) * tmp;
                cy += (points[i].Y + points[k].Y) * tmp;
            }
            area *= 0.5f;
            cx *= 1.0f / (6.0f * area);
            cy *= 1.0f / (6.0f * area);

            return new Vector2(cx, cy);
        }

        //public static Vector2 Project(Vector2 v, Vector2 onto)
        //{
        //    return onto * (Vector2.Dot(onto, v) / onto.LengthSquared());
        //}

        //public static float ProjectLength(Vector2 v, Vector2 onto)
        //{
        //    return Vector2.Dot(onto, v) / onto.Length();
        //}

        public static Vector2 Rotate(Vector2 v, float angle)
        {
            float num = Calc.Cos(angle); 
            float num2 = Calc.Sin(angle);
            return new Vector2(v.X * num + v.Y * num2, -v.X * num2 + v.Y * num);
        }

        //TODO: rename
        public static bool Intersects(Vector2 dirMiddle, Vector2 dirTest, Vector2 dirTestAgainst)
        {
            float dot1 = Vector2.Dot(dirMiddle, dirTest);
            float dot2 = Vector2.Dot(dirMiddle, dirTestAgainst);
            return dot1 < dot2;
        }

        // ref: http://stackoverflow.com/a/6075960/1466456
        public static void Barycentric(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c, out Vector3 baryCoords)
        {
            float abcArea = Area(ref a, ref b, ref c);

            float u = Area(ref p, ref b, ref c) / abcArea;
            float v = Area(ref a, ref p, ref c) / abcArea;
            //float w = Area(a, b, p) / abcArea;
            float w = 1 - u - v;

            baryCoords = new Vector3(u, v, w);
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
        {
            c = a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>Positive number if point is left, negative if point is right, 
        /// and 0 if points are collinear.</returns>
        public static float Area(Vector2 a, Vector2 b, Vector2 c)
        {
            return Area(ref a, ref b, ref c);
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>Positive number if point is left, negative if point is right, 
        /// and 0 if points are collinear.</returns>
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
        }

        /// <summary>
        /// Determines if three vertices are collinear (ie. on a straight line)
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <param name="c">Third vertex</param>
        /// <returns></returns>
        public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return Collinear(ref a, ref b, ref c, 0);
        }

        public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance)
        {
            return Calc.FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        }

        public static bool NearEqual(Vector2 lhv, Vector2 rhv)
        {
            return Calc.NearEqual(lhv.X, rhv.X) && Calc.NearEqual(lhv.Y, rhv.Y);
        }

        public static void Normalize(ref Vector2 source, out float magnitude, out Vector2 result)
        {
            magnitude = source.Length();            
            if (magnitude > 0)
            {
                float magnitudeInv = (1 / magnitude);
                result.X = source.X * magnitudeInv;
                result.Y = source.Y * magnitudeInv;
            }
            else
            {
                result = Vector2.Zero;
            }
        }

        /// <summary>
        /// Uses a matrix multiplication to Transform the vector.
        /// </summary>
        /// <param name="matrix">The Transformation matrix</param>
        /// <param name="source">The Vector to be transformed</param>
        /// <returns>The transformed vector.</returns>
        /// <remarks><seealso href="http://en.wikipedia.org/wiki/Transformation_matrix#Affine_transformations"/></remarks>
        public static Vector2 Transform(Matrix3x3 matrix, Vector2 source)
        {
            float inverseZ = 1 / (source.X * matrix.m20 + source.Y * matrix.m21 + matrix.m22);
            Vector2 result;
            result.X = (source.X * matrix.m00 + source.Y * matrix.m01 + matrix.m02) * inverseZ;
            result.Y = (source.X * matrix.m10 + source.Y * matrix.m11 + matrix.m12) * inverseZ;
            return result;
        }
        public static void Transform(ref Matrix3x3 matrix, ref Vector2 source, out Vector2 result)
        {
            float X = source.X;
            float inverseZ = 1 / (X * matrix.m20 + source.Y * matrix.m21 + matrix.m22);
            result.X = (X * matrix.m00 + source.Y * matrix.m01 + matrix.m02) * inverseZ;
            result.Y = (X * matrix.m10 + source.Y * matrix.m11 + matrix.m12) * inverseZ;
        }
        public static Vector2 TransformNormal(Matrix3x3 matrix, Vector2 source)
        {
            Vector2 result;
            result.X = (source.X * matrix.m00 + source.Y * matrix.m01);
            result.Y = (source.X * matrix.m10 + source.Y * matrix.m11);
            return result;
        }
        public static void TransformNormal(ref Matrix3x3 matrix, ref Vector2 source, out Vector2 result)
        {
            float X = source.X;
            result.X = (X * matrix.m00 + source.Y * matrix.m01);
            result.Y = (X * matrix.m10 + source.Y * matrix.m11);
        }
        /// <summary>
        /// Uses a matrix multiplication to Transform the vector.
        /// </summary>
        /// <param name="matrix">The Transformation matrix</param>
        /// <param name="source">The Vector to be transformed</param>
        /// <returns>The transformed vector.</returns>
        /// <remarks><seealso href="http://en.wikipedia.org/wiki/Transformation_matrix#Affine_transformations"/></remarks>
        public static Vector2 Transform(Matrix2x3 matrix, Vector2 source)
        {
            Vector2 result;
            result.X = (source.X * matrix.m00 + source.Y * matrix.m01 + matrix.m02);
            result.Y = (source.X * matrix.m10 + source.Y * matrix.m11 + matrix.m12);
            return result;
        }
        public static void Transform(ref Matrix2x3 matrix, ref Vector2 source, out Vector2 result)
        {
            float X = source.X;
            result.X = (X * matrix.m00 + source.Y * matrix.m01 + matrix.m02);
            result.Y = (X * matrix.m10 + source.Y * matrix.m11 + matrix.m12);
        }
        public static Vector2 TransformNormal(Matrix2x3 matrix, Vector2 source)
        {
            Vector2 result;
            result.X = (source.X * matrix.m00 + source.Y * matrix.m01);
            result.Y = (source.X * matrix.m10 + source.Y * matrix.m11);
            return result;
        }
        public static void TransformNormal(ref Matrix2x3 matrix, ref Vector2 source, out Vector2 result)
        {
            float X = source.X;
            result.X = (X * matrix.m00 + source.Y * matrix.m01);
            result.Y = (X * matrix.m10 + source.Y * matrix.m11);
        }
        /// <summary>
        /// Uses a matrix multiplication to Transform the vector.
        /// </summary>
        /// <param name="matrix">The rotation matrix</param>
        /// <param name="source">The Vector to be transformed</param>
        /// <returns>The transformed vector.</returns>
        /// <remarks><seealso href="http://en.wikipedia.org/wiki/Transformation_matrix#Rotation"/></remarks>
        public static Vector2 Transform(Matrix2x2 matrix, Vector2 source)
        {
            Vector2 result;
            result.X = (source.X * matrix.m00 + source.Y * matrix.m01);
            result.Y = (source.X * matrix.m10 + source.Y * matrix.m11);
            return result;
        }
        public static void Transform(ref Matrix2x2 matrix, ref Vector2 source, out Vector2 result)
        {
            float X = source.X;
            result.X = (X * matrix.m00 + source.Y * matrix.m01);
            result.Y = (X * matrix.m10 + source.Y * matrix.m11);
        }

        /// <summary>
        /// Does Scaler Multiplication on a Vector2.
        /// </summary>
        /// <param name="scalar">The scalar value that will multiply the Vector2.</param>
        /// <param name="source">The Vector2 to be multiplied.</param>
        /// <returns>The Product of the Scaler Multiplication.</returns>
        /// <remarks><seealso href="http://en.wikipedia.org/wiki/Vector_%28spatial%29#Scalar_multiplication"/></remarks>
        public static Vector2 Multiply(Vector2 source, float scalar)
        {
            Vector2 result;
            result.X = source.X * scalar;
            result.Y = source.Y * scalar;
            return result;
        }
        public static void Multiply(ref Vector2 source, ref float scalar, out Vector2 result)
        {
            result.X = source.X * scalar;
            result.Y = source.Y * scalar;
        }
    }
}
