using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Common;
using Penumbra;

namespace Sandbox
{
    class ScenariosComponent : GameComponent
    {
        private readonly PenumbraComponent _penumbra;
        private readonly PenumbraControllerComponent _penumbraController;

        private Scenario[] _scenarios;
        private int _currentScenarioIndex;

        public ScenariosComponent(SandboxGame game, PenumbraComponent penumbra, PenumbraControllerComponent penumbraController)
            : base(game)
        {
            _penumbra = penumbra;
            _penumbraController = penumbraController;
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadScenarios();
            SwitchScenario();
        }

        public Scenario ActiveScenario { get; private set; }

        public void NextScenario()
        {
            _currentScenarioIndex = (_currentScenarioIndex + 1) % _scenarios.Length;
            SwitchScenario();
        }

        public void PreviousScenario()
        {
            _currentScenarioIndex--;
            if (_currentScenarioIndex == -1)
                _currentScenarioIndex = _scenarios.Length - 1;
            SwitchScenario();
        }

        public override void Update(GameTime gameTime)
        {
            ActiveScenario.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void LoadScenarios()
        {
            _scenarios = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == typeof (Scenario))
                .OrderBy(t => t.Name)
                .Select(t => (Scenario)Activator.CreateInstance(t))
                .ToArray();
            foreach (Scenario scenario in _scenarios)
                scenario.Device = Game.GraphicsDevice;
            _currentScenarioIndex = 0;
        }

        private void SwitchScenario()
        {
            _penumbra.Lights.Clear();
            _penumbra.Hulls.Clear();

            ActiveScenario = _scenarios[_currentScenarioIndex];
            ActiveScenario.Activate(_penumbra, Game.Content);

            foreach (Light light in _penumbra.Lights)
                light.ShadowType = _penumbraController.ActiveShadowType;
        }
    }
}
