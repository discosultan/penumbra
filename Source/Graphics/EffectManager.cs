using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal static class EffectManager
    {
        private const string Prefix = "Penumbra.Content.";
        private const string Suffix = ".dx11.mgfxo";

        public static Effect LoadEffectFromEmbeddedResource(GraphicsDevice device, string name)
        {
#if WINRT
            Assembly assembly = typeof(VertexShadow).GetTypeInfo().Assembly;
#else
            Assembly assembly = typeof(VertexShadow).Assembly;
#endif            
            var stream = assembly.GetManifestResourceStream(Prefix + name + Suffix);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return new Effect(device, ms.ToArray());
            }
        }
    }
}
