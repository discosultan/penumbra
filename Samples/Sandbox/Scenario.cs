namespace Sandbox
{
    public abstract class Scenario
    {
        public abstract string Name { get; }

        public virtual void Update(float deltaSeconds)
        {            
        }
    }
}
