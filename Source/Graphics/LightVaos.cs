using System;

namespace Penumbra.Graphics
{
    internal sealed class LightVaos : IDisposable
    {
        public readonly DynamicVao PenumbraVao;
        public readonly DynamicVao UmbraVao;
        public readonly DynamicVao SolidVao;
        public readonly DynamicVao AntumbraVao;

        public bool HasPenumbra { get; set; }
        public bool HasUmbra { get; set; }
        public bool HasSolid { get; set; }
        public bool HasAntumbra { get; set; }

        public LightVaos(DynamicVao umbra, DynamicVao penumbra, DynamicVao antumbra, DynamicVao solid)
        {
            UmbraVao = umbra;
            PenumbraVao = penumbra;
            AntumbraVao = antumbra;
            SolidVao = solid;            
        }

        public void Dispose()
        {
            PenumbraVao?.Dispose();
            UmbraVao?.Dispose();
            SolidVao?.Dispose();
            AntumbraVao?.Dispose();
        }
    }
}
