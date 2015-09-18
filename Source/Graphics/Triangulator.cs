using Microsoft.Xna.Framework;
using Polygon = Penumbra.Utilities.FastList<Microsoft.Xna.Framework.Vector2>;
using Indices = Penumbra.Utilities.FastList<int>;

namespace Penumbra.Graphics
{
    // ref: http://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml
    internal static class Triangulator
    {
        const float Epsilon = 1e-5f;
        
        public static bool Process(Polygon contour, Indices V, Indices resultIndices, bool clockwise = true)
        {
            int n = contour.Count;
            if (n < 3) return false;
          
            for (int v = 0; v < n; v++) V.Add(v);

            int nv = n;

            /*  remove nv-2 Vertices, creating 1 triangle every time */
            int count = 2 * nv;   /* error detection */

            for (int m = 0, v = nv - 1; nv > 2;)
            {
                /* if we loop, it is probably a non-simple polygon */
                if (0 >= (count--))
                {
                    //** Triangulate: ERROR - probably bad polygon!
                    return false;
                }

                /* three consecutive vertices in current polygon, <u,v,w> */
                int u = v; if (nv <= u) u = 0;     /* previous */
                v = u + 1; if (nv <= v) v = 0;     /* new v    */
                int w = v + 1; if (nv <= w) w = 0;     /* next     */

                if (Snip(contour, u, v, w, nv, V))
                {
                    int a, b, c, s, t;

                    /* true names of the vertices */
                    a = V[u]; b = V[v]; c = V[w];

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

                    m++;

                    /* remove v from remaining polygon */
                    for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t]; nv--;

                    /* resest error detection counter */
                    count = 2 * nv;
                }
            }

            return true;
        }

        private static bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = c.X - b.X; ay = c.Y - b.Y;
            bx = a.X - c.X; by = a.Y - c.Y;
            cx = b.X - a.X; cy = b.Y - a.Y;
            apx = p.X - a.X; apy = p.Y - a.Y;
            bpx = p.X - b.X; bpy = p.Y - b.Y;
            cpx = p.X - c.X; cpy = p.Y - c.Y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }

        private static bool Snip(Polygon contour, int u, int v, int w, int n, Indices V)
        {
            int p;

            Vector2 A = contour[V[u]];
            Vector2 B = contour[V[v]];
            Vector2 C = contour[V[w]];

            if (Epsilon > (((B.X - A.X) * (C.Y - A.Y)) - ((B.Y - A.Y) * (C.X - A.X)))) return false;

            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w)) continue;
                Vector2 P = contour[V[p]];                
                if (InsideTriangle(A,B,C,P)) return false;
            }

            return true;
        }
    }
}
