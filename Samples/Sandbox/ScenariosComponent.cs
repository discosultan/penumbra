using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Penumbra;
using Sandbox.Scenarios;

namespace Sandbox
{
    class ScenariosComponent : GameComponent
    {               
        private static readonly ShadowType[] ShadowTypes = {ShadowType.Illuminated, ShadowType.Solid, ShadowType.Occluded};

        private readonly PenumbraComponent _penumbra;

        private const int NumScenarios = 3;
        private Scenario[] _scenarios;
        private Scenario _activeScenario;
        private int _currentScenarioIndex;

        private int _currentShadowType;

        public event EventHandler ShadowTypeChanged;

        public ScenariosComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;
            LoadScenarios();
            SwitchScenario();
        }        

        public override void Initialize()
        {
            base.Initialize();
            SwitchShadowType();
        }

        public Scenario ActiveScenario => _activeScenario;
        public ShadowType ActiveShadowType => ShadowTypes[_currentShadowType];

        public void NextShadowType()
        {
            _currentShadowType = (_currentShadowType + 1) % ShadowTypes.Length;
            SwitchShadowType();
        }        

        public void PreviousShadowType()
        {
            _currentShadowType--;
            if (_currentShadowType == -1)
                _currentShadowType = ShadowTypes.Length - 1;
            SwitchShadowType();
        }

        public void NextScenario()
        {
            _currentScenarioIndex = (_currentScenarioIndex + 1) % NumScenarios;
            SwitchScenario();
        }        

        public void PreviousScenario()
        {
            _currentScenarioIndex--;
            if (_currentScenarioIndex == -1)
                _currentScenarioIndex = NumScenarios - 1;
            SwitchScenario();
        }

        public override void Update(GameTime gameTime)
        {            
            _activeScenario.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void LoadScenarios()
        {
            _scenarios = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == typeof (Scenario))
                .OrderBy(t => t.Name)
                .Select(t => (Scenario)Activator.CreateInstance(t))
                .ToArray();
            _currentScenarioIndex = _scenarios.Length - 1;
        }

        private void SwitchScenario()
        {
            _penumbra.Lights.Clear();
            _penumbra.Hulls.Clear();
            _activeScenario = _scenarios[_currentScenarioIndex];
            _activeScenario.Activate(_penumbra);
            SwitchShadowType();
        }

        private void SwitchShadowType()
        {
            foreach (Light light in _penumbra.Lights)
            {
                light.ShadowType = ActiveShadowType;
            }
            ShadowTypeChanged?.Invoke(this, EventArgs.Empty);
        }      
    }
}
