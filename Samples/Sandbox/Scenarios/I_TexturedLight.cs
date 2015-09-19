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

        public override void Activate(PenumbraComponent penumbra, ContentManager content)
        {
            _light = new TexturedLight
            {           
				Position = new Vector2(0, -400),
				Origin = new Vector2(0, -400),
                Color = Color.Cornsilk,
                Range = 400,                				
				Texture = content.Load<Texture2D>("LightTexture"),
				TextureTransform = Matrix.CreateTranslation(new Vector3(0, -0.5f, 0)) * Matrix.CreateRotationZ(MathHelper.PiOver2)
            };
            penumbra.Lights.Add(_light);
			_hull = new Hull(new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) })
			{
				Position = Vector2.Zero,
				Scale = new Vector2(50)
			};
            penumbra.Hulls.Add(_hull);
        }

        public override void Update(float deltaSeconds)
        {
            //float angle = deltaSeconds*RotationSpeed;
            //var s = (float) Sin(angle);
            //var c = (float) Cos(angle);

            //_light.Position = new Vector2(
            //    _light.Position.X * c - _light.Position.Y * s,
            //    _light.Position.X * s + _light.Position.Y * c
            //);
        }
    }
}
