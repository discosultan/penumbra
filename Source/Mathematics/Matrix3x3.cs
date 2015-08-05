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
    /// A 3x3 matrix which can represent rotations around axes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = Matrix3x3.Size)]
    public struct Matrix3x3
    {
        #region const fields
        /// <summary>
        /// The number of rows.
        /// </summary>
        public const int RowCount = 3;
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

        public static readonly Matrix3x3 Identity = new Matrix3x3(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1);
        public static readonly Matrix3x3 Zero = new Matrix3x3(
        0, 0, 0,
        0, 0, 0,
        0, 0, 0);
        #endregion
        #region static methods

        public static void Copy(ref Matrix3x3 matrix, Scalar[] destArray)
        {
            Copy(ref matrix, destArray, 0);
        }
        public static void Copy(ref Matrix3x3 matrix, Scalar[] destArray, int index)
        {
            destArray[index] = matrix.m00;
            destArray[++index] = matrix.m01;
            destArray[++index] = matrix.m02;

            destArray[++index] = matrix.m10;
            destArray[++index] = matrix.m11;
            destArray[++index] = matrix.m12;

            destArray[++index] = matrix.m20;
            destArray[++index] = matrix.m21;
            destArray[++index] = matrix.m22;
        }
        public static void Copy(Scalar[] sourceArray, out Matrix3x3 result)
        {
            Copy(sourceArray, 0, out result);
        }
        public static void Copy(Scalar[] sourceArray, int index, out Matrix3x3 result)
        {
            result.m00 = sourceArray[index];
            result.m01 = sourceArray[++index];
            result.m02 = sourceArray[++index];

            result.m10 = sourceArray[++index];
            result.m11 = sourceArray[++index];
            result.m12 = sourceArray[++index];

            result.m20 = sourceArray[++index];
            result.m21 = sourceArray[++index];
            result.m22 = sourceArray[++index];
        }

        public static void CopyTranspose(ref Matrix3x3 matrix, Scalar[] destArray)
        {
            CopyTranspose(ref matrix, destArray, 0);
        }
        public static void CopyTranspose(ref Matrix3x3 matrix, Scalar[] destArray, int index)
        {
            destArray[index] = matrix.m00;
            destArray[++index] = matrix.m10;
            destArray[++index] = matrix.m20;

            destArray[++index] = matrix.m01;
            destArray[++index] = matrix.m11;
            destArray[++index] = matrix.m21;

            destArray[++index] = matrix.m02;
            destArray[++index] = matrix.m12;
            destArray[++index] = matrix.m22;
        }
        public static void CopyTranspose(Scalar[] sourceArray, out Matrix3x3 result)
        {
            CopyTranspose(sourceArray, 0, out result);
        }
        public static void CopyTranspose(Scalar[] sourceArray, int index, out Matrix3x3 result)
        {
            result.m00 = sourceArray[index];
            result.m10 = sourceArray[++index];
            result.m20 = sourceArray[++index];

            result.m01 = sourceArray[++index];
            result.m11 = sourceArray[++index];
            result.m21 = sourceArray[++index];

            result.m02 = sourceArray[++index];
            result.m12 = sourceArray[++index];
            result.m22 = sourceArray[++index];
        }

        public static void Copy(ref Matrix4x4 source, out Matrix3x3 dest)
        {
            dest.m00 = source.m00;
            dest.m01 = source.m01;
            dest.m02 = source.m02;

            dest.m10 = source.m10;
            dest.m11 = source.m11;
            dest.m12 = source.m12;

            dest.m20 = source.m20;
            dest.m21 = source.m21;
            dest.m22 = source.m22;
        }
        public static void Copy(ref Matrix2x2 source, ref Matrix3x3 dest)
        {
            dest.m00 = source.m00;
            dest.m01 = source.m01;

            dest.m10 = source.m10;
            dest.m11 = source.m11;
        }

        public static void Copy2DToOpenGlMatrix(ref Matrix3x3 source, Scalar[] destArray)
        {
            destArray[0] = source.m00;
            destArray[1] = source.m10;

            destArray[3] = source.m20;

            destArray[4] = source.m01;
            destArray[5] = source.m11;

            destArray[7] = source.m21;

            destArray[12] = source.m02;
            destArray[13] = source.m12;

            destArray[15] = source.m22;
        }


        public static Matrix3x3 Lerp(Matrix3x3 left, Matrix3x3 right, Scalar amount)
        {
            Matrix3x3 result;
            Lerp(ref left, ref right, ref amount, out result);
            return result;
        }
        public static void Lerp(ref Matrix3x3 left, ref  Matrix3x3 right, ref  Scalar amount, out Matrix3x3 result)
        {
            result.m00 = (right.m00 - left.m00) * amount + left.m00;
            result.m01 = (right.m01 - left.m01) * amount + left.m01;
            result.m02 = (right.m02 - left.m02) * amount + left.m02;

            result.m10 = (right.m10 - left.m10) * amount + left.m10;
            result.m11 = (right.m11 - left.m11) * amount + left.m11;
            result.m12 = (right.m12 - left.m12) * amount + left.m12;

            result.m20 = (right.m20 - left.m20) * amount + left.m20;
            result.m21 = (right.m21 - left.m21) * amount + left.m21;
            result.m22 = (right.m22 - left.m22) * amount + left.m22;
        }


        /// <summary>
        /// Used to multiply (concatenate) two Matrix4x4s.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 Multiply(Matrix3x3 left, Matrix3x3 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;

            return result;
        }
        public static void Multiply(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;

            Scalar m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            Scalar m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            Scalar m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;


            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = m22;
        }

        /// <summary>
        /// Used to multiply a Matrix3x3 object by a scalar value..
        /// </summary>
        /// <param name="left"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Matrix3x3 Multiply(Matrix3x3 left, Scalar scalar)
        {
            Matrix3x3 result;

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;

            return result;
        }
        public static void Multiply(ref Matrix3x3 left, ref Scalar scalar, out Matrix3x3 result)
        {

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;
        }

        /// <summary>
        /// Used to multiply (concatenate) a Matrix3x3 and a Matrix2x2.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 Multiply(Matrix3x3 left, Matrix2x2 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m02;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m12;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11;
            result.m22 = left.m22;

            return result;
        }
        public static void Multiply(ref Matrix3x3 left, ref Matrix2x2 right, out Matrix3x3 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;

            Scalar m20 = left.m20 * right.m00 + left.m21 * right.m10;
            Scalar m21 = left.m20 * right.m01 + left.m21 * right.m11;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = left.m02;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = left.m12;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = left.m22;
        }

        /// <summary>
        /// Used to multiply (concatenate) a Matrix3x3 and a Matrix2x2.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 Multiply(Matrix2x2 left, Matrix3x3 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;



            return result;
        }
        public static void Multiply(ref Matrix2x2 left, ref Matrix3x3 right, out Matrix3x3 result)
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

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;


        }


        /// <summary>
        /// Used to add two matrices together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 Add(Matrix3x3 left, Matrix3x3 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;

            return result;
        }
        public static void Add(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
        }

        public static Matrix3x3 Add(Matrix2x2 left, Matrix3x3 right)
        {
            Matrix3x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix2x2 left, ref Matrix3x3 right, out Matrix3x3 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = right.m12;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = 1 + right.m22;
        }
        public static Matrix3x3 Add(Matrix3x3 left, Matrix2x2 right)
        {
            Matrix3x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix3x3 left, ref Matrix2x2 right, out Matrix3x3 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12;

            result.m20 = left.m20;
            result.m21 = left.m21;
            result.m22 = left.m22 + 1;
        }



        /// <summary>
        /// Used to subtract two matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 Subtract(Matrix3x3 left, Matrix3x3 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;

            return result;
        }
        public static void Subtract(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;

        }

        public static Matrix3x3 Subtract(Matrix2x2 left, Matrix3x3 right)
        {
            Matrix3x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix2x2 left, ref Matrix3x3 right, out Matrix3x3 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = -right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = -right.m12;

            result.m20 = -right.m20;
            result.m21 = -right.m21;
            result.m22 = 1 - right.m22;
        }
        public static Matrix3x3 Subtract(Matrix3x3 left, Matrix2x2 right)
        {
            Matrix3x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix3x3 left, ref Matrix2x2 right, out Matrix3x3 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12;

            result.m20 = left.m20;
            result.m21 = left.m21;
            result.m22 = left.m22 - 1;
        }

        /// <summary>
        /// Negates a Matrix3x3.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 Negate(Matrix3x3 source)
        {
            Matrix3x3 result;

            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;

            result.m20 = -source.m20;
            result.m21 = -source.m21;
            result.m22 = -source.m22;


            return result;
        }
        [CLSCompliant(false)]
        public static void Negate(ref Matrix3x3 source)
        {
            Negate(ref source, out source);
        }
        public static void Negate(ref Matrix3x3 source, out Matrix3x3 result)
        {
            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;

            result.m20 = -source.m20;
            result.m21 = -source.m21;
            result.m22 = -source.m22;
        }

        public static Matrix3x3 Invert(Matrix3x3 source)
        {
            Matrix3x3 result;
            Invert(ref source, out result);
            return result;
        }
        public static void Invert(ref Matrix3x3 source, out Matrix3x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;

            Scalar m11 = source.m11;
            Scalar m12 = source.m12;




            Scalar m11m22m12m21 = (m11 * source.m22 - m12 * source.m21);
            Scalar m10m22m12m20 = (source.m10 * source.m22 - m12 * source.m20);
            Scalar m10m21m11m20 = (source.m10 * source.m21 - m11 * source.m20);



            Scalar detInv = 1 / (source.m00 * (m11m22m12m21) - m01 * (m10m22m12m20) + m02 * (m10m21m11m20));


            result.m01 = detInv * (-(m01 * source.m22 - m02 * source.m21));
            result.m02 = detInv * (m01 * m12 - m02 * m11);

            result.m11 = detInv * (source.m00 * source.m22 - m02 * source.m20);
            result.m12 = detInv * (-(source.m00 * m12 - m02 * source.m10));

            result.m21 = detInv * (-(source.m00 * source.m21 - m01 * source.m20));
            result.m22 = detInv * (source.m00 * m11 - m01 * source.m10);


            result.m00 = detInv * (m11m22m12m21);
            result.m10 = detInv * (-(m10m22m12m20));
            result.m20 = detInv * (m10m21m11m20);

        }

        public static Scalar GetDeterminant(Matrix3x3 source)
        {
            Scalar result;
            GetDeterminant(ref source, out result);
            return result;
        }
        public static void GetDeterminant(ref Matrix3x3 source, out Scalar result)
        {
            // note: this is an expanded version of the Ogre determinant() method, to give better performance in C#. Generated using a script
            result =
            source.m00 * (source.m11 * source.m22 - source.m12 * source.m21) -
            source.m01 * (source.m10 * source.m22 - source.m12 * source.m20) +
            source.m02 * (source.m10 * source.m21 - source.m11 * source.m20);
        }

        public static Matrix3x3 Transpose(Matrix3x3 source)
        {
            Matrix3x3 result;
            Transpose(ref source, out result);
            return result;
        }
        public static void Transpose(ref Matrix3x3 source, out Matrix3x3 result)
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



        }

        public static Matrix3x3 GetAdjoint(Matrix3x3 source)
        {
            Matrix3x3 result;
            GetAdjoint(ref source, out result);
            return result;
        }
        public static void GetAdjoint(ref Matrix3x3 source, out Matrix3x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;

            Scalar m11m22m12m21 = (m11 * source.m22 - m12 * source.m21);
            Scalar m10m22m12m20 = (source.m10 * source.m22 - m12 * source.m20);
            Scalar m10m21m11m20 = (source.m10 * source.m21 - m11 * source.m20);


            result.m01 = (-(m01 * source.m22 - m02 * source.m21));
            result.m02 = (m01 * m12 - m02 * m11);

            result.m11 = (source.m00 * source.m22 - m02 * source.m20);
            result.m12 = (-(source.m00 * m12 - m02 * source.m10));

            result.m21 = (-(source.m00 * source.m21 - m01 * source.m20));
            result.m22 = (source.m00 * m11 - m01 * source.m10);


            result.m00 = (m11m22m12m21);
            result.m10 = (-(m10m22m12m20));
            result.m20 = (m10m21m11m20);

        }

        public static Matrix3x3 GetCofactor(Matrix3x3 source)
        {
            Matrix3x3 result;
            GetCofactor(ref source, out result);
            return result;
        }
        public static void GetCofactor(ref Matrix3x3 source, out Matrix3x3 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;

            Scalar m11m22m12m21 = (m11 * source.m22 - m12 * source.m21);
            Scalar m10m22m12m20 = (source.m10 * source.m22 - m12 * source.m20);
            Scalar m10m21m11m20 = (source.m10 * source.m21 - m11 * source.m20);


            result.m01 = ((m01 * source.m22 - m02 * source.m21));
            result.m02 = (-(m01 * m12 - m02 * m11));

            result.m11 = (-(source.m00 * source.m22 - m02 * source.m20));
            result.m12 = ((source.m00 * m12 - m02 * source.m10));

            result.m21 = ((source.m00 * source.m21 - m01 * source.m20));
            result.m22 = (-(source.m00 * m11 - m01 * source.m10));


            result.m00 = (-(m11m22m12m21));
            result.m10 = ((m10m22m12m20));
            result.m20 = (-(m10m21m11m20));

        }



        public static Matrix3x3 FromArray(Scalar[] array)
        {
            Matrix3x3 result;
            Copy(array, 0, out result);
            return result;
        }
        public static Matrix3x3 FromTransposedArray(Scalar[] array)
        {
            Matrix3x3 result;
            CopyTranspose(array, 0, out result);
            return result;
        }

        public static Matrix3x3 FromTransformation(Scalar rotation, Vector2 translation)
        {
            Matrix3x3 result;
            FromTransformation(ref rotation, ref translation, out result);
            return result;
        }
        public static void FromTransformation(ref Scalar rotation, ref Vector2 translation, out Matrix3x3 result)
        {
            result.m00 = Calc.Cos(rotation);
            result.m10 = Calc.Sin(rotation);
            result.m01 = -result.m10;
            result.m11 = result.m00;
            result.m02 = translation.X;
            result.m12 = translation.Y;
            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
        }

        public static Matrix3x3 FromRotationX(Scalar radianAngle)
        {
            Matrix3x3 result;

            result.m21 = Calc.Sin(radianAngle);


            result.m00 = 1;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = Calc.Cos(radianAngle);
            result.m12 = -result.m21;

            result.m20 = 0;
            result.m22 = result.m11;

            return result;
        }
        public static void FromRotationX(ref Scalar radianAngle, out Matrix3x3 result)
        {
            result.m21 = Calc.Sin(radianAngle);


            result.m00 = 1;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = Calc.Cos(radianAngle);
            result.m12 = -result.m21;

            result.m20 = 0;
            result.m22 = result.m11;
        }
        public static Matrix3x3 FromRotationY(Scalar radianAngle)
        {
            Matrix3x3 result;

            result.m20 = Calc.Sin(radianAngle);


            result.m00 = Calc.Cos(radianAngle);
            result.m01 = 0;
            result.m02 = -result.m20;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = 0;

            result.m21 = 0;
            result.m22 = result.m00;

            return result;
        }
        public static void FromRotationY(ref Scalar radianAngle, out Matrix3x3 result)
        {
            result.m20 = Calc.Sin(radianAngle);


            result.m00 = Calc.Cos(radianAngle);
            result.m01 = 0;
            result.m02 = -result.m20;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = 0;

            result.m21 = 0;
            result.m22 = result.m00;

        }
        public static Matrix3x3 FromRotationZ(Scalar radianAngle)
        {
            Matrix3x3 result;

            result.m10 = Calc.Sin(radianAngle);


            result.m00 = Calc.Cos(radianAngle);
            result.m01 = -result.m10;
            result.m02 = 0;

            result.m11 = result.m00;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;

            return result;
        }
        public static void FromRotationZ(ref Scalar radianAngle, out Matrix3x3 result)
        {

            result.m10 = Calc.Sin(radianAngle);


            result.m00 = Calc.Cos(radianAngle);
            result.m01 = -result.m10;
            result.m02 = 0;

            result.m11 = result.m00;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;

        }
        public static Matrix3x3 FromRotationAxisUsingAtan(Scalar radianAngle, Vector3 axis)
        {
            Scalar zAngle;
            Scalar yAngle;
            if (axis.X == 0)
            {
                if (axis.Y == 0)
                {
                    return FromRotationZ(radianAngle);
                }
                else
                {
                    zAngle = MathHelper.PiOver2;
                    yAngle = (Scalar)Math.Atan(axis.Z / axis.Y);
                }
            }
            else
            {
                zAngle = (Scalar)Math.Atan(axis.Y / axis.X);
                yAngle = (Scalar)Math.Atan(axis.Z / Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y));
            }
            return FromRotationZ(-zAngle) *
            FromRotationY(-yAngle) *
            FromRotationX(radianAngle) *
            FromRotationY(yAngle) *
            FromRotationZ(zAngle);
        }

        //public static Matrix3x3 FromRotationAxis(Scalar radianAngle, Vector3 axis)
        //{
        //    Matrix3x3 first = FromLookAt(Vector3.Zero, axis, new Vector3(axis.Z, axis.X, axis.Y));
        //    return first.Inverted * FromRotationZ(radianAngle) * first;
        //}
        //internal static Matrix3x3 FromLookAt(Vector3 origin, Vector3 positiveZAxis, Vector3 onPositiveY)
        //{
        //    Matrix3x3 rv = Identity;
        //    rv.Rz = Vector3.Normalize(positiveZAxis - origin);
        //    rv.Rx = Vector3.Normalize((onPositiveY - origin) ^ rv.Rz);
        //    rv.Ry = Vector3.Normalize(rv.Rz ^ rv.Rx);
        //    return rv;
        //}


        public static Matrix3x3 FromScale(Vector3 scale)
        {
            Matrix3x3 result;

            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = scale.Z;

            return result;
        }
        public static void FromScale(ref Vector3 scale, out Matrix3x3 result)
        {
            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = scale.Z;
        }
        public static Matrix3x3 FromScale(Vector2 scale)
        {
            Matrix3x3 result;
            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            return result;
        }
        public static void FromScale(ref Vector2 scale, out Matrix3x3 result)
        {
            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
        }
        public static Matrix3x3 FromTranslate2D(Vector2 value)
        {
            Matrix3x3 result;

            result.m00 = 1;
            result.m01 = 0;
            result.m02 = value.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = value.Y;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;

            return result;
        }
        public static void FromTranslate2D(ref Vector2 value, out Matrix3x3 result)
        {
            result.m00 = 1;
            result.m01 = 0;
            result.m02 = value.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = value.Y;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
        }
        public static Matrix3x3 FromShear3D(Vector2 value)
        {
            Matrix3x3 result;

            result.m00 = 1;
            result.m01 = 0;
            result.m02 = value.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = value.Y;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;

            return result;
        }
        public static void FromShear3D(ref Vector2 value, out Matrix3x3 result)
        {
            result.m00 = 1;
            result.m01 = 0;
            result.m02 = value.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = value.Y;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
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

        /// <summary>
        /// Constructs this Matrix from 3 euler angles, in degrees.
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <param name="roll"></param>
        public static Matrix3x3 FromEulerAnglesXYZ(Scalar yaw, Scalar pitch, Scalar roll)
        {
            return FromRotationX(yaw) * (FromRotationY(pitch) * FromRotationZ(roll));
        }


        public static bool Equals(Matrix3x3 left, Matrix3x3 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22;
        }
        [CLSCompliant(false)]
        public static bool Equals(ref Matrix3x3 left, ref Matrix3x3 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22;
        }

        #endregion
        #region fields

        // | m00 m01 m02 |
        // | m10 m11 m12 |
        // | m20 m21 m22 |

        public Scalar m00, m01, m02;

        public Scalar m10, m11, m12;

        public Scalar m20, m21, m22;

        #endregion
        #region Constructors

        /// <summary>
        /// Creates a new Matrix3 with all the specified parameters.
        /// </summary>
        public Matrix3x3(Scalar m00, Scalar m01, Scalar m02,
        Scalar m10, Scalar m11, Scalar m12,
        Scalar m20, Scalar m21, Scalar m22)
        {
            this.m00 = m00; this.m01 = m01; this.m02 = m02;
            this.m10 = m10; this.m11 = m11; this.m12 = m12;
            this.m20 = m20; this.m21 = m21; this.m22 = m22;
        }

        /// <summary>
        /// Create a new Matrix from 3 Vertex3 objects.
        /// </summary>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>

        public Matrix3x3(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis)
        {
            this.m00 = xAxis.X; this.m01 = xAxis.Y; this.m02 = xAxis.Z;
            this.m10 = yAxis.X; this.m11 = yAxis.Y; this.m12 = yAxis.Z;
            this.m20 = zAxis.X; this.m21 = zAxis.Y; this.m22 = zAxis.Z;
        }
        public Matrix3x3(Scalar[] values) : this(values, 0) { }
        public Matrix3x3(Scalar[] values, int index)
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

        public Vector3 Rz
        {
            get
            {
                Vector3 value;
                value.X = m20;
                value.Y = m21;
                value.Z = m22;
                return value;
            }
            set
            {
                m20 = value.X;
                m21 = value.Y;
                m22 = value.Z;
            }
        }

        public Vector3 Cx
        {
            get
            {
                return new Vector3(m00, m10, m20);
            }
            set
            {
                this.m00 = value.X;
                this.m10 = value.Y;
                this.m20 = value.Z;
            }
        }

        public Vector3 Cy
        {
            get
            {
                return new Vector3(m01, m11, m21);
            }
            set
            {
                this.m01 = value.X;
                this.m11 = value.Y;
                this.m21 = value.Z;
            }
        }

        public Vector3 Cz
        {
            get
            {
                return new Vector3(m02, m12, m22);
            }
            set
            {
                this.m02 = value.X;
                this.m12 = value.Y;
                this.m22 = value.Z;
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
        public Matrix3x3 Transposed
        {
            get
            {
                Matrix3x3 result;
                Transpose(ref this, out result);
                return result;
            }
        }
        public Matrix3x3 Adjoint
        {
            get
            {
                Matrix3x3 result;
                GetAdjoint(ref this, out result);
                return result;
            }
        }
        public Matrix3x3 Cofactor
        {
            get
            {
                Matrix3x3 result;
                GetCofactor(ref this, out result);
                return result;
            }
        }
        public Matrix3x3 Inverted
        {
            get
            {
                Matrix3x3 result;
                Invert(ref this, out result);
                return result;
            }
        }

        #endregion Properties
        #region Methods

        public Vector3 GetColumn(int columnIndex)
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
            throw new IndexOutOfRangeException();
        }
        public void SetColumn(int columnIndex, Vector3 value)
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
            throw new IndexOutOfRangeException();
        }
        public Vector3 GetRow(int rowIndex)
        {
            switch (rowIndex)
            {
                case 0:
                    return Rx;
                case 1:
                    return Ry;
                case 2:
                    return Rz;
            }
            throw new IndexOutOfRangeException();
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
                case 2:
                    Rz = value;
                    return;
            }
            throw new IndexOutOfRangeException();
        }

        public Scalar[,] ToMatrixArray()
        {
            return new Scalar[RowCount, ColumnCount] { { m00, m01, m02 }, { m10, m11, m12 }, { m20, m21, m22 } };
        }
        public Scalar[] ToArray()
        {
            return new Scalar[Count] { m00, m01, m02, m10, m11, m12, m20, m21, m22 };
        }
        public Scalar[] ToTransposedArray()
        {
            return new Scalar[Count] { m00, m10, m20, m01, m11, m21, m02, m12, m22 };
        }

        public Matrix4x4 ToMatrix4x4From2D()
        {
            Matrix4x4 result = Matrix4x4.Identity;
            result.m00 = this.m00; result.m01 = this.m01; result.m03 = this.m02;
            result.m10 = this.m10; result.m11 = this.m11; result.m13 = this.m12;
            result.m30 = this.m20; result.m31 = this.m21; result.m33 = this.m22;
            return result;
        }
        public Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 result = Matrix4x4.Identity;
            result.m00 = this.m00; result.m01 = this.m01; result.m02 = this.m02;
            result.m10 = this.m10; result.m11 = this.m11; result.m12 = this.m12;
            result.m20 = this.m20; result.m21 = this.m21; result.m22 = this.m22;
            return result;
        }

        public void CopyTo(Scalar[] array, int index)
        {
            Copy(ref this, array, index);
        }
        public void CopyTransposedTo(Scalar[] array, int index)
        {
            CopyTranspose(ref this, array, index);
        }
        public void CopyFrom(Scalar[] array, int index)
        {
            Copy(array, index, out this);
        }
        public void CopyTransposedFrom(Scalar[] array, int index)
        {
            CopyTranspose(array, index, out this);
        }

        public override int GetHashCode()
        {
            return
            m00.GetHashCode() ^ m01.GetHashCode() ^ m02.GetHashCode() ^
            m10.GetHashCode() ^ m11.GetHashCode() ^ m12.GetHashCode() ^
            m20.GetHashCode() ^ m21.GetHashCode() ^ m22.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return
                (obj is Matrix3x3) &&
                Equals((Matrix3x3)obj);
        }
        public bool Equals(Matrix3x3 other)
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
        public static Matrix3x3 operator *(Matrix3x3 left, Matrix3x3 right)
        {

            Matrix3x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;

            return result;
        }
        /// <summary>
        /// Multiply (concatenate) a Matrix3x3 and a Matrix2x2
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 operator *(Matrix2x2 left, Matrix3x3 right)
        {

            Matrix3x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;

            return result;
        }
        /// <summary>
        /// Multiply (concatenate) a Matrix3x3 and a Matrix2x2
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 operator *(Matrix3x3 left, Matrix2x2 right)
        {

            Matrix3x3 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m02;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m12;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11;
            result.m22 = left.m22;

            return result;
        }

        /// <summary>
        /// Multiplies all the items in the Matrix3 by a scalar value.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Matrix3x3 operator *(Matrix3x3 matrix, Scalar scalar)
        {
            Matrix3x3 result;

            result.m00 = matrix.m00 * scalar;
            result.m01 = matrix.m01 * scalar;
            result.m02 = matrix.m02 * scalar;
            result.m10 = matrix.m10 * scalar;
            result.m11 = matrix.m11 * scalar;
            result.m12 = matrix.m12 * scalar;
            result.m20 = matrix.m20 * scalar;
            result.m21 = matrix.m21 * scalar;
            result.m22 = matrix.m22 * scalar;

            return result;
        }
        /// <summary>
        /// Multiplies all the items in the Matrix3 by a scalar value.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Matrix3x3 operator *(Scalar scalar, Matrix3x3 matrix)
        {
            Matrix3x3 result;

            result.m00 = matrix.m00 * scalar;
            result.m01 = matrix.m01 * scalar;
            result.m02 = matrix.m02 * scalar;
            result.m10 = matrix.m10 * scalar;
            result.m11 = matrix.m11 * scalar;
            result.m12 = matrix.m12 * scalar;
            result.m20 = matrix.m20 * scalar;
            result.m21 = matrix.m21 * scalar;
            result.m22 = matrix.m22 * scalar;

            return result;
        }
        /// <summary>
        /// Used to add two matrices together.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 operator +(Matrix3x3 left, Matrix3x3 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;

            return result;
        }
        public static Matrix3x3 operator +(Matrix2x2 left, Matrix3x3 right)
        {
            Matrix3x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix3x3 operator +(Matrix3x3 left, Matrix2x2 right)
        {
            Matrix3x3 result;
            Add(ref left, ref right, out result);
            return result;
        }
        /// <summary>
        /// Used to subtract two matrices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Matrix3x3 operator -(Matrix3x3 left, Matrix3x3 right)
        {
            Matrix3x3 result;

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;

            return result;
        }
        public static Matrix3x3 operator -(Matrix2x2 left, Matrix3x3 right)
        {
            Matrix3x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix3x3 operator -(Matrix3x3 left, Matrix2x2 right)
        {
            Matrix3x3 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        /// <summary>
        /// Negates all the items in the Matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Matrix3x3 operator -(Matrix3x3 matrix)
        {
            Matrix3x3 result;

            result.m00 = -matrix.m00;
            result.m01 = -matrix.m01;
            result.m02 = -matrix.m02;
            result.m10 = -matrix.m10;
            result.m11 = -matrix.m11;
            result.m12 = -matrix.m12;
            result.m20 = -matrix.m20;
            result.m21 = -matrix.m21;
            result.m22 = -matrix.m22;

            return result;
        }
        /// <summary>
        /// Test two matrices for (value) equality
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Matrix3x3 left, Matrix3x3 right)
        {
            return
            left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 &&
            left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 &&
            left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22;
        }
        public static bool operator !=(Matrix3x3 left, Matrix3x3 right)
        {
            return !(left == right);
        }


        public static explicit operator Matrix3x3(Matrix4x4 source)
        {
            Matrix3x3 result;

            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = source.m02;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = source.m12;

            result.m20 = source.m20;
            result.m21 = source.m21;
            result.m22 = source.m22;

            return result;
        }
        public static explicit operator Matrix3x3(Matrix2x2 source)
        {
            Matrix3x3 result;

            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = 0;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;





            return result;
        }

        #endregion
    }
}
