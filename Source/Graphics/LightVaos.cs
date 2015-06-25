using System;
using Penumbra.Utilities;

namespace Penumbra.Graphics
{
    internal sealed class LightVaos : IDisposable
    {
        public readonly DynamicVao PenumbraVao;
        public readonly DynamicVao UmbraVao;
        public readonly DynamicVao SolidVao;

        public bool HasPenumbra { get; set; }
        public bool HasUmbra { get; set; }
        public bool HasSolid { get; set; }

        public LightVaos(DynamicVao penumbra, DynamicVao umbra, DynamicVao solid)
        {
            PenumbraVao = penumbra;
            UmbraVao = umbra;
            SolidVao = solid;
        }

        public void Dispose()
        {
            Util.Dispose(PenumbraVao);
            Util.Dispose(UmbraVao);
            Util.Dispose(SolidVao);            
        }
    }
}
