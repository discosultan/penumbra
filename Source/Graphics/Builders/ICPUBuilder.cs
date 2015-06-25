namespace Penumbra.Graphics.Builders
{
    internal interface ICPUBuilder
    {
        void ProcessHullPoint(Light light, CPUHullPart hull, ref PointProcessingContext context);
    }
}
