using System.Collections.Generic;
using ClipperLib;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Clipping
{
    /* Terminology:
    A simple polygon is one that does not self-intersect.
    A weakly simple polygon is a simple polygon that contains 'touching' vertices, or 'touching' edges.
    A strictly simple polygon is a simple polygon that does not contain 'touching' vertices, or 'touching' edges.
    */

    internal static class AngusClipper
    {
        private static readonly List<List<IntPoint>> Solution = new List<List<IntPoint>>();
        private static readonly List<IntPoint> Subject = new List<IntPoint>();
        private static readonly List<IntPoint> Clip = new List<IntPoint>();

        private static readonly Clipper Clipper = new Clipper
        {
            PreserveCollinear = false, // removes inner vertices if there are three or more collinear
            StrictlySimple = false, // computationally expensive to turn this on
            ReverseSolution = false // result has same winding as source
        };

        public static void Union(Polygon subj, Polygon clip, Polygon result)
        {
            Execute(subj, clip, result, ClipType.ctUnion);
        }

        public static void Difference(Polygon subj, Polygon clip, Polygon result)
        {
            Execute(subj, clip, result, ClipType.ctDifference);
        }

        private static void Execute(Polygon subj, Polygon clip, Polygon result, ClipType clipType)
        {
            Solution.Clear();
            subj.ToClipperPolygon(Subject);
            clip.ToClipperPolygon(Clip);
            Clipper.AddPath(Subject, PolyType.ptSubject, true);
            Clipper.AddPath(Clip, PolyType.ptClip, true);
            Clipper.Execute(clipType, Solution, PolyFillType.pftPositive, PolyFillType.pftPositive);
            Solution[0].ToPenumbraPolygon(result);
        }
    }

    internal static class AngusClipperExtensions
    {
        private const float ScalingFactor = 10f;

        public static void ToClipperPolygon(this Polygon polygon, List<IntPoint> result)
        {
            result.Clear();
            foreach (Vector2 point in polygon)            
                result.Add(new IntPoint(FloatToInteger(point.X), FloatToInteger(point.Y)));                        
        }

        public static void ToPenumbraPolygon(this List<IntPoint> polygon, Polygon result)
        {
            result.Clear();
            foreach (IntPoint point in polygon)
                result.Add(new Vector2(IntegerToFloat(point.X), IntegerToFloat(point.Y)));            
        }

        private static long FloatToInteger(float val)
        {
            //return (long) Math.Pow(val, 10);
            return (long)(val * ScalingFactor);
        }

        private static float IntegerToFloat(long val)
        {
            return val / ScalingFactor;
        }
    }
}
