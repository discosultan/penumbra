using Penumbra;

namespace Sandbox
{
    public abstract class Scenario
    {
        public abstract string Name { get; }

        public abstract void Activate(PenumbraComponent penumbra);

        public virtual void Update(float deltaSeconds)
        {            
        }
    }
}
