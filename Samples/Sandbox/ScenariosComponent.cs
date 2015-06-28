using System;
using Microsoft.Xna.Framework;
using Penumbra;
using Sandbox.Scenarios;

namespace Sandbox
{
    class ScenariosComponent : GameComponent
    {               
        private static readonly ShadowType[] ShadowTypes = {ShadowType.Illuminated, ShadowType.Solid/*, ShadowType.Occluded*/};

        private readonly PenumbraComponent _penumbra;

        private const int NumScenarios = 2;
        private Scenario _activeScenario;
        private int _currentScenario = 1;

        private int _currentShadowType;

        public event EventHandler ShadowTypeChanged;

        public ScenariosComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;
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
            _currentScenario = (_currentScenario + 1) % NumScenarios;
            SwitchScenario();
        }        

        public void PreviousScenario()
        {
            _currentScenario--;
            if (_currentScenario == -1)
                _currentScenario = NumScenarios - 1;
            SwitchScenario();
        }

        public override void Update(GameTime gameTime)
        {            
            _activeScenario.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void SwitchShadowType()
        {
            foreach (Light light in _penumbra.Lights)
            {
                light.ShadowType = ActiveShadowType;
            }
            ShadowTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SwitchScenario()
        {
            _penumbra.Lights.Clear();
            _penumbra.Hulls.Clear();
            switch (_currentScenario)
            {
                default:
                    _activeScenario = new SimpleRotation(_penumbra);
                    break;
                case 1:
                    _activeScenario = new PassThrough(_penumbra);
                    break;
            }
            SwitchShadowType();            
        }        
    }
}
