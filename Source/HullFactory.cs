namespace Penumbra
{
    internal interface IHullFactory<out T>
    {
        T New(HullPart hull);
    }

    internal class CPUHullFactory : IHullFactory<CPUHullPart>
    {
        public CPUHullPart New(HullPart hull)
        {
            return new CPUHullPart(hull);
        }
    }
}
