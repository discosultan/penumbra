using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace Sandbox
{
    public abstract class Scenario
    {
        internal GraphicsDevice Device { get; set; }

        public abstract string Name { get; }

        public abstract void Activate(PenumbraComponent penumbra);

        public virtual void Update(float deltaSeconds)
        {            
        }
    }
}
