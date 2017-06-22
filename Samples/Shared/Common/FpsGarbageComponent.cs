using System.Globalization;
using Common.Utilities;
using Microsoft.Xna.Framework;

namespace Common
{
    public class FpsGarbageComponent : GameComponent
    {
        private int _fps;
        private float _garbage;
        private bool _fpsOrGarbageDirty;

        public FpsGarbageComponent(Game game) : base(game)
        {
            FPS.FPSUpdated += (s, e) => { _fps = e.FPS; _fpsOrGarbageDirty = true; };
            Garbage.GarbageUpdated += (s, e) => { _garbage = e.GarbagePerSecond; _fpsOrGarbageDirty = true; };

            DisableFpsLimit();

            Enabled = true;
        }

        public void DisableFpsLimit()
        {
            Game.IsFixedTimeStep = false;
            var deviceManager = (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>();
            deviceManager.SynchronizeWithVerticalRetrace = false;
        }

        public override void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            FPS.Update(deltaSeconds);
            Garbage.Update(deltaSeconds);

            if (_fpsOrGarbageDirty)
            {
                _fpsOrGarbageDirty = false;
                Game.Window.Title = $"FPS {_fps} | Garbage per sec KB {_garbage.ToString(CultureInfo.InvariantCulture)}";
            }
        }
    }
}
