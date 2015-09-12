using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Penumbra;

namespace Sandbox
{
    class ScenariosComponent : GameComponent
    {
        public event EventHandler ShadowTypeChanged;

        private readonly PenumbraComponent _penumbra;
        private readonly ShadowType[] ShadowTypes;        
        
        private Scenario[] _scenarios;
        private int _currentScenarioIndex;
        private int _currentShadowType;        

        public ScenariosComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;
            ShadowTypes = (ShadowType[])Enum.GetValues(typeof (ShadowType));
        }        

        public override void Initialize()
        {
            base.Initialize();
            SwitchShadowType();
            LoadScenarios();
            SwitchScenario();
        }

        public Scenario ActiveScenario { get; private set; }
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
            _currentScenarioIndex = _scenarios.Length - 1;
        }

        private void SwitchScenario()
        {
            _penumbra.Lights.Clear();
            _penumbra.Hulls.Clear();
            ActiveScenario = _scenarios[_currentScenarioIndex];
            ActiveScenario.Activate(_penumbra);
            SwitchShadowType();
        }

        private void SwitchShadowType()
        {
            foreach (Light light in _penumbra.Lights)
                light.ShadowType = ActiveShadowType;
            ShadowTypeChanged?.Invoke(this, EventArgs.Empty);
        }      
    }
}
