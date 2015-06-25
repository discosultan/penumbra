using System.Linq;

namespace Penumbra.Graphics
{
    /// <summary>
    /// Collection of <see cref="RenderStep"/>s. Comparable to Effects Framework technique.
    /// </summary>
    internal class RenderProcess
    {
        private readonly RenderStep[] _stepsIncludingDebug;
        private readonly RenderStep[] _steps;

        public RenderProcess(params RenderStep[] steps)
        {
            _stepsIncludingDebug = steps;
            _steps = steps.Where(step => !step.IsDebug).ToArray();
        }

        public RenderStep[] Steps(bool debug)
        {
            return debug
                ? _stepsIncludingDebug
                : _steps;
        }
    }
}
