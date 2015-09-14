using System;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;

namespace Common
{
    public class PenumbraControllerComponent : DrawableGameComponent
    {
        public const Keys DebugKey = Keys.T;
        public const Keys ShadowTypeKey = Keys.Y;
        public const Keys EnabledKey = Keys.U;

        private SpriteBatch _spriteBatch;

        private readonly PenumbraComponent _penumbra;
        private readonly ShadowType[] ShadowTypes;

        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;
        private int _currentShadowType;        

        public PenumbraControllerComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;
            ShadowTypes = (ShadowType[])Enum.GetValues(typeof(ShadowType));
            penumbra.Lights.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (Light light in e.NewItems)
                        light.ShadowType = ActiveShadowType;
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            SetShadowTypes();
        }

        public ShadowType ActiveShadowType => ShadowTypes[_currentShadowType];

        public override void Update(GameTime gameTime)
        {
            _currentKeyState = Keyboard.GetState();

            if (IsKeyPressed(DebugKey))
                _penumbra.Debug = !_penumbra.Debug;

            if (IsKeyPressed(ShadowTypeKey))
                SetShadowTypes();

            if (IsKeyPressed(EnabledKey))
                _penumbra.Visible = !_penumbra.Visible;

            _previousKeyState = _currentKeyState;
        }

        private void SetShadowTypes()
        {
            _currentShadowType = (_currentShadowType + 1) % ShadowTypes.Length;
            foreach (Light light in _penumbra.Lights)
                light.ShadowType = ActiveShadowType;            
        }

        private bool IsKeyPressed(Keys key)
        {
            return !_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyDown(key);
        }
    }
}
