using System;

namespace Common.Utilities
{
    /// <summary>
    /// Event argumentss used to pass the FPS info to an event handler.
    /// </summary>
    public class GarbageEventArgs : EventArgs
    {
        /// <summary>
        /// Garbage allocated in the past second in kilobytes.
        /// </summary>
        public float GarbagePerSecond { get; set; }
    }

    /// <summary>
    /// Static class used to display current and average garbage allocations.
    /// </summary>
    public static class Garbage
    {
        private static float _garbageAmountAtLastUpdate;
        private static float _secondsPassedSinceLastSecond;
        private static int _updatesPerSecond;

        /// <summary>
        /// Gets the current amount of allocated garbage in kilobytes.
        /// </summary>
        public static float CurrentAmount { get; private set; }

        /// <summary>
        /// Gets the average amount of garbage created per frame in kilobytes.
        /// </summary>
        public static float CreatedPerFrame { get; private set; }

        /// <summary>
        /// Gets the average amount of garbage created per second in kilobytes.
        /// </summary>
        public static float CreatedPerSecond { get; private set; }

        private static readonly GarbageEventArgs GarbageEventArgs = new GarbageEventArgs();
        public static event EventHandler<GarbageEventArgs> GarbageUpdated;

        /// <summary>
        /// Updated garbage info.
        /// </summary>
        /// <param name="deltaSeconds">Seconds passed since last frame.</param>
        public static void Update(float deltaSeconds)
        {
            _secondsPassedSinceLastSecond += deltaSeconds;
            _updatesPerSecond++;

            if (_secondsPassedSinceLastSecond >= 1f)
            {
                _secondsPassedSinceLastSecond -= 1f;

                long currentGarbageAmount = GC.GetTotalMemory(false);

                // If the garbage collector did not run in the past second, calculate the average amount of garbage created per frame in the past second.
                if (currentGarbageAmount > _garbageAmountAtLastUpdate)
                {
                    CreatedPerSecond = (currentGarbageAmount - _garbageAmountAtLastUpdate) / 1024f;
                    CurrentAmount = currentGarbageAmount / 1024f;
                    CreatedPerFrame = CreatedPerSecond / _updatesPerSecond;
                }

                // Record the current amount of garbage to use to calculate the garbage created per second on the next update.
                _garbageAmountAtLastUpdate = currentGarbageAmount;

                // Reset how many updates have been done in the past second.
                _updatesPerSecond = 0;

                RaiseGarbageUpdated();
            }
        }

        private static void RaiseGarbageUpdated()
        {
            if (GarbageUpdated != null)
            {
                GarbageEventArgs.GarbagePerSecond = CreatedPerSecond;
                GarbageUpdated(null, GarbageEventArgs);
            }
        }
    }
}
