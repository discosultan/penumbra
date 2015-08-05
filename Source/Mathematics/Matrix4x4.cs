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

namespace Penumbra.Mathematics
{
    [StructLayout(LayoutKind.Sequential, Size = Matrix4x4.Size)]
    public struct Matrix4x4
    {
        #region const fields
        /// <summary>
        /// The number of rows.
        /// </summary>
        public const int RowCount = 4;
        /// <summary>
        /// The number of columns.
        /// </summary>
        public const int ColumnCount = 4;
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
        public readonly static Matrix4x4 Zero = new Matrix4x4(
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0);

        public readonly static Matrix4x4 Identity = new Matrix4x4(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
        
        #endregion
        #region static methods

        public static void Copy(ref Matrix4x4 matrix, Scalar[] destArray)
        {
            Copy(ref matrix, destArray, 0);
        }
        public static void Copy(ref Matrix4x4 matrix, Scalar[] destArray, int index)
        {
            destArray[index] = matrix.m00;
            destArray[++index] = matrix.m01;
            destArray[++index] = matrix.m02;
            destArray[++index] = matrix.m03;

            destArray[++index] = matrix.m10;
            destArray[++index] = matrix.m11;
            destArray[++index] = matrix.m12;
            destArray[++index] = matrix.m13;

            destArray[++index] = matrix.m20;
            destArray[++index] = matrix.m21;
            destArray[++index] = matrix.m22;
            destArray[++index] = matrix.m23;

            destArray[++index] = matrix.m30;
            destArray[++index] = matrix.m31;
            destArray[++index] = matrix.m32;
            destArray[++index] = matrix.m33;
        }
        public static void Copy(Scalar[] sourceArray, out Matrix4x4 result)
        {
            Copy(sourceArray, 0, out result);
        }
        public static void Copy(Scalar[] sourceArray, int index, out Matrix4x4 result)
        {
            result.m00 = sourceArray[index];
            result.m01 = sourceArray[++index];
            result.m02 = sourceArray[++index];
            result.m03 = sourceArray[++index];

            result.m10 = sourceArray[++index];
            result.m11 = sourceArray[++index];
            result.m12 = sourceArray[++index];
            result.m13 = sourceArray[++index];

            result.m20 = sourceArray[++index];
            result.m21 = sourceArray[++index];
            result.m22 = sourceArray[++index];
            result.m23 = sourceArray[++index];

            result.m30 = sourceArray[++index];
            result.m31 = sourceArray[++index];
            result.m32 = sourceArray[++index];
            result.m33 = sourceArray[++index];
        }
        public static void CopyTranspose(ref Matrix4x4 matrix, Scalar[] destArray)
        {
            CopyTranspose(ref matrix, destArray, 0);
        }
        public static void CopyTranspose(ref Matrix4x4 matrix, Scalar[] destArray, int index)
        {
            destArray[index] = matrix.m00;
            destArray[++index] = matrix.m10;
            destArray[++index] = matrix.m20;
            destArray[++index] = matrix.m30;

            destArray[++index] = matrix.m01;
            destArray[++index] = matrix.m11;
            destArray[++index] = matrix.m21;
            destArray[++index] = matrix.m31;

            destArray[++index] = matrix.m02;
            destArray[++index] = matrix.m12;
            destArray[++index] = matrix.m22;
            destArray[++index] = matrix.m32;

            destArray[++index] = matrix.m03;
            destArray[++index] = matrix.m13;
            destArray[++index] = matrix.m23;
            destArray[++index] = matrix.m33;
        }
        public static void CopyTranspose(Scalar[] sourceArray, out Matrix4x4 result)
        {
            CopyTranspose(sourceArray, 0, out result);
        }
        public static void CopyTranspose(Scalar[] sourceArray, int index, out Matrix4x4 result)
        {
            result.m00 = sourceArray[index];
            result.m10 = sourceArray[++index];
            result.m20 = sourceArray[++index];
            result.m30 = sourceArray[++index];

            result.m01 = sourceArray[++index];
            result.m11 = sourceArray[++index];
            result.m21 = sourceArray[++index];
            result.m31 = sourceArray[++index];

            result.m02 = sourceArray[++index];
            result.m12 = sourceArray[++index];
            result.m22 = sourceArray[++index];
            result.m32 = sourceArray[++index];

            result.m03 = sourceArray[++index];
            result.m13 = sourceArray[++index];
            result.m23 = sourceArray[++index];
            result.m33 = sourceArray[++index];
        }
        public static void Copy(ref Matrix3x3 source, ref Matrix4x4 dest)
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
        public static void Copy(ref Matrix2x2 source, ref Matrix4x4 dest)
        {
            dest.m00 = source.m00;
            dest.m01 = source.m01;

            dest.m10 = source.m10;
            dest.m11 = source.m11;
        }

        public static Matrix4x4 Lerp(Matrix4x4 left, Matrix4x4 right, Scalar amount)
        {
            Matrix4x4 result;
            Lerp(ref left, ref right, ref amount, out result);
            return result;
        }
        public static void Lerp(ref Matrix4x4 left, ref  Matrix4x4 right, ref  Scalar amount, out Matrix4x4 result)
        {
            result.m00 = (right.m00 - left.m00) * amount + left.m00;
            result.m01 = (right.m01 - left.m01) * amount + left.m01;
            result.m02 = (right.m02 - left.m02) * amount + left.m02;
            result.m03 = (right.m03 - left.m03) * amount + left.m03;

            result.m10 = (right.m10 - left.m10) * amount + left.m10;
            result.m11 = (right.m11 - left.m11) * amount + left.m11;
            result.m12 = (right.m12 - left.m12) * amount + left.m12;
            result.m13 = (right.m13 - left.m13) * amount + left.m13;

            result.m20 = (right.m20 - left.m20) * amount + left.m20;
            result.m21 = (right.m21 - left.m21) * amount + left.m21;
            result.m22 = (right.m22 - left.m22) * amount + left.m22;
            result.m23 = (right.m23 - left.m23) * amount + left.m23;

            result.m30 = (right.m30 - left.m30) * amount + left.m30;
            result.m31 = (right.m31 - left.m31) * amount + left.m31;
            result.m32 = (right.m32 - left.m32) * amount + left.m32;
            result.m33 = (right.m33 - left.m33) * amount + left.m33;

        }


        public static Matrix4x4 FromArray(Scalar[] array)
        {
            Matrix4x4 result;
            Copy(array, 0, out result);
            return result;
        }
        public static Matrix4x4 FromTransposedArray(Scalar[] array)
        {
            Matrix4x4 result;
            CopyTranspose(array, 0, out result);
            return result;
        }

        public static Matrix4x4 FromTranslation(Vector3 translation)
        {
            Matrix4x4 result;

            result.m00 = 1;
            result.m01 = 0;
            result.m02 = 0;
            result.m03 = translation.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = 0;
            result.m13 = translation.Y;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            result.m23 = translation.Z;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;

            return result;
        }
        public static void FromTranslation(ref Vector3 translation, out Matrix4x4 result)
        {
            result.m00 = 1;
            result.m01 = 0;
            result.m02 = 0;
            result.m03 = translation.X;

            result.m10 = 0;
            result.m11 = 1;
            result.m12 = 0;
            result.m13 = translation.Y;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            result.m23 = translation.Z;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;
        }
        public static Matrix4x4 FromScale(Vector3 scale)
        {
            Matrix4x4 result;
            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;
            result.m03 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;
            result.m13 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = scale.Z;
            result.m23 = 0;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;
            return result;
        }
        public static void FromScale(ref Vector3 scale, out Matrix4x4 result)
        {
            result.m00 = scale.X;
            result.m01 = 0;
            result.m02 = 0;
            result.m03 = 0;

            result.m10 = 0;
            result.m11 = scale.Y;
            result.m12 = 0;
            result.m13 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = scale.Z;
            result.m23 = 0;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;
        }
        //public static Matrix4x4 FromLookAt(Vector3 origin, Vector3 positiveZAxis, Vector3 onPositiveY)
        //{
        //    return Matrix3x3.FromLookAt(origin, positiveZAxis, onPositiveY) * FromTranslation(-origin);
        //}

        public static Matrix4x4 FromOrthographic(
                Scalar left, Scalar right,
                Scalar bottom, Scalar top,
                Scalar near, Scalar far)
        {

            Matrix4x4 result;
            result.m00 = 2 / (right - left);
            result.m01 = 0;
            result.m02 = 0;
            result.m03 = -(right + left) / (right - left);

            result.m10 = 0;
            result.m11 = 2 / (top - bottom);
            result.m12 = 0;
            result.m13 = -(top + bottom) / (top - bottom);

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = -2 / (far - near);
            result.m23 = (far + near) / (far - near);

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;
            return result;
        }

        public static Matrix4x4 From2DMatrix(Matrix3x3 source)
        {
            Matrix4x4 result;
            From2DMatrix(ref source, out result);
            return result;
        }
        public static void From2DMatrix(ref Matrix3x3 source, out Matrix4x4 result)
        {
            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = 0;
            result.m03 = source.m02;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = 0;
            result.m13 = source.m12;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            result.m23 = 0;

            result.m30 = source.m20;
            result.m31 = source.m21;
            result.m32 = 0;
            result.m33 = source.m22;
        }

        public static Matrix4x4 From2DMatrix(Matrix2x3 source)
        {
            Matrix4x4 result;
            From2DMatrix(ref source, out result);
            return result;
        }
        public static void From2DMatrix(ref Matrix2x3 source, out Matrix4x4 result)
        {
            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = 0;
            result.m03 = source.m02;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = 0;
            result.m13 = source.m12;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            result.m23 = 0;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;
        }

        public static Matrix4x4 Multiply(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20 + left.m03 * right.m30;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21 + left.m03 * right.m31;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22 + left.m03 * right.m32;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23 + left.m03 * right.m33;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20 + left.m13 * right.m30;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20 + left.m23 * right.m30;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32;
            result.m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20 + left.m33 * right.m30;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32;
            result.m33 = left.m30 * right.m03 + left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33;

            return result;
        }
        public static void Multiply(ref Matrix4x4 left, ref Matrix4x4 right, out Matrix4x4 result)
        {


            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20 + left.m03 * right.m30;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21 + left.m03 * right.m31;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22 + left.m03 * right.m32;
            Scalar m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23 + left.m03 * right.m33;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20 + left.m13 * right.m30;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32;
            Scalar m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33;

            Scalar m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20 + left.m23 * right.m30;
            Scalar m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31;
            Scalar m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32;
            Scalar m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33;

            Scalar m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20 + left.m33 * right.m30;
            Scalar m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31;
            Scalar m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32;
            Scalar m33 = left.m30 * right.m03 + left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;
            result.m03 = m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = m13;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = m22;
            result.m23 = m23;

            result.m30 = m30;
            result.m31 = m31;
            result.m32 = m32;
            result.m33 = m33;



        }

        public static Matrix4x4 Multiply(Matrix4x4 left, Scalar scalar)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;
            result.m03 = left.m03 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;
            result.m13 = left.m13 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;
            result.m23 = left.m23 * scalar;

            result.m30 = left.m30 * scalar;
            result.m31 = left.m31 * scalar;
            result.m32 = left.m32 * scalar;
            result.m33 = left.m33 * scalar;

            return result;
        }
        public static void Multiply(ref Matrix4x4 left, ref Scalar scalar, out Matrix4x4 result)
        {

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;
            result.m03 = left.m03 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;
            result.m13 = left.m13 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;
            result.m23 = left.m23 * scalar;

            result.m30 = left.m30 * scalar;
            result.m31 = left.m31 * scalar;
            result.m32 = left.m32 * scalar;
            result.m33 = left.m33 * scalar;
        }

        public static Matrix4x4 Multiply(Matrix4x4 left, Matrix3x3 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            result.m03 = left.m03;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            result.m13 = left.m13;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;
            result.m23 = left.m23;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22;
            result.m33 = left.m33;

            return result;
        }
        public static void Multiply(ref Matrix4x4 left, ref Matrix3x3 right, out Matrix4x4 result)
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

            Scalar m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20;
            Scalar m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21;
            Scalar m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22;


            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;
            result.m03 = left.m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = left.m13;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = m22;
            result.m23 = left.m23;

            result.m30 = m30;
            result.m31 = m31;
            result.m32 = m32;
            result.m33 = left.m33;
        }

        public static Matrix4x4 Multiply(Matrix3x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;
            result.m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;

            return result;
        }
        public static void Multiply(ref Matrix3x3 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            Scalar m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            Scalar m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23;

            Scalar m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            Scalar m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            Scalar m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;
            Scalar m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;
            result.m03 = m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = m13;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = m22;
            result.m23 = m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;
        }

        public static Matrix4x4 Multiply(Matrix4x4 left, Matrix2x3 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12;
            result.m13 = left.m13;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22;
            result.m23 = left.m23;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32;
            result.m33 = left.m33;

            return result;
        }
        public static void Multiply(ref Matrix4x4 left, ref Matrix2x3 right, out Matrix4x4 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12;

            Scalar m20 = left.m20 * right.m00 + left.m21 * right.m10;
            Scalar m21 = left.m20 * right.m01 + left.m21 * right.m11;
            Scalar m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22;

            Scalar m30 = left.m30 * right.m00 + left.m31 * right.m10;
            Scalar m31 = left.m30 * right.m01 + left.m31 * right.m11;
            Scalar m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32;


            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;
            result.m03 = left.m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = left.m13;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = m22;
            result.m23 = left.m23;

            result.m30 = m30;
            result.m31 = m31;
            result.m32 = m32;
            result.m33 = left.m33;
        }

        public static Matrix4x4 Multiply(Matrix2x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23;

            result.m20 =  right.m20;
            result.m21 =  right.m21;
            result.m22 =  right.m22;
            result.m23 =  right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;

            return result;
        }
        public static void Multiply(ref Matrix2x3 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            Scalar m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            Scalar m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;
            result.m03 = m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = m13;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;
        }

        public static Matrix4x4 Multiply(Matrix4x4 left, Matrix2x2 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m12;
            result.m13 = left.m13;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11;
            result.m22 = left.m22;
            result.m23 = left.m23;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11;
            result.m32 = left.m32;
            result.m33 = left.m33;

            return result;
        }
        public static void Multiply(ref Matrix4x4 left, ref Matrix2x2 right, out Matrix4x4 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;


            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;


            Scalar m20 = left.m20 * right.m00 + left.m21 * right.m10;
            Scalar m21 = left.m20 * right.m01 + left.m21 * right.m11;


            Scalar m30 = left.m30 * right.m00 + left.m31 * right.m10;
            Scalar m31 = left.m30 * right.m01 + left.m31 * right.m11;


            result.m00 = m00;
            result.m01 = m01;
            result.m02 = left.m02;
            result.m03 = left.m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = left.m12;
            result.m13 = left.m13;

            result.m20 = m20;
            result.m21 = m21;
            result.m22 = left.m22;
            result.m23 = left.m23;

            result.m30 = m30;
            result.m31 = m31;
            result.m32 = left.m32;
            result.m33 = left.m33;


        }

        public static Matrix4x4 Multiply(Matrix2x2 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;

            return result;
        }
        public static void Multiply(ref Matrix2x2 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            Scalar m00 = left.m00 * right.m00 + left.m01 * right.m10;
            Scalar m01 = left.m00 * right.m01 + left.m01 * right.m11;
            Scalar m02 = left.m00 * right.m02 + left.m01 * right.m12;
            Scalar m03 = left.m00 * right.m03 + left.m01 * right.m13;

            Scalar m10 = left.m10 * right.m00 + left.m11 * right.m10;
            Scalar m11 = left.m10 * right.m01 + left.m11 * right.m11;
            Scalar m12 = left.m10 * right.m02 + left.m11 * right.m12;
            Scalar m13 = left.m10 * right.m03 + left.m11 * right.m13;

            result.m00 = m00;
            result.m01 = m01;
            result.m02 = m02;
            result.m03 = m03;

            result.m10 = m10;
            result.m11 = m11;
            result.m12 = m12;
            result.m13 = m13;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;
        }


        public static Matrix4x4 Add(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03 + right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13 + right.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = left.m23 + right.m23;

            result.m30 = left.m30 + right.m30;
            result.m31 = left.m31 + right.m31;
            result.m32 = left.m32 + right.m32;
            result.m33 = left.m33 + right.m33;

            return result;
        }
        public static void Add(ref Matrix4x4 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03 + right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13 + right.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = left.m23 + right.m23;

            result.m30 = left.m30 + right.m30;
            result.m31 = left.m31 + right.m31;
            result.m32 = left.m32 + right.m32;
            result.m33 = left.m33 + right.m33;
        }

        public static Matrix4x4 Add(Matrix3x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix3x3 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = right.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = 1 + right.m33;
        }
        public static Matrix4x4 Add(Matrix4x4 left, Matrix3x3 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix4x4 left, ref Matrix3x3 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = left.m23;

            result.m30 = left.m30;
            result.m31 = left.m31;
            result.m32 = left.m32;
            result.m33 = left.m33 + 1;
        }

        public static Matrix4x4 Add(Matrix2x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix2x3 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = right.m13;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = 1 + right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = 1 + right.m33;
        }
        public static Matrix4x4 Add(Matrix4x4 left, Matrix2x3 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix4x4 left, ref Matrix2x3 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13;

            result.m20 = left.m20;
            result.m21 = left.m21;
            result.m22 = left.m22 + 1;
            result.m23 = left.m23;

            result.m30 = left.m30;
            result.m31 = left.m31;
            result.m32 = left.m32;
            result.m33 = left.m33 + 1;
        }

        public static Matrix4x4 Add(Matrix2x2 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix2x2 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = right.m02;
            result.m03 = right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = right.m12;
            result.m13 = right.m13;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = 1 + right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = 1 + right.m33;
        }
        public static Matrix4x4 Add(Matrix4x4 left, Matrix2x2 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static void Add(ref Matrix4x4 left, ref Matrix2x2 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12;
            result.m13 = left.m13;

            result.m20 = left.m20;
            result.m21 = left.m21;
            result.m22 = left.m22 + 1;
            result.m23 = left.m23;

            result.m30 = left.m30;
            result.m31 = left.m31;
            result.m32 = left.m32;
            result.m33 = left.m33 + 1;
        }


        public static Matrix4x4 Subtract(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03 - right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13 - right.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = left.m23 - right.m23;

            result.m30 = left.m30 - right.m30;
            result.m31 = left.m31 - right.m31;
            result.m32 = left.m32 - right.m32;
            result.m33 = left.m33 - right.m33;

            return result;
        }
        public static void Subtract(ref Matrix4x4 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03 - right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13 - right.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = left.m23 - right.m23;

            result.m30 = left.m30 - right.m30;
            result.m31 = left.m31 - right.m31;
            result.m32 = left.m32 - right.m32;
            result.m33 = left.m33 - right.m33;
        }

        public static Matrix4x4 Subtract(Matrix3x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix3x3 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = -right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = -right.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = -right.m23;

            result.m30 = -right.m30;
            result.m31 = -right.m31;
            result.m32 = -right.m32;
            result.m33 = 1 - right.m33;
        }
        public static Matrix4x4 Subtract(Matrix4x4 left, Matrix3x3 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix4x4 left, ref Matrix3x3 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = left.m23;

            result.m30 = left.m30;
            result.m31 = left.m31;
            result.m32 = left.m32;
            result.m33 = left.m33 - 1;
        }

        public static Matrix4x4 Subtract(Matrix2x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix2x3 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = -right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = -right.m13;

            result.m20 = -right.m20;
            result.m21 = -right.m21;
            result.m22 = 1 - right.m22;
            result.m23 = -right.m23;

            result.m30 = -right.m30;
            result.m31 = -right.m31;
            result.m32 = -right.m32;
            result.m33 = 1 - right.m33;
        }
        public static Matrix4x4 Subtract(Matrix4x4 left, Matrix2x3 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix4x4 left, ref Matrix2x3 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13;

            result.m20 = left.m20;
            result.m21 = left.m21;
            result.m22 = left.m22 - 1;
            result.m23 = left.m23;

            result.m30 = left.m30;
            result.m31 = left.m31;
            result.m32 = left.m32;
            result.m33 = left.m33 - 1;
        }

        public static Matrix4x4 Subtract(Matrix2x2 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix2x2 left, ref Matrix4x4 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = -right.m02;
            result.m03 = -right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = -right.m12;
            result.m13 = -right.m13;

            result.m20 = -right.m20;
            result.m21 = -right.m21;
            result.m22 = 1 - right.m22;
            result.m23 = -right.m23;

            result.m30 = -right.m30;
            result.m31 = -right.m31;
            result.m32 = -right.m32;
            result.m33 = 1 - right.m33;
        }
        public static Matrix4x4 Subtract(Matrix4x4 left, Matrix2x2 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static void Subtract(ref Matrix4x4 left, ref Matrix2x2 right, out Matrix4x4 result)
        {
            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12;
            result.m13 = left.m13;

            result.m20 = left.m20;
            result.m21 = left.m21;
            result.m22 = left.m22 - 1;
            result.m23 = left.m23;

            result.m30 = left.m30;
            result.m31 = left.m31;
            result.m32 = left.m32;
            result.m33 = left.m33 - 1;
        }

        public static Matrix4x4 Negate(Matrix4x4 source)
        {
            Matrix4x4 result;

            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;
            result.m03 = -source.m03;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;
            result.m13 = -source.m13;

            result.m20 = -source.m20;
            result.m21 = -source.m21;
            result.m22 = -source.m22;
            result.m23 = -source.m23;

            result.m30 = -source.m30;
            result.m31 = -source.m31;
            result.m32 = -source.m32;
            result.m33 = -source.m33;

            return result;
        }
        [CLSCompliant(false)]
        public static void Negate(ref Matrix4x4 source)
        {
            Negate(ref source, out source);
        }
        public static void Negate(ref Matrix4x4 source, out Matrix4x4 result)
        {
            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;
            result.m03 = -source.m03;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;
            result.m13 = -source.m13;

            result.m20 = -source.m20;
            result.m21 = -source.m21;
            result.m22 = -source.m22;
            result.m23 = -source.m23;

            result.m30 = -source.m30;
            result.m31 = -source.m31;
            result.m32 = -source.m32;
            result.m33 = -source.m33;
        }

        public static Matrix4x4 Invert(Matrix4x4 source)
        {
            Matrix4x4 result;
            Invert(ref source, out result);
            return result;
        }
        public static void Invert(ref Matrix4x4 source, out Matrix4x4 result)
        {
            Scalar m00 = source.m00;
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m03 = source.m03;

            Scalar m10 = source.m10;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;
            Scalar m13 = source.m13;

            Scalar m22m33m32m23 = (source.m22 * source.m33 - source.m32 * source.m23);
            Scalar m21m33m31m23 = (source.m21 * source.m33 - source.m31 * source.m23);
            Scalar m21m32m31m22 = (source.m21 * source.m32 - source.m31 * source.m22);

            Scalar m12m33m32m13 = (m12 * source.m33 - source.m32 * m13);
            Scalar m11m33m31m13 = (m11 * source.m33 - source.m31 * m13);
            Scalar m11m32m31m12 = (m11 * source.m32 - source.m31 * m12);

            Scalar m12m23m22m13 = (m12 * source.m23 - source.m22 * m13);
            Scalar m11m23m21m13 = (m11 * source.m23 - source.m21 * m13);
            Scalar m11m22m21m12 = (m11 * source.m22 - source.m21 * m12);

            Scalar m20m33m30m23 = (source.m20 * source.m33 - source.m30 * source.m23);
            Scalar m20m32m30m22 = (source.m20 * source.m32 - source.m30 * source.m22);
            Scalar m10m33m30m13 = (m10 * source.m33 - source.m30 * m13);

            Scalar m10m32m30m12 = (m10 * source.m32 - source.m30 * m12);
            Scalar m10m23m20m13 = (m10 * source.m23 - source.m20 * m13);
            Scalar m10m22m20m12 = (m10 * source.m22 - source.m20 * m12);

            Scalar m20m31m30m21 = (source.m20 * source.m31 - source.m30 * source.m21);
            Scalar m10m31m30m11 = (m10 * source.m31 - source.m30 * m11);
            Scalar m10m21m20m11 = (m10 * source.m21 - source.m20 * m11);


            Scalar detInv = 1 /
            (m00 * (m11 * m22m33m32m23 - m12 * m21m33m31m23 + m13 * m21m32m31m22) -
            m01 * (m10 * m22m33m32m23 - m12 * m20m33m30m23 + m13 * m20m32m30m22) +
            m02 * (m10 * m21m33m31m23 - m11 * m20m33m30m23 + m13 * m20m31m30m21) -
            m03 * (m10 * m21m32m31m22 - m11 * m20m32m30m22 + m12 * m20m31m30m21));


            result.m00 = detInv * (m11 * m22m33m32m23 - m12 * m21m33m31m23 + m13 * m21m32m31m22);
            result.m01 = detInv * (-(m01 * m22m33m32m23 - m02 * m21m33m31m23 + m03 * m21m32m31m22));
            result.m02 = detInv * (m01 * m12m33m32m13 - m02 * m11m33m31m13 + m03 * m11m32m31m12);
            result.m03 = detInv * (-(m01 * m12m23m22m13 - m02 * m11m23m21m13 + m03 * m11m22m21m12));

            result.m10 = detInv * (-(m10 * m22m33m32m23 - m12 * m20m33m30m23 + m13 * m20m32m30m22));
            result.m11 = detInv * (m00 * m22m33m32m23 - m02 * m20m33m30m23 + m03 * m20m32m30m22);
            result.m12 = detInv * (-(m00 * m12m33m32m13 - m02 * m10m33m30m13 + m03 * m10m32m30m12));
            result.m13 = detInv * (m00 * m12m23m22m13 - m02 * m10m23m20m13 + m03 * m10m22m20m12);

            result.m20 = detInv * (m10 * m21m33m31m23 - m11 * m20m33m30m23 + m13 * m20m31m30m21);
            result.m21 = detInv * (-(m00 * m21m33m31m23 - m01 * m20m33m30m23 + m03 * m20m31m30m21));
            result.m22 = detInv * (m00 * m11m33m31m13 - m01 * m10m33m30m13 + m03 * m20m31m30m21);
            result.m23 = detInv * (-(m00 * m11m23m21m13 - m01 * m10m23m20m13 + m03 * m10m21m20m11));

            result.m30 = detInv * (-(m10 * m21m32m31m22 - m11 * m20m32m30m22 + m12 * m20m31m30m21));
            result.m31 = detInv * (m00 * m21m32m31m22 - m01 * m20m32m30m22 + m02 * m20m31m30m21);
            result.m32 = detInv * (-(m00 * m11m32m31m12 - m01 * m10m32m30m12 + m02 * m10m31m30m11));
            result.m33 = detInv * (m00 * m11m22m21m12 - m01 * m10m22m20m12 + m02 * m10m21m20m11);

        }

        public static Scalar GetDeterminant(Matrix4x4 source)
        {
            Scalar result;
            GetDeterminant(ref source, out result);
            return result;
        }
        public static void GetDeterminant(ref Matrix4x4 source, out Scalar result)
        {
            Scalar m22m33m32m23 = (source.m22 * source.m33 - source.m32 * source.m23);
            Scalar m21m33m31m23 = (source.m21 * source.m33 - source.m31 * source.m23);
            Scalar m21m32m31m22 = (source.m21 * source.m32 - source.m31 * source.m22);

            Scalar m20m33m30m23 = (source.m20 * source.m33 - source.m30 * source.m23);
            Scalar m20m32m30m22 = (source.m20 * source.m32 - source.m30 * source.m22);
            Scalar m20m31m30m21 = (source.m20 * source.m31 - source.m30 * source.m21);

            result =
                source.m00 * (source.m11 * m22m33m32m23 - source.m12 * m21m33m31m23 + source.m13 * m21m32m31m22) -
                source.m01 * (source.m10 * m22m33m32m23 - source.m12 * m20m33m30m23 + source.m13 * m20m32m30m22) +
                source.m02 * (source.m10 * m21m33m31m23 - source.m11 * m20m33m30m23 + source.m13 * m20m31m30m21) -
                source.m03 * (source.m10 * m21m32m31m22 - source.m11 * m20m32m30m22 + source.m12 * m20m31m30m21);
        }

        public static Matrix4x4 Transpose(Matrix4x4 source)
        {
            Matrix4x4 result;
            Transpose(ref source, out result);
            return result;
        }
        public static void Transpose(ref Matrix4x4 source, out Matrix4x4 result)
        {
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m03 = source.m03;

            Scalar m12 = source.m12;
            Scalar m13 = source.m13;
            Scalar m23 = source.m23;

            result.m00 = source.m00;
            result.m01 = source.m10;
            result.m02 = source.m20;
            result.m03 = source.m30;

            result.m10 = m01;
            result.m11 = source.m11;
            result.m12 = source.m21;
            result.m13 = source.m31;

            result.m20 = m02;
            result.m21 = m12;
            result.m22 = source.m22;
            result.m23 = source.m32;

            result.m30 = m03;
            result.m31 = m13;
            result.m32 = m23;
            result.m33 = source.m33;

        }

        public static Matrix4x4 GetAdjoint(Matrix4x4 source)
        {
            Matrix4x4 result;
            GetAdjoint(ref source, out result);
            return result;
        }
        public static void GetAdjoint(ref Matrix4x4 source, out Matrix4x4 result)
        {
            Scalar m00 = source.m00;
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m03 = source.m03;

            Scalar m10 = source.m10;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;
            Scalar m13 = source.m13;

            //even further expanded to give even better performance. Generated using a keyboard and mouse
            Scalar m22m33m32m23 = (source.m22 * source.m33 - source.m32 * source.m23);
            Scalar m21m33m31m23 = (source.m21 * source.m33 - source.m31 * source.m23);
            Scalar m21m32m31m22 = (source.m21 * source.m32 - source.m31 * source.m22);

            Scalar m12m33m32m13 = (m12 * source.m33 - source.m32 * m13);
            Scalar m11m33m31m13 = (m11 * source.m33 - source.m31 * m13);
            Scalar m11m32m31m12 = (m11 * source.m32 - source.m31 * m12);

            Scalar m12m23m22m13 = (m12 * source.m23 - source.m22 * m13);
            Scalar m11m23m21m13 = (m11 * source.m23 - source.m21 * m13);
            Scalar m11m22m21m12 = (m11 * source.m22 - source.m21 * m12);

            Scalar m20m33m30m23 = (source.m20 * source.m33 - source.m30 * source.m23);
            Scalar m20m32m30m22 = (source.m20 * source.m32 - source.m30 * source.m22);
            Scalar m10m33m30m13 = (m10 * source.m33 - source.m30 * m13);

            Scalar m10m32m30m12 = (m10 * source.m32 - source.m30 * m12);
            Scalar m10m23m20m13 = (m10 * source.m23 - source.m20 * m13);
            Scalar m10m22m20m12 = (m10 * source.m22 - source.m20 * m12);

            Scalar m20m31m30m21 = (source.m20 * source.m31 - source.m30 * source.m21);
            Scalar m10m31m30m11 = (m10 * source.m31 - source.m30 * m11);
            Scalar m10m21m20m11 = (m10 * source.m21 - source.m20 * m11);




            // note: this is an expanded version of the Ogre adjoint() method, to give better performance in C#. Generated using a script

            result.m00 = (m11 * m22m33m32m23 - m12 * m21m33m31m23 + m13 * m21m32m31m22);
            result.m01 = (-(m01 * m22m33m32m23 - m02 * m21m33m31m23 + m03 * m21m32m31m22));
            result.m02 = (m01 * m12m33m32m13 - m02 * m11m33m31m13 + m03 * m11m32m31m12);
            result.m03 = (-(m01 * m12m23m22m13 - m02 * m11m23m21m13 + m03 * m11m22m21m12));

            result.m10 = (-(m10 * m22m33m32m23 - m12 * m20m33m30m23 + m13 * m20m32m30m22));
            result.m11 = (m00 * m22m33m32m23 - m02 * m20m33m30m23 + m03 * m20m32m30m22);
            result.m12 = (-(m00 * m12m33m32m13 - m02 * m10m33m30m13 + m03 * m10m32m30m12));
            result.m13 = (m00 * m12m23m22m13 - m02 * m10m23m20m13 + m03 * m10m22m20m12);

            result.m20 = (m10 * m21m33m31m23 - m11 * m20m33m30m23 + m13 * m20m31m30m21);
            result.m21 = (-(m00 * m21m33m31m23 - m01 * m20m33m30m23 + m03 * m20m31m30m21));
            result.m22 = (m00 * m11m33m31m13 - m01 * m10m33m30m13 + m03 * m20m31m30m21);
            result.m23 = (-(m00 * m11m23m21m13 - m01 * m10m23m20m13 + m03 * m10m21m20m11));

            result.m30 = (-(m10 * m21m32m31m22 - m11 * m20m32m30m22 + m12 * m20m31m30m21));
            result.m31 = (m00 * m21m32m31m22 - m01 * m20m32m30m22 + m02 * m20m31m30m21);
            result.m32 = (-(m00 * m11m32m31m12 - m01 * m10m32m30m12 + m02 * m10m31m30m11));
            result.m33 = (m00 * m11m22m21m12 - m01 * m10m22m20m12 + m02 * m10m21m20m11);

        }

        public static Matrix4x4 GetCofactor(Matrix4x4 source)
        {
            Matrix4x4 result;
            GetCofactor(ref source, out result);
            return result;
        }
        public static void GetCofactor(ref Matrix4x4 source, out Matrix4x4 result)
        {
            Scalar m00 = source.m00;
            Scalar m01 = source.m01;
            Scalar m02 = source.m02;
            Scalar m03 = source.m03;

            Scalar m10 = source.m10;
            Scalar m11 = source.m11;
            Scalar m12 = source.m12;
            Scalar m13 = source.m13;

            //even further expanded to give even better performance. Generated using a keyboard and mouse
            Scalar m22m33m32m23 = (source.m22 * source.m33 - source.m32 * source.m23);
            Scalar m21m33m31m23 = (source.m21 * source.m33 - source.m31 * source.m23);
            Scalar m21m32m31m22 = (source.m21 * source.m32 - source.m31 * source.m22);
            Scalar m12m33m32m13 = (m12 * source.m33 - source.m32 * m13);

            Scalar m11m33m31m13 = (m11 * source.m33 - source.m31 * m13);
            Scalar m11m32m31m12 = (m11 * source.m32 - source.m31 * m12);
            Scalar m12m23m22m13 = (m12 * source.m23 - source.m22 * m13);
            Scalar m11m23m21m13 = (m11 * source.m23 - source.m21 * m13);

            Scalar m11m22m21m12 = (m11 * source.m22 - source.m21 * m12);
            Scalar m20m33m30m23 = (source.m20 * source.m33 - source.m30 * source.m23);
            Scalar m20m32m30m22 = (source.m20 * source.m32 - source.m30 * source.m22);
            Scalar m10m33m30m13 = (m10 * source.m33 - source.m30 * m13);

            Scalar m10m32m30m12 = (m10 * source.m32 - source.m30 * m12);
            Scalar m10m23m20m13 = (m10 * source.m23 - source.m20 * m13);
            Scalar m10m22m20m12 = (m10 * source.m22 - source.m20 * m12);
            Scalar m20m31m30m21 = (source.m20 * source.m31 - source.m30 * source.m21);

            Scalar m10m31m30m11 = (m10 * source.m31 - source.m30 * m11);
            Scalar m10m21m20m11 = (m10 * source.m21 - source.m20 * m11);





            result.m00 = (-(m11 * m22m33m32m23 - m12 * m21m33m31m23 + m13 * m21m32m31m22));
            result.m01 = ((m01 * m22m33m32m23 - m02 * m21m33m31m23 + m03 * m21m32m31m22));
            result.m02 = (-(m01 * m12m33m32m13 - m02 * m11m33m31m13 + m03 * m11m32m31m12));
            result.m03 = ((m01 * m12m23m22m13 - m02 * m11m23m21m13 + m03 * m11m22m21m12));

            result.m10 = ((m10 * m22m33m32m23 - m12 * m20m33m30m23 + m13 * m20m32m30m22));
            result.m11 = (-(m00 * m22m33m32m23 - m02 * m20m33m30m23 + m03 * m20m32m30m22));
            result.m12 = ((m00 * m12m33m32m13 - m02 * m10m33m30m13 + m03 * m10m32m30m12));
            result.m13 = (-(m00 * m12m23m22m13 - m02 * m10m23m20m13 + m03 * m10m22m20m12));

            result.m20 = (-(m10 * m21m33m31m23 - m11 * m20m33m30m23 + m13 * m20m31m30m21));
            result.m21 = ((m00 * m21m33m31m23 - m01 * m20m33m30m23 + m03 * m20m31m30m21));
            result.m22 = (-(m00 * m11m33m31m13 - m01 * m10m33m30m13 + m03 * m20m31m30m21));
            result.m23 = ((m00 * m11m23m21m13 - m01 * m10m23m20m13 + m03 * m10m21m20m11));

            result.m30 = ((m10 * m21m32m31m22 - m11 * m20m32m30m22 + m12 * m20m31m30m21));
            result.m31 = (-(m00 * m21m32m31m22 - m01 * m20m32m30m22 + m02 * m20m31m30m21));
            result.m32 = ((m00 * m11m32m31m12 - m01 * m10m32m30m12 + m02 * m10m31m30m11));
            result.m33 = (-(m00 * m11m22m21m12 - m01 * m10m22m20m12 + m02 * m10m21m20m11));

        }


        public static bool Equals(Matrix4x4 left, Matrix4x4 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23 &&
                left.m30 == right.m30 && left.m31 == right.m31 && left.m32 == right.m32 && left.m33 == right.m33;
        }
        [CLSCompliant(false)]
        public static bool Equals(ref Matrix4x4 left, ref Matrix4x4 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23 &&
                left.m30 == right.m30 && left.m31 == right.m31 && left.m32 == right.m32 && left.m33 == right.m33;
        }
        #endregion
        #region fields


        public Scalar m00, m01, m02, m03;

        public Scalar m10, m11, m12, m13;

        public Scalar m20, m21, m22, m23;

        public Scalar m30, m31, m32, m33;

        #endregion
        #region Constructors

        public Matrix4x4(
            Scalar m00, Scalar m01, Scalar m02, Scalar m03,
            Scalar m10, Scalar m11, Scalar m12, Scalar m13,
            Scalar m20, Scalar m21, Scalar m22, Scalar m23,
            Scalar m30, Scalar m31, Scalar m32, Scalar m33)
        {
            this.m00 = m00; this.m01 = m01; this.m02 = m02; this.m03 = m03;
            this.m10 = m10; this.m11 = m11; this.m12 = m12; this.m13 = m13;
            this.m20 = m20; this.m21 = m21; this.m22 = m22; this.m23 = m23;
            this.m30 = m30; this.m31 = m31; this.m32 = m32; this.m33 = m33;
        }

        public Matrix4x4(Vector4 xAxis, Vector4 yAxis, Vector4 zAxis, Vector4 wAxis)
        {
            m00 = xAxis.X; m01 = xAxis.Y; m02 = xAxis.Z; m03 = xAxis.W;
            m10 = yAxis.X; m11 = yAxis.Y; m12 = yAxis.Z; m13 = yAxis.W;
            m20 = zAxis.X; m21 = zAxis.Y; m22 = zAxis.Z; m23 = zAxis.W;
            m30 = wAxis.X; m31 = wAxis.Y; m32 = wAxis.Z; m33 = wAxis.W;
        }
        public Matrix4x4(Scalar[] values) : this(values, 0) { }
        public Matrix4x4(Scalar[] values, int index) { Copy(values, index, out this); }
        #endregion
        #region Properties

        public Vector4 Rx
        {
            get
            {
                Vector4 value;
                value.X = m00;
                value.Y = m01;
                value.Z = m02;
                value.W = m03;
                return value;
            }
            set
            {
                m00 = value.X;
                m01 = value.Y;
                m02 = value.Z;
                m03 = value.W;
            }
        }

        public Vector4 Ry
        {
            get
            {
                Vector4 value;
                value.X = m10;
                value.Y = m11;
                value.Z = m12;
                value.W = m13;
                return value;
            }
            set
            {
                m10 = value.X;
                m11 = value.Y;
                m12 = value.Z;
                m13 = value.W;
            }
        }

        public Vector4 Rz
        {
            get
            {
                Vector4 value;
                value.X = m20;
                value.Y = m21;
                value.Z = m22;
                value.W = m23;
                return value;
            }
            set
            {
                m20 = value.X;
                m21 = value.Y;
                m22 = value.Z;
                m23 = value.W;
            }
        }

        public Vector4 Rw
        {
            get
            {
                Vector4 value;
                value.X = m30;
                value.Y = m31;
                value.Z = m32;
                value.W = m33;
                return value;
            }
            set
            {
                m30 = value.X;
                m31 = value.Y;
                m32 = value.Z;
                m33 = value.W;
            }
        }

        public Vector4 Cx
        {
            get
            {
                Vector4 rv;
                rv.X = m00;
                rv.Y = m10;
                rv.Z = m20;
                rv.W = m30;
                return rv;
            }
            set
            {
                this.m00 = value.X;
                this.m10 = value.Y;
                this.m20 = value.Z;
                this.m30 = value.W;
            }
        }

        public Vector4 Cy
        {
            get
            {
                Vector4 rv;
                rv.X = m01;
                rv.Y = m11;
                rv.Z = m21;
                rv.W = m31;
                return rv;
            }
            set
            {
                this.m01 = value.X;
                this.m11 = value.Y;
                this.m21 = value.Z;
                this.m31 = value.W;
            }
        }

        public Vector4 Cz
        {
            get
            {
                Vector4 rv;
                rv.X = m02;
                rv.Y = m12;
                rv.Z = m22;
                rv.W = m32;
                return rv;
            }
            set
            {
                this.m02 = value.X;
                this.m12 = value.Y;
                this.m22 = value.Z;
                this.m32 = value.W;
            }
        }

        public Vector4 Cw
        {
            get
            {
                Vector4 rv;
                rv.X = m03;
                rv.Y = m13;
                rv.Z = m23;
                rv.W = m33;
                return rv;
            }
            set
            {
                this.m03 = value.X;
                this.m13 = value.Y;
                this.m23 = value.Z;
                this.m33 = value.W;
            }
        }


        /// <summary>
        /// Gets the determinant of this matrix.
        /// </summary>
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
        public Matrix4x4 Transposed
        {
            get
            {
                return new Matrix4x4(this.m00, this.m10, this.m20, this.m30,
                this.m01, this.m11, this.m21, this.m31,
                this.m02, this.m12, this.m22, this.m32,
                this.m03, this.m13, this.m23, this.m33);
            }
        }
        /// <summary>
        /// Used to generate the Cofactor of this matrix.
        /// </summary>
        /// <returns>The Cofactor matrix of the current instance.</returns>
        public Matrix4x4 Cofactor
        {
            get
            {
                Matrix4x4 result;
                GetCofactor(ref this, out result);
                return result;
            }
        }
        /// <summary>
        /// Used to generate the adjoint of this matrix..
        /// </summary>
        /// <returns>The adjoint matrix of the current instance.</returns>
        public Matrix4x4 Adjoint
        {
            get
            {
                Matrix4x4 result;
                GetAdjoint(ref this, out result);
                return result;
            }
        }
        /// <summary>
        /// written to test out a theory. a very wasteful implimentation. but works.
        /// </summary>

        /// <summary>
        /// Returns an inverted 4d matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 Inverted
        {
            get
            {
                Matrix4x4 result;
                Invert(ref this, out result);
                return result;
            }
        }

        #endregion
        #region Methods

        public Vector4 GetColumn(int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    return Cx;
                case 1:
                    return Cy;
                case 2:
                    return Cz;
                case 3:
                    return Cw;
            }
            throw new IndexOutOfRangeException();
        }
        public void SetColumn(int columnIndex, Vector4 value)
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
                case 3:
                    Cw = value;
                    return;
            }
            throw new IndexOutOfRangeException();
        }
        public Vector4 GetRow(int rowIndex)
        {
            switch (rowIndex)
            {
                case 0:
                    return Rx;
                case 1:
                    return Ry;
                case 2:
                    return Rz;
                case 3:
                    return Rw;
            }
            throw new IndexOutOfRangeException();
        }
        public void SetRow(int rowIndex, Vector4 value)
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
                case 3:
                    Rw = value;
                    return;
            }
            throw new IndexOutOfRangeException();
        }


        public Scalar[,] ToMatrixArray()
        {
            return new Scalar[RowCount, ColumnCount]
{
{ m00, m01, m02, m03 },
{ m10, m11, m12, m13 },
{ m20, m21, m22, m23 },
{ m30, m31, m32, m33 }
};
        }
        public Scalar[] ToArray()
        {
            return new Scalar[Count] { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33 };
        }
        public Scalar[] ToTransposedArray()
        {
            return new Scalar[Count] { m00, m10, m20, m30, m01, m11, m21, m31, m02, m12, m22, m32, m03, m13, m23, m33 };
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
            m00.GetHashCode() ^ m01.GetHashCode() ^ m02.GetHashCode() ^ m03.GetHashCode() ^
            m10.GetHashCode() ^ m11.GetHashCode() ^ m12.GetHashCode() ^ m13.GetHashCode() ^
            m20.GetHashCode() ^ m21.GetHashCode() ^ m22.GetHashCode() ^ m23.GetHashCode() ^
            m30.GetHashCode() ^ m31.GetHashCode() ^ m32.GetHashCode() ^ m33.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return
                (obj is Matrix4x4) &&
                Equals((Matrix4x4)obj);
        }
        public bool Equals(Matrix4x4 other)
        {
            return Equals(ref this, ref other);
        }

        #endregion
        #region indexers
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
        #region Operators

        public static Matrix4x4 operator *(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20 + left.m03 * right.m30;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21 + left.m03 * right.m31;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22 + left.m03 * right.m32;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23 + left.m03 * right.m33;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20 + left.m13 * right.m30;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21 + left.m13 * right.m31;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22 + left.m13 * right.m32;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23 + left.m13 * right.m33;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20 + left.m23 * right.m30;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21 + left.m23 * right.m31;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22 + left.m23 * right.m32;
            result.m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23 + left.m23 * right.m33;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20 + left.m33 * right.m30;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21 + left.m33 * right.m31;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22 + left.m33 * right.m32;
            result.m33 = left.m30 * right.m03 + left.m31 * right.m13 + left.m32 * right.m23 + left.m33 * right.m33;

            return result;
        }

        public static Matrix4x4 operator *(Matrix4x4 left, Scalar scalar)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * scalar;
            result.m01 = left.m01 * scalar;
            result.m02 = left.m02 * scalar;
            result.m03 = left.m03 * scalar;

            result.m10 = left.m10 * scalar;
            result.m11 = left.m11 * scalar;
            result.m12 = left.m12 * scalar;
            result.m13 = left.m13 * scalar;

            result.m20 = left.m20 * scalar;
            result.m21 = left.m21 * scalar;
            result.m22 = left.m22 * scalar;
            result.m23 = left.m23 * scalar;

            result.m30 = left.m30 * scalar;
            result.m31 = left.m31 * scalar;
            result.m32 = left.m32 * scalar;
            result.m33 = left.m33 * scalar;

            return result;
        }

        public static Matrix4x4 operator *(Matrix4x4 left, Matrix3x3 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            result.m03 = left.m03;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            result.m13 = left.m13;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;
            result.m23 = left.m23;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10 + left.m32 * right.m20;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11 + left.m32 * right.m21;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32 * right.m22;
            result.m33 = left.m33;

            return result;
        }

        public static Matrix4x4 operator *(Matrix3x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10 + left.m22 * right.m20;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11 + left.m22 * right.m21;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22 * right.m22;
            result.m23 = left.m20 * right.m03 + left.m21 * right.m13 + left.m22 * right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;

            return result;
        }

        public static Matrix4x4 operator *(Matrix4x4 left, Matrix2x3 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12;
            result.m13 = left.m13;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11;
            result.m22 = left.m20 * right.m02 + left.m21 * right.m12 + left.m22;
            result.m23 = left.m23;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11;
            result.m32 = left.m30 * right.m02 + left.m31 * right.m12 + left.m32;
            result.m33 = left.m33;

            return result;
        }

        public static Matrix4x4 operator *(Matrix2x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10 + left.m02 * right.m20;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11 + left.m02 * right.m21;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12 + left.m02 * right.m22;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13 + left.m02 * right.m23;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10 + left.m12 * right.m20;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11 + left.m12 * right.m21;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12 + left.m12 * right.m22;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13 + left.m12 * right.m23;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;

            return result;
        }

        public static Matrix4x4 operator *(Matrix4x4 left, Matrix2x2 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m02;
            result.m03 = left.m03;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m12;
            result.m13 = left.m13;

            result.m20 = left.m20 * right.m00 + left.m21 * right.m10;
            result.m21 = left.m20 * right.m01 + left.m21 * right.m11;
            result.m22 = left.m22;
            result.m23 = left.m23;

            result.m30 = left.m30 * right.m00 + left.m31 * right.m10;
            result.m31 = left.m30 * right.m01 + left.m31 * right.m11;
            result.m32 = left.m32;
            result.m33 = left.m33;

            return result;
        }

        public static Matrix4x4 operator *(Matrix2x2 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 * right.m00 + left.m01 * right.m10;
            result.m01 = left.m00 * right.m01 + left.m01 * right.m11;
            result.m02 = left.m00 * right.m02 + left.m01 * right.m12;
            result.m03 = left.m00 * right.m03 + left.m01 * right.m13;

            result.m10 = left.m10 * right.m00 + left.m11 * right.m10;
            result.m11 = left.m10 * right.m01 + left.m11 * right.m11;
            result.m12 = left.m10 * right.m02 + left.m11 * right.m12;
            result.m13 = left.m10 * right.m03 + left.m11 * right.m13;

            result.m20 = right.m20;
            result.m21 = right.m21;
            result.m22 = right.m22;
            result.m23 = right.m23;

            result.m30 = right.m30;
            result.m31 = right.m31;
            result.m32 = right.m32;
            result.m33 = right.m33;

            return result;
        }

        public static Matrix4x4 operator +(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 + right.m00;
            result.m01 = left.m01 + right.m01;
            result.m02 = left.m02 + right.m02;
            result.m03 = left.m03 + right.m03;

            result.m10 = left.m10 + right.m10;
            result.m11 = left.m11 + right.m11;
            result.m12 = left.m12 + right.m12;
            result.m13 = left.m13 + right.m13;

            result.m20 = left.m20 + right.m20;
            result.m21 = left.m21 + right.m21;
            result.m22 = left.m22 + right.m22;
            result.m23 = left.m23 + right.m23;

            result.m30 = left.m30 + right.m30;
            result.m31 = left.m31 + right.m31;
            result.m32 = left.m32 + right.m32;
            result.m33 = left.m33 + right.m33;

            return result;
        }
        public static Matrix4x4 operator +(Matrix3x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator +(Matrix4x4 left, Matrix3x3 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator +(Matrix2x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator +(Matrix4x4 left, Matrix2x3 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator +(Matrix2x2 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator +(Matrix4x4 left, Matrix2x2 right)
        {
            Matrix4x4 result;
            Add(ref left, ref right, out result);
            return result;
        }

        public static Matrix4x4 operator -(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 result;

            result.m00 = left.m00 - right.m00;
            result.m01 = left.m01 - right.m01;
            result.m02 = left.m02 - right.m02;
            result.m03 = left.m03 - right.m03;

            result.m10 = left.m10 - right.m10;
            result.m11 = left.m11 - right.m11;
            result.m12 = left.m12 - right.m12;
            result.m13 = left.m13 - right.m13;

            result.m20 = left.m20 - right.m20;
            result.m21 = left.m21 - right.m21;
            result.m22 = left.m22 - right.m22;
            result.m23 = left.m23 - right.m23;

            result.m30 = left.m30 - right.m30;
            result.m31 = left.m31 - right.m31;
            result.m32 = left.m32 - right.m32;
            result.m33 = left.m33 - right.m33;

            return result;
        }
        public static Matrix4x4 operator -(Matrix3x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator -(Matrix4x4 left, Matrix3x3 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator -(Matrix2x3 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator -(Matrix4x4 left, Matrix2x3 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator -(Matrix2x2 left, Matrix4x4 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }
        public static Matrix4x4 operator -(Matrix4x4 left, Matrix2x2 right)
        {
            Matrix4x4 result;
            Subtract(ref left, ref right, out result);
            return result;
        }


        public static Matrix4x4 operator -(Matrix4x4 source)
        {
            Matrix4x4 result;

            result.m00 = -source.m00;
            result.m01 = -source.m01;
            result.m02 = -source.m02;
            result.m03 = -source.m03;

            result.m10 = -source.m10;
            result.m11 = -source.m11;
            result.m12 = -source.m12;
            result.m13 = -source.m13;

            result.m20 = -source.m20;
            result.m21 = -source.m21;
            result.m22 = -source.m22;
            result.m23 = -source.m23;

            result.m30 = -source.m30;
            result.m31 = -source.m31;
            result.m32 = -source.m32;
            result.m33 = -source.m33;

            return result;
        }

        public static bool operator ==(Matrix4x4 left, Matrix4x4 right)
        {
            return
                left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
                left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
                left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23 &&
                left.m30 == right.m30 && left.m31 == right.m31 && left.m32 == right.m32 && left.m33 == right.m33;
        }

        public static bool operator !=(Matrix4x4 left, Matrix4x4 right)
        {
            return !(left == right);
        }

        public static explicit operator Matrix4x4(Matrix3x3 source)
        {
            Matrix4x4 result;

            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = source.m02;
            result.m03 = 0;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = source.m12;
            result.m13 = 0;

            result.m20 = source.m20;
            result.m21 = source.m21;
            result.m22 = source.m22;
            result.m23 = 0;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;

            return result;
        }
        public static explicit operator Matrix4x4(Matrix2x2 source)
        {
            Matrix4x4 result;

            result.m00 = source.m00;
            result.m01 = source.m01;
            result.m02 = 0;
            result.m03 = 0;

            result.m10 = source.m10;
            result.m11 = source.m11;
            result.m12 = 0;
            result.m13 = 0;

            result.m20 = 0;
            result.m21 = 0;
            result.m22 = 1;
            result.m23 = 0;

            result.m30 = 0;
            result.m31 = 0;
            result.m32 = 0;
            result.m33 = 1;

            return result;
        }
        #endregion
    }
}