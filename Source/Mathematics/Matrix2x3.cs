#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
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


// NOTE. The (x,y,z) coordinate system is assumed to be right-handed.
// Coordinate axis rotation matrices are of the form
// RX = 1 0 0
// 0 cos(t) -sin(t)
// 0 sin(t) cos(t)
// where t > 0 indicates a counterclockwise rotation in the yz-plane
// RY = cos(t) 0 sin(t)
// 0 1 0
// -sin(t) 0 cos(t)
// where t > 0 indicates a counterclockwise rotation in the zx-plane
// RZ = cos(t) -sin(t) 0
// sin(t) cos(t) 0
// 0 0 1
// where t > 0 indicates a counterclockwise rotation in the xy-plane.

namespace Penumbra.Mathematics
{
    /// <summary>
    /// A 2x3 matrix which can represent rotations around axes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = Matrix2x3.Size)]
    public struct Matrix2x3
    {
        #region const fields
        /// <summary>
        /// The number of rows.
        /// </summary>
        public const int RowCount = 2;
        /// <summary>
        /// The number of columns.
        /// </summary>
        public const int ColumnCount = 3;
        /// <summary>
        /// The number of Scalar values in the class.
        /// </summary>
        public const int Count = RowCount * ColumnCount;
        /// <summary>
        /// The Size of the class in bytes;
        /// </summary>
        public const int Size = sizeof(Scalar) * Count;
        #endregion
        #region static fields

        public static readonly Matrix2x3 Identity = new Matrix2x3(
        1, 0, 0,
        0, 1, 0);
        public static readonly Matrix2x3 Zero = new Matrix2x3(
        0, 0, 0,
        0, 0, 0);
        #endregion
        #region static methods

        public static void Copy(ref Matrix2x3 matrix, Scalar[] destArray)
        {
            Copy(ref matrix, destArray, 0);
        }
        public static void Copy(ref Matrix2x3 matrix, Scalar[] destArray, int index)
        {
            destArray[index] = matrix.m00;
            destArray[++index] = matrix.m01;
            destArray[++index] = matrix.m02;

            destArray[++index] = matrix.m10;
            destArray[++index] = matrix.m11;
            destArray[++index] = matrix.m12;

        }
        public static void Copy(Scalar[] sourceArray, out Matrix2x3 result)
        {
            Copy(sourceArray, 0, out result);
        }
        public static void Copy(Scalar[] sourceArray, int index, out Matrix2x3 result)
        {
            result.m00 = sourceArray[index];
            result.m01 = sourceArray[++index];
            result.m02 = sourceArray[++index];

            result.m10 = sourceArray[++index];
            result.m11 = sourceArray[++index];
            result.m12 = sourceArray[++index];

        }


        public static void Copy(ref Matrix4x4 source, out Matrix2x3 dest)
        {
            dest.m00 = source.m00;
            dest.m01 = source.m01;
            dest.m02 = source.m02;

            dest.m10 = source.m10;
            dest.m11 = source.m11;
            dest.m12 = source.m12;

        }
        public static void Copy(ref Matrix2x2 source, ref Matrix2x3 dest)
        {
            dest.m00 = source.m00;
            dest.m01 = source.m01;

            dest.m10 = source.m10;
            dest.m11 = source.m11;
        }



        public static void Copy2DToOpenGlMatrix(ref Matrix2x3 source, Scalar[] destArray)
        {
            destArray[0] = source.m00;
            destArray[1] = source.m10;


            destArray[4] = source.m01;
            destArray[5] = source.m11;


            destArray[12] = source.m02;
            destArray[13] = source.m12;

            destArray[10] = 1;
            destArray[15] = 1;
        }
        public static void Copy2DFromOpenGlMatrix(Scalar[] destArray, out Matrix2x3 result)
        {
            result.m00 = destArray[0];
            result.m10 = destArray[1];

            result.m01 = destArray[4];
            result.m11 = destArray[5];

            result.m02 = destArray[12];
            result.m12 = destArray[13];
        }

        public static Matrix2x3 Lerp(Matrix2x3 left, Matrix2x3 right, Scalar amount)
        {
            Matrix2x3 result;
            Lerp(ref left, ref right, ref amount, out result);
            return result;
        }
        public static void Lerp(ref Matrix2x3 left, ref  Matrix2x3 right, ref  Scalar amount, out Matrix2x3 result)
        {
            result.m00 = (right.m00 - left.m00) * amount + left.m00;
            result.m01 = (right.m01 - left.m01) * amount + left.m01;
            result.m02 = (right.m02 - left.m02) * amount + left.m02;

            result.m10 = (right.m10 - left.m10) * amount + left.m10;
            result.m11 = (right.m11 - left.m11) * amount + left.m11;
            result.m12 = (right.m12 - left.m12) * amount + left.m12;

        }


        /// <summary>
        /// Used to multiply (concatenate) two Matrix4x4s.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 Multiply(Matrix2x3 left, Matrix2x3 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 ;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12;


            return result;
        }
        public static void Multiply(ref Matrix2x3 left, ref Matrix2x3 right, out Matrix2x3 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
        }

        /// <summary>
        /// Used to multiply a Matrix2x3 object by a scalar value..
        /// </summary>
        /// <param name="left"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Matrix2x3 Multiply(Matrix2x3 left, Scalar scalar)
        {
            Matrix2x3 result;

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;


            return result;
        }
        public static void Multiply(ref Matrix2x3 left, ref Scalar scalar, out Matrix2x3 result)
        {

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;

        }

        /// <summary>
        /// Used to multiply (concatenate) a Matrix2x3 and a Matrix2x2.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 Multiply(Matrix2x3 left, Matrix2x2 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m02;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m12;

            return result;
        }
        public static void Multiply(ref Matrix2x3 left, ref Matrix2x2 right, out Matrix2x3 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;


            result.m00 = m00;
            result.m01 = m01;
            result.m02 = left.m02;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = left.m12;

        }

        /// <summary>
        /// Used to multiply (concatenate) a Matrix2x3 and a Matrix2x2.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 Multiply(Matrix2x2 left, Matrix2x3 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12;


            return result;
        }
        public static void Multiply(ref Matrix2x2 left, ref Matrix2x3 right, out Matrix2x3 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;



        }


        /// <summary>
        /// Used to add two matrices together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 Add(Matrix2x3 left, Matrix2x3 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;


            return result;
        }
        public static void Add(ref Matrix2x3 left, ref Matrix2x3 right, out Matrix2x3 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;


        }

        public static Matrix2x3 Add(Matrix2x2 left, Matrix2x3 right)
        {
            Matrix2x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix2x2 left, ref Matrix2x3 right, out Matrix2x3 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = right.m12;

        }
        public static Matrix2x3 Add(Matrix2x3 left, Matrix2x2 right)
        {
            Matrix2x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix2x3 left, ref Matrix2x2 right, out Matrix2x3 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12;

        }

        public static Matrix2x3 Transpose(Matrix2x3 source)
        {
            Matrix2x3 result;
            Transpose(ref source, out result);
            return result;
        }
        public static void Transpose(ref Matrix2x3 source, out Matrix2x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m12 = source.m12;

            result.m00 = source.m00;
            result.m01 = source.m10;
            result.m02 = 0;

            result.m10 = m01;
            result.m11 = source.m11;
            result.m12 = 0;




        }

        /// <summary>
        /// Used to subtract two matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 Subtract(Matrix2x3 left, Matrix2x3 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;


            return result;
        }
        public static void Subtract(ref Matrix2x3 left, ref Matrix2x3 right, out Matrix2x3 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;


        }

        public static Matrix2x3 Subtract(Matrix2x2 left, Matrix2x3 right)
        {
            Matrix2x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix2x2 left, ref Matrix2x3 right, out Matrix2x3 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = -right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = -right.m12;

        }
        public static Matrix2x3 Subtract(Matrix2x3 left, Matrix2x2 right)
        {
            Matrix2x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix2x3 left, ref Matrix2x2 right, out Matrix2x3 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12;

        }

        /// <summary>
        /// Negates a Matrix2x3.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 Negate(Matrix2x3 source)
        {
            Matrix2x3 result;

            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;



            return result;
        }
        [CLSCompliant(false)]
        public static void Negate(ref Matrix2x3 source)
        {
            Negate(ref source, out source);
        }
        public static void Negate(ref Matrix2x3 source, out Matrix2x3 result)
        {
            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;

        }

        public static Matrix2x3 Invert(Matrix2x3 source)
        {
            Matrix2x3 result;
            Invert(ref source, out result);
            return result;
        }
        public static void Invert(ref Matrix2x3 source, out Matrix2x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;

            Scalar m11 = source.m11;
            Scalar m12 = source.m12;




            // Scalar m11m22m12m21 = (m11);
            // Scalar m10m22m12m20 = (source.m10 );
            // Scalar m10m21m11m20 = 0;



            Scalar detInv = 1 / (source.m00 * m11 - m01 * source.m10);


            result.m01 = detInv * (-m01);
            result.m02 = detInv * (m01 * m12 - m02 * m11);

            result.m11 = detInv * (source.m00);
            result.m12 = detInv * (-(source.m00 * m12 - m02 * source.m10));

            result.m00 = detInv * (m11);
            result.m10 = detInv * (-source.m10);
        }

        public static Scalar GetDeterminant(Matrix2x3 source)
        {
            Scalar result;
            GetDeterminant(ref source, out result);
            return result;
        }
        public static void GetDeterminant(ref Matrix2x3 source, out Scalar result)
        {
            result =
                source.m00 * (source.m11) -
                source.m01 * (source.m10);
        }

        /*  public static Matrix2x3 Transpose(Matrix2x3 source)
         {
             Matrix2x3 result;
             Transpose(ref source, out result);
             return result;
         }
        public static void Transpose(ref Matrix2x3 source, out Matrix2x3 result)
         {
             Scalar m01 = source.m01;
             Scalar m02 = source.m02;
             Scalar m12 = source.m12;

             result.m00 = source.m00;
             result.m01 = source.m10;
             result.m02 = source.m20;

             result.m10 = m01;
             result.m11 = source.m11;
             result.m12 = source.m21;

             result.m20 = m02;
             result.m21 = m12;
             result.m22 = source.m22;



         }*/

        public static Matrix2x3 GetAdjoint(Matrix2x3 source)
        {
            Matrix2x3 result;
            GetAdjoint(ref source, out result);
            return result;
        }
        public static void GetAdjoint(ref Matrix2x3 source, out Matrix2x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;


            result.m01 = (-(m01 * 1 - m02 * 0));
            result.m02 = (m01 * m12 - m02 * m11);

            result.m11 = (source.m00);
            result.m12 = (-(source.m00 * m12 - m02 * source.m10));

            result.m00 = (m11);
            result.m10 = (-(source.m10));
        }

        public static Matrix2x3 GetCofactor(Matrix2x3 source)
        {
            Matrix2x3 result;
            GetCofactor(ref source, out result);
            return result;
        }
        public static void GetCofactor(ref Matrix2x3 source, out Matrix2x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;

            result.m01 = m01;
            result.m02 = -(m01 * m12 - m02 * m11);

            result.m11 = -source.m00;
            result.m12 = source.m00 * m12 - m02 * source.m10;

            result.m00 = -m11;
            result.m10 = source.m10;

        }


        public static Matrix2x3 FromTransformation(Scalar rotation, Vector2 translation)
        {
            Matrix2x3 result;
            FromTransformation(ref rotation, ref translation, out result);
            return result;
        }
        public static void FromTransformation(ref Scalar rotation, ref Vector2 translation, out Matrix2x3 result)
        {
            result.m00 = Calc.Cos(rotation);
            result.m10 = Calc.Sin(rotation);
            result.m01 = -result.m10;
            result.m11 = result.m00;
            result.m02 = translation.X;
            result.m12 = translation.Y;
        }


        public static Matrix2x3 FromArray(Scalar[] array)
        {
            Matrix2x3 result;
            Copy(array, 0, out result);
            return result;
        }
       /* public static Matrix2x3 FromTransposedArray(Scalar[] array)
        {
            Matrix2x3 result;
            CopyTranspose(array, 0, out result);
            return result;
        }*/

        public static Matrix2x3 FromRotationZ(Scalar radianAngle)
        {
            Matrix2x3 result;

            result.m10 = Calc.Sin(radianAngle);


            result.m00 = Calc.Cos(radianAngle);
            result.m01 = -result.m10;
            result.m02 = 0;

            result.m11 = result.m00;
            result.m12 = 0;

            return result;
        }
        public static void FromRotationZ(ref Scalar radianAngle, out Matrix2x3 result)
        {

            result.m10 = Calc.Sin(radianAngle);

            result.m00 = Calc.Cos(radianAngle);
            result.m01 = -result.m10;
            result.m02 = 0;

            result.m11 = result.m00;
            result.m12 = 0;


        }



        public static Matrix2x3 FromScale(Vector2 scale)
        {
            Matrix2x3 result;

            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;

            return result;
        }
        public static void FromScale(ref Vector2 scale, out Matrix2x3 result)
        {
            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;

        }

        public static Matrix2x3 FromTranslate2D(Vector2 value)
        {
            Matrix2x3 result;

            result.m00 = 1;
            result.m01 = 0;
            result.m02 = value.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = value.Y;


            return result;
        }
        public static void FromTranslate2D(ref Vector2 value, out Matrix2x3 result)
        {
            result.m00 = 1;
            result.m01 = 0;
            result.m02 = value.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = value.Y;
        }

        public static Scalar GetDeterminant(Scalar m00, Scalar m01, Scalar m02,
            Scalar m10, Scalar m11, Scalar m12,
            Scalar m20, Scalar m21, Scalar m22)
        {
            Scalar cofactor00 = m11 * m22 - m12 * m21;
            Scalar cofactor10 = m12 * m20 - m10 * m22;
            Scalar cofactor20 = m10 * m21 - m11 * m20;
            Scalar result =
            m00 * cofactor00 +
            m01 * cofactor10 +
            m02 * cofactor20;
            return result;
        }
        public static Scalar GetDeterminant(Vector3 Rx, Vector3 Ry, Vector3 Rz)
        {
            Scalar cofactor00 = Ry.Y * Rz.Z - Ry.Z * Rz.Y;
            Scalar cofactor10 = Ry.Z * Rz.X - Ry.X * Rz.Z;
            Scalar cofactor20 = Ry.X * Rz.Y - Ry.Y * Rz.X;
            Scalar result =
            Rx.X * cofactor00 +
            Rx.Y * cofactor10 +
            Rx.Z * cofactor20;
            return result;
        }



        public static bool Equals(Matrix2x3 left, Matrix2x3 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12;
        }
        [CLSCompliant(false)]
        public static bool Equals(ref Matrix2x3 left, ref Matrix2x3 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12;
        }

        #endregion
        #region fields

        // | m00 m01 m02 |
        // | m10 m11 m12 |
        public Scalar m00, m01, m02;
        public Scalar m10, m11, m12;

        #endregion
        #region Constructors

        /// <summary>
        /// Creates a new Matrix3 with all the specified parameters.
        /// </summary>
        public Matrix2x3(Scalar m00, Scalar m01, Scalar m02,
        Scalar m10, Scalar m11, Scalar m12)
        {
            this.m00 = m00; this.m01 = m01; this.m02 = m02;
            this.m10 = m10; this.m11 = m11; this.m12 = m12;
        }

        /// <summary>
        /// Create a new Matrix from 3 Vertex3 objects.
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        public Matrix2x3(Vector3 xAxis, Vector3 yAxis)
        {
            this.m00 = xAxis.X; this.m01 = xAxis.Y; this.m02 = xAxis.Z;
            this.m10 = yAxis.X; this.m11 = yAxis.Y; this.m12 = yAxis.Z;
        }
        public Matrix2x3(Scalar[] values) : this(values, 0) { }
        public Matrix2x3(Scalar[] values, int index)
        {
            Copy(values, index, out this);
        }
        #endregion
        #region Properties
        public Vector3 Rx
        {
            get
            {
                Vector3 value;
                value.X = m00;
                value.Y = m01;
                value.Z = m02;
                return value;
            }
            set
            {
                m00 = value.X;
                m01 = value.Y;
                m02 = value.Z;
            }
        }
        public Vector3 Ry
        {
            get
            {
                Vector3 value;
                value.X = m10;
                value.Y = m11;
                value.Z = m12;
                return value;
            }
            set
            {
                m10 = value.X;
                m11 = value.Y;
                m12 = value.Z;
            }
        }
        public Vector2 Cx
        {
            get
            {
                return new Vector2(m00, m10);
            }
            set
            {
                this.m00 = value.X;
                this.m10 = value.Y;
            }
        }
        public Vector2 Cy
        {
            get
            {
                return new Vector2(m01, m11);
            }
            set
            {
                this.m01 = value.X;
                this.m11 = value.Y;
            }
        }
        public Vector2 Cz
        {
            get
            {
                return new Vector2(m02, m12);
            }
            set
            {
                this.m02 = value.X;
                this.m12 = value.Y;
            }
        }

        public Scalar Determinant
        {
            get
            {
                Scalar result;
                GetDeterminant(ref this, out result);
                return result;
            }
        }
        /// <summary>
        /// Swap the rows of the matrix with the columns.
        /// </summary>
        /// <returns>A transposed Matrix.</returns>
        public Matrix2x3 Transposed
        {
            get
            {
                throw new NotSupportedException();
              /*  Matrix2x3 result;
                Transpose(ref this, out result);
                return result;*/
            }
        }
        public Matrix2x3 Adjoint
        {
            get
            {
                Matrix2x3 result;
                GetAdjoint(ref this, out result);
                return result;
            }
        }
        public Matrix2x3 Cofactor
        {
            get
            {
                Matrix2x3 result;
                GetCofactor(ref this, out result);
                return result;
            }
        }
        public Matrix2x3 Inverted
        {
            get
            {
                Matrix2x3 result;
                Invert(ref this, out result);
                return result;
            }
        }
        #endregion Properties
        #region Methods

        public Vector2 GetColumn(int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    return Cx;
                case 1:
                    return Cy;
                case 2:
                    return Cz;
            }
            throw new InvalidOperationException();
        }
        public void SetColumn(int columnIndex, Vector2 value)
        {
            switch (columnIndex)
            {
                case 0:
                    Cx = value;
                    return;
                case 1:
                    Cy = value;
                    return;
                case 2:
                    Cz = value;
                    return;
            }
            throw new InvalidOperationException();
        }
        public Vector3 GetRow(int rowIndex)
        {
            switch (rowIndex)
            {
                case 0:
                    return Rx;
                case 1:
                    return Ry;
            }
            throw new InvalidOperationException();
        }
        public void SetRow(int rowIndex, Vector3 value)
        {
            switch (rowIndex)
            {
                case 0:
                    Rx = value;
                    return;
                case 1:
                    Ry = value;
                    return;
            }
            throw new InvalidOperationException();
        }

        public Scalar[,] ToMatrixArray()
        {
            return new Scalar[RowCount, ColumnCount]{ { m00, m01, m02 }, { m10, m11, m12 } };
        }
        public Scalar[] ToArray()
        {
            return new Scalar[Count] { m00, m01, m02, m10, m11, m12 };
        }
        public Scalar[] ToTransposedArray()
        {
            throw new NotSupportedException();
          //  return new Scalar[Count] { m00, m10, m20, m01, m11, m21, m02, m12, m22 };
        }

        public Matrix4x4 ToMatrix4x4From2D()
        {
            Matrix4x4 result = Matrix4x4.Identity;
            result.m00 = this.m00; result.m01 = this.m01; result.m03 = this.m02;
            result.m10 = this.m10; result.m11 = this.m11; result.m13 = this.m12;
            return result;
        }
        public Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 result = Matrix4x4.Identity;
            result.m00 = this.m00; result.m01 = this.m01; result.m02 = this.m02;
            result.m10 = this.m10; result.m11 = this.m11; result.m12 = this.m12;
            return result;
        }

        public void CopyTo(Scalar[] array, int index)
        {
            Copy(ref this, array, index);
        }
        public void CopyTransposedTo(Scalar[] array, int index)
        {
            throw new NotSupportedException();
           // CopyTranspose(ref this, array, index);
        }
        public void CopyFrom(Scalar[] array, int index)
        {
            Copy(array, index, out this);
        }
        public void CopyTransposedFrom(Scalar[] array, int index)
        {
            throw new NotSupportedException();
           // CopyTranspose(array, index, out this);
        }

        public override int GetHashCode()
        {
            return
            m00.GetHashCode() ^ m01.GetHashCode() ^ m02.GetHashCode() ^
            m10.GetHashCode() ^ m11.GetHashCode() ^ m12.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return
                (obj is Matrix2x3) &&
                Equals((Matrix2x3)obj);
        }
        public bool Equals(Matrix2x3 other)
        {
            return Equals(ref this, ref other);
        }

        #endregion
        #region Indexors
#if UNSAFE
        /// <summary>
        /// Allows the Matrix to be accessed like a 2d array (i.e. matrix[2,3])
        /// </summary>
        /// <remarks>
        /// This indexer is only provided as a convenience, and is <b>not</b> recommended for use in
        /// intensive applications.
        /// </remarks>
        public Scalar this[int rowIndex, int columnIndex]
        {
            get
            {
                ThrowHelper.CheckIndex("rowIndex", rowIndex, RowCount);
                ThrowHelper.CheckIndex("columnIndex", columnIndex, ColumnCount);
                unsafe
                {
                    fixed (Scalar* pM = &m00)
                    {
                        return pM[(ColumnCount * rowIndex) + columnIndex];
                    }
                }
            }
            set
            {
                ThrowHelper.CheckIndex("rowIndex", rowIndex, RowCount);
                ThrowHelper.CheckIndex("columnIndex", columnIndex, ColumnCount);
                unsafe
                {
                    fixed (Scalar* pM = &m00)
                    {
                        pM[(ColumnCount * rowIndex) + columnIndex] = value;
                    }
                }
            }
        }
        /// <summary>
        /// Allows the Matrix to be accessed linearly (m[0] -> m[ColumnCount*RowCount-1]).
        /// </summary>
        /// <remarks>
        /// This indexer is only provided as a convenience, and is <b>not</b> recommended for use in
        /// intensive applications.
        /// </remarks>
        public Scalar this[int index]
        {
            get
            {
                ThrowHelper.CheckIndex("index", index, Count);
                unsafe
                {
                    fixed (Scalar* pMatrix = &this.m00)
                    {
                        return pMatrix[index];
                    }
                }
            }
            set
            {
                ThrowHelper.CheckIndex("index", index, Count);
                unsafe
                {
                    fixed (Scalar* pMatrix = &this.m00)
                    {
                        pMatrix[index] = value;
                    }
                }
            }
        }
#endif
        #endregion
        #region Operator overloads
        /// <summary>
        /// Multiply (concatenate) two Matrix3 instances together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 operator *(Matrix2x3 left, Matrix2x3 right)
        {

            Matrix2x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 ;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 ;

            return result;
        }
        /// <summary>
        /// Multiply (concatenate) a Matrix2x3 and a Matrix2x2
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 operator *(Matrix2x2 left, Matrix2x3 right)
        {

            Matrix2x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12;

            return result;
        }
        /// <summary>
        /// Multiply (concatenate) a Matrix2x3 and a Matrix2x2
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 operator *(Matrix2x3 left, Matrix2x2 right)
        {

            Matrix2x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m02;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m12;

            return result;
        }

        /// <summary>
        /// Multiplies all the items in the Matrix3 by a scalar value.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Matrix2x3 operator *(Matrix2x3 matrix, Scalar scalar)
        {
            Matrix2x3 result;

            result.m00 = matrix.m00 * scalar;
            result.m01 = matrix.m01 * scalar;
            result.m02 = matrix.m02 * scalar;
            result.m10 = matrix.m10 * scalar;
            result.m11 = matrix.m11 * scalar;
            result.m12 = matrix.m12 * scalar;

            return result;
        }
        /// <summary>
        /// Multiplies all the items in the Matrix3 by a scalar value.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Matrix2x3 operator *(Scalar scalar, Matrix2x3 matrix)
        {
            Matrix2x3 result;

            result.m00 = matrix.m00 * scalar;
            result.m01 = matrix.m01 * scalar;
            result.m02 = matrix.m02 * scalar;
            result.m10 = matrix.m10 * scalar;
            result.m11 = matrix.m11 * scalar;
            result.m12 = matrix.m12 * scalar;

            return result;
        }
        /// <summary>
        /// Used to add two matrices together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 operator +(Matrix2x3 left, Matrix2x3 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;


            return result;
        }
        public static Matrix2x3 operator +(Matrix2x2 left, Matrix2x3 right)
        {
            Matrix2x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix2x3 operator +(Matrix2x3 left, Matrix2x2 right)
        {
            Matrix2x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        /// <summary>
        /// Used to subtract two matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix2x3 operator -(Matrix2x3 left, Matrix2x3 right)
        {
            Matrix2x3 result;

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;


            return result;
        }
        public static Matrix2x3 operator -(Matrix2x2 left, Matrix2x3 right)
        {
            Matrix2x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix2x3 operator -(Matrix2x3 left, Matrix2x2 right)
        {
            Matrix2x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        /// <summary>
        /// Negates all the items in the Matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Matrix2x3 operator -(Matrix2x3 matrix)
        {
            Matrix2x3 result;

            result.m00 = -matrix.m00;
            result.m01 = -matrix.m01;
            result.m02 = -matrix.m02;
            result.m10 = -matrix.m10;
            result.m11 = -matrix.m11;
            result.m12 = -matrix.m12;

            return result;
        }
        /// <summary>
        /// Test two matrices for (value) equality
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Matrix2x3 left, Matrix2x3 right)
        {
            return
            left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 &&
            left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12;
        }
        public static bool operator !=(Matrix2x3 left, Matrix2x3 right)
        {
            return !(left == right);
        }


        public static explicit operator Matrix2x3(Matrix4x4 source)
        {
            Matrix2x3 result;

            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = source.m02;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = source.m12;


            return result;
        }
        public static explicit operator Matrix2x3(Matrix2x2 source)
        {
            Matrix2x3 result;

            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = 0;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = 0;






            return result;
        }

        #endregion


    }
}
