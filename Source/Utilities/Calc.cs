using Microsoft.Xna.Framework;

namespace Penumbra.Utilities
{
    internal static class Calc
    {
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static void CreateTransform(ref Vector2 position, ref Vector2 origin, ref Vector2 scale, float rotation,
            out Matrix transform)
        {
            transform = Matrix.CreateTranslation(new Vector3(-origin, 0))*
                        Matrix.CreateScale(new Vector3(scale, 1.0f))*
                        Matrix.CreateRotationZ(rotation)*
                        //Matrix.CreateTranslation(new Vector3(origin, 0))*
                        Matrix.CreateTranslation(new Vector3(position, 0));

            //transform = Matrix.Identity;

            //var cos = (float)Math.Cos(rotation);
            //var sin = (float)Math.Sin(rotation);            

            //transform.M11 = scale.X * cos;
            //transform.M12 = scale.X * sin;
            //transform.M21 = scale.Y * -sin;
            //transform.M22 = scale.Y * cos;
            //transform.M41 = position.X - origin.X;
            //transform.M42 = position.Y - origin.Y;            
        }
    }
}
