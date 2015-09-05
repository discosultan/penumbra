using System;
using Microsoft.Xna.Framework;
using Penumbra;
using Penumbra.Core;

namespace Sandbox.Scenarios
{
    class D_LineOfSight : Scenario
    {
        private enum State
        {
            MovingInward,
            Pause,
            MovingOutward
        }

        private const float MarginFromEdge = 200;
        private const float MovingSecondsOut = 1.5f;
        private const float MovingSecondsIn = 0.5f;
        private const float PauseSeconds = 1f;

        private Hull _hull1, _hull2;
        private State _state;
        private float _progress;        

        public override void Activate(PenumbraComponent penumbra)
        {
            _state = State.MovingInward;
            _progress = 0;

            penumbra.Lights.Add(new Light
            {
                Position = new Vector2(0, -100),
                Color = Color.White,
                Range = 300,
                Radius = 20                
            });

            Vector2[] hullVertices =
            {
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f)            
            };
            _hull1 = new Hull(hullVertices)
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(50f)
            };
            _hull2 = new Hull(hullVertices)
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(50f)
            };
            penumbra.Hulls.Add(_hull1);
            penumbra.Hulls.Add(_hull2);
        }

        public override void Update(float deltaSeconds)
        {
            float halfWidth = Device.Viewport.Width/2f;
            if (_state == State.MovingInward)
            {
                _progress = Math.Min(_progress + deltaSeconds / MovingSecondsIn, 1f);
                _hull1.Position = new Vector2(MathHelper.Lerp(-halfWidth + MarginFromEdge, -_hull1.Scale.X/2f, _progress), 0);
                _hull2.Position = new Vector2(MathHelper.Lerp(halfWidth - MarginFromEdge, _hull2.Scale.X / 2f, _progress), 0);
            }
            else if (_state == State.MovingOutward)
            {
                _progress = Math.Min(_progress + deltaSeconds/MovingSecondsOut, 1f);
                _hull1.Position =
                    new Vector2(MathHelper.Lerp(-_hull1.Scale.X/2f, -halfWidth + MarginFromEdge, _progress), 0);
                _hull2.Position = new Vector2(
                    MathHelper.Lerp(_hull2.Scale.X/2f, halfWidth - MarginFromEdge, _progress), 0);
            }
            else
            {
                _progress += deltaSeconds/PauseSeconds;
            }

            if (_progress >= 1f)
            {
                _state = (State)((int)(_state + 1) % (int)(State.MovingOutward + 1));
                _progress = 0;
            }
        }
    }
}
