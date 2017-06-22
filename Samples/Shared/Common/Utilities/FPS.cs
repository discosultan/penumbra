using System;
using System.Collections.Generic;

namespace Common.Utilities
{
    /// <summary>
    /// Event argumentss used to pass the FPS info to an event handler.
    /// </summary>
    public class FPSEventArgs : EventArgs
    {
        /// <summary>
        /// Frames per second achieved in the past second.
        /// </summary>
        public int FPS { get; set; }

        /// <summary>
        /// Average frame per Second achieved.
        /// </summary>
        public float AverageFPS { get; set; }
    }

    /// <summary>
    /// Static class used to display the current and average frames per second.
    /// </summary>
    public static class FPS
    {
        // Frames per second variables.
        private static int _fpsCount;
        private static float _timeSinceLastUpdateInSeconds;

        // Average frames per second variables.
        private static int _numberOfSecondsToComputerAverageOver = 10;
        private static readonly Queue<int> FpsQueue = new Queue<int>();
        // Stores the sum of all the FPS numbers in the FPS queue.
        private static int _fpsQueueSum;

        /// <summary>
        /// Event handler that fires every second, directly after the CurrentFPS and AverageFPS have been updated.
        /// </summary>
        public static event EventHandler<FPSEventArgs> FPSUpdated;

        // Event args passed into the event handler.
        // We use a static instance instead of creating a new instance each time for the sake of garbage collection.
        private static readonly FPSEventArgs FPSEventArgs = new FPSEventArgs();

        /// <summary>
        /// Gets the current number of Frames Per Second being achieved.
        /// </summary>
        public static int CurrentFPS { get; private set; }

        /// <summary>
        /// Gets the average number of Frames Per Second being achieved.
        /// </summary>
        public static float AverageFPS { get; private set; }

        /// <summary>
        /// Gets the time in milliseconds it took the render current frame.
        /// </summary>
        public static float CurrentFrameTimeMS => 1000.0f / CurrentFPS;

        /// <summary>
        /// Gets the average time in milliseconds it takes the render a frame.
        /// </summary>
        public static float AverageFrameTimeMS => 1000.0f / AverageFPS;

        /// <summary>
        /// Gets or sets the number of seconds that the average FPS should be computed over.
        /// </summary>
        /// <param name="value">
        /// The number of seconds that the average FPS should be computed over.
        /// NOTE: This must be greater than 1.
        /// </param>
        public static int NumberOfSecondsToComputeAverageOver
        {
            get { return _numberOfSecondsToComputerAverageOver; }
            set
            {
                // If a valid number has been specified.
                if (value > 1) {
                    // Set the number of seconds that the average should be computed over.
                    _numberOfSecondsToComputerAverageOver = value;

                    // Resize the queue if it is needed
                    while (FpsQueue.Count > _numberOfSecondsToComputerAverageOver) {
                        // Remove an item from the end of the FPS queue and subtract it from the queue's sum.
                        _fpsQueueSum -= FpsQueue.Dequeue();
                    }

                    // Calculate the new average FPS.
                    AverageFPS = (float)_fpsQueueSum / FpsQueue.Count;
                }
            }
        }

        /// <summary>
        /// This function should be called every Frame and is used to update how many FPS were achieved.
        /// </summary>
        /// <param name="deltaSeconds">The elapsed time since the last Frame was drawn, in seconds.</param>
        public static void Update(float deltaSeconds)
        {
            // Increment the frames per second counter.
            _fpsCount++;

            // Update the total time since the FPS was last updated.
            _timeSinceLastUpdateInSeconds += deltaSeconds;

            // If one second has passed since the last FPS update.
            if (_timeSinceLastUpdateInSeconds >= 1.0f)
            {
                // Update the number of FPS achieved so that it can be displayed.
                CurrentFPS = _fpsCount;

                // Subtract one second from the time since the Last FPS update.
                _timeSinceLastUpdateInSeconds -= 1.0f;

                // Reset the FPS counter to zero.
                _fpsCount = 0;

                // Store the last seconds FPS count in the FPS queue.
                FpsQueue.Enqueue(CurrentFPS);

                // Add the last seconds FPS count to the FPS queue sum.
                _fpsQueueSum += CurrentFPS;

                // Only store the last numberOfSecondsToComputerAverageOver FPS counts.
                if (FpsQueue.Count > _numberOfSecondsToComputerAverageOver)
                {
                    // Remove the oldest FPS count from the FPS queue and subtract it's value from the queue sum.
                    _fpsQueueSum -= FpsQueue.Dequeue();
                }

                // Calculate the average FPS.
                AverageFPS = (float)_fpsQueueSum / FpsQueue.Count;

                // Let any listeners know that the FPS and average FPS have been updated.
                RaiseFpsUpdated();
            }
        }

        /// <summary>
        /// Function to erase the current FPS values being used to calculate the average and to start over.
        /// </summary>
        public static void ResetAverageFPS()
        {
            // Reset the average FPS variables
            AverageFPS = 0.0f;
            _fpsQueueSum = 0;
            FpsQueue.Clear();
        }

        /// <summary>
        /// Checks if FPS update event has any listeners and raises the event if has any.
        /// </summary>
        private static void RaiseFpsUpdated()
        {
            if (FPSUpdated != null)
            {
                FPSEventArgs.FPS = CurrentFPS;
                FPSEventArgs.AverageFPS = AverageFPS;
                FPSUpdated(null, FPSEventArgs);
            }
        }
    }
}
