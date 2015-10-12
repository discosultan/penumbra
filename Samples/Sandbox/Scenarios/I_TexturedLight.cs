using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace Sandbox.Scenarios
{
    class I_TexturedLight : Scenario
    {
        private TexturedLight _light;
        private Hull _hull;

        private const float MarginFromEdge = 200;
        private const float MovingSeconds = 2;

        private bool _movingUp;
        private float _progress;

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            Texture2D tex = content.Load<Texture2D>("LightTexture");
            _light = new TexturedLight(tex)
            {           
				Position = new Vector2(-penumbra.GraphicsDevice.Viewport.Width / 2f, 0),
				Origin = new Vector2(0.5f, 1),
                Color = Color.White,                                
                Radius = 150,
                Rotation = MathHelper.PiOver2                
            };
            penumbra.Lights.Add(_light);
			_hull = new Hull(new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f))
			{
				Position = Vector2.Zero,
				Scale = new Vector2(50)
			};
            penumbra.Hulls.Add(_hull);
        }

        public override void Update(float deltaSeconds)
        {
            float halfHeight = Device.Viewport.Height / 2f;

            _progress += deltaSeconds / MovingSeconds;

            float y = _movingUp
                ? MathHelper.Lerp(-halfHeight + MarginFromEdge, halfHeight - MarginFromEdge, _progress)
                : MathHelper.Lerp(halfHeight - MarginFromEdge, -halfHeight + MarginFromEdge, _progress);
            _hull.Position = new Vector2(_hull.Position.X, y);

            if (_progress >= 1)
            {
                _progress = 0;
                _movingUp = !_movingUp;
            }
        }
    }
}
