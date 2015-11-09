using Microsoft.Xna.Framework;
using Polygon = Penumbra.Utilities.FastList<Microsoft.Xna.Framework.Vector2>;
using Indices = Penumbra.Utilities.FastList<int>;

namespace Penumbra.Graphics
{
    // ref: http://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml
    internal static unsafe class Triangulator
    {
        private const float Epsilon = 1e-5f;

        public static bool Process(Polygon contour, Indices resultIndices, bool clockwise = true)
        {
            int n = contour.Count;
            if (n < 3)
                return false;

            int* intermediaryIndices = stackalloc int[n];

            for (var i = 0; i < n; i++)
                intermediaryIndices[i] = i;

            int nv = n;

            /*  remove nv-2 Vertices, creating 1 triangle every time */
            int count = 2*nv; /* error detection */

            for (int v = nv - 1; nv > 2;)
            {
                /* if we loop, it is probably a non-simple polygon */
                if (0 >= (count--))
                    //** Triangulate: ERROR - probably bad polygon!
                    return false;

                /* three consecutive vertices in current polygon, <u,v,w> */
                int u = v;
                if (nv <= u) u = 0; /* previous */
                v = u + 1;
                if (nv <= v) v = 0; /* new v    */
                int w = v + 1;
                if (nv <= w) w = 0; /* next     */

                if (Snip(contour, u, v, w, nv, intermediaryIndices))
                {
                    int s, t;

                    /* true names of the vertices */
                    int a = intermediaryIndices[u];
                    int b = intermediaryIndices[v];
                    int c = intermediaryIndices[w];

                    /* output Triangle */
                    if (clockwise)
                    {
                        resultIndices.Add(a);
                        resultIndices.Add(c);
                        resultIndices.Add(b);
                    }
                    else
                    {
                        resultIndices.Add(a);
                        resultIndices.Add(b);
                        resultIndices.Add(c);
                    }

                    /* remove v from remaining polygon */
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        intermediaryIndices[s] = intermediaryIndices[t];
                    nv--;

                    /* resest error detection counter */
                    count = 2*nv;
                }
            }

            return true;
        }

        private static bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float ax = c.X - b.X;
            float ay = c.Y - b.Y;
            float bx = a.X - c.X;
            float by = a.Y - c.Y;
            float cx = b.X - a.X;
            float cy = b.Y - a.Y;
            float apx = p.X - a.X;
            float apy = p.Y - a.Y;
            float bpx = p.X - b.X;
            float bpy = p.Y - b.Y;
            float cpx = p.X - c.X;
            float cpy = p.Y - c.Y;

            var aCROSSbp = ax*bpy - ay*bpx;
            var cCROSSap = cx*apy - cy*apx;
            var bCROSScp = bx*cpy - by*cpx;

            return aCROSSbp >= 0.0f && bCROSScp >= 0.0f && cCROSSap >= 0.0f;
        }

        private static bool Snip(Polygon contour, int u, int v, int w, int n, int* indices)
        {
            Vector2 a = contour[indices[u]];
            Vector2 b = contour[indices[v]];
            Vector2 c = contour[indices[w]];

            if (Epsilon > (b.X - a.X)*(c.Y - a.Y) - (b.Y - a.Y)*(c.X - a.X)) return false;

            for (int i = 0; i < n; i++)
            {
                if (i == u || i == v || i == w) continue;
                Vector2 p = contour[indices[i]];
                if (InsideTriangle(a, b, c, p)) return false;
            }

            return true;
        }
    }
}
