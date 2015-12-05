using System;
using System.Linq;
using System.Reflection;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using QuakeConsole;

namespace Sandbox
{
    class ScenariosComponent : DrawableGameComponent
    {
        private static readonly Color BackgroundColor = Color.White;
        private static readonly Color DefaultNormals = new Color(new Vector4(0.5f, 0.5f, 1.0f, 0.0f));

        private readonly SandboxGame _game;
        private readonly PenumbraComponent _penumbra;
        private readonly PenumbraControllerComponent _penumbraController;
        private readonly PythonInterpreter _interpreter;
        private readonly CameraMovementComponent _camera;

        private PrimitiveRenderer _renderer;
        private Scenario[] _scenarios;
        private int _currentScenarioIndex;        

        public ScenariosComponent(SandboxGame game, PenumbraComponent penumbra, PenumbraControllerComponent penumbraController, 
            PythonInterpreter interpreter, CameraMovementComponent camera) 
            : base(game)
        {
            _game = game;
            _penumbra = penumbra;
            _penumbraController = penumbraController;
            _interpreter = interpreter;
            _camera = camera;
        }

        public Scenario ActiveScenario { get; private set; }

        public override void Initialize()
        {
            base.Initialize();            
            LoadScenarios();
            SwitchScenario();
            _renderer = new PrimitiveRenderer(GraphicsDevice, Game.Content);
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

        public override void Draw(GameTime gameTime)
        {
            _renderer.Transform = _camera.Transform * _game.Projection;

            if (_penumbra.NormalMappedLightingEnabled)
            {
                _penumbra.BeginNormalMapped();
                GraphicsDevice.Clear(DefaultNormals);
                ActiveScenario.DrawNormals(_renderer);
            }

            _penumbra.BeginDraw();
            GraphicsDevice.Clear(BackgroundColor);
            ActiveScenario.DrawDiffuse(_renderer);
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
            _penumbra.NormalMappedLightingEnabled = false;
            _interpreter.Reset();
            ActiveScenario = _scenarios[_currentScenarioIndex];
            ActiveScenario.Activate(_penumbra, Game.Content);
            for (int i = 0; i < _penumbra.Lights.Count; i++)
            {
                Light light = _penumbra.Lights[i];
                light.ShadowType = _penumbraController.ActiveShadowType;
                _interpreter.AddVariable($"light{i}", light);
            }
            for (int i = 0; i < _penumbra.Hulls.Count; i++)
            {
                Hull hull = _penumbra.Hulls[i];
                _interpreter.AddVariable($"hull{i}", hull);
            }
        }
    }
}
