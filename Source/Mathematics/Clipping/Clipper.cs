using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Clipping
{
    internal static class Clipper
    { 
        //private static readonly List<List<Vector2>> _subj = new List<List<Vector2>>(1);
        //private static readonly List<List<Vector2>> _clip = new List<List<Vector2>>(1);
        private static readonly List<List<Vector2>> _sln = new List<List<Vector2>>(1);

        private static readonly AngusjClipper _clipper = new AngusjClipper
        {
            PreserveCollinear = true,
            ReverseSolution = true,
            StrictlySimple = false
        };

        public static void Clip(List<Vector2> subj, List<Vector2> clip, out List<Vector2> sln)
        {
            _clipper.Clear();
            _sln.Clear();
            _clipper.AddPath(subj, PolyType.ptSubject, true);
            _clipper.AddPath(clip, PolyType.ptClip, true);
            _clipper.Execute(ClipType.ctDifference, _sln, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            sln = _sln.Count > 0 ? _sln[0] : subj;
        }
    }
}
