using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal class SparkPlayer : Players.Player
    {
        public float Speed { get; set; } = 20f;

        public SparkPlayer(TimeMachine timeMachine, IEnumerable<Vector3> frames) : base(timeMachine)
        {
            _frames = frames.ToList();
            _spark = new AnimateProp(Constants.SparkModel, Vehicle, _frames[0], Vector3.Zero);
        }

        private AnimateProp _spark;

        private readonly List<Vector3> _frames = new List<Vector3>();

        private int _currentFrame;

        private Vector3 _lastDirection;
        private Vector3 _lastRotation;

        public override void Dispose()
        {
            _spark.Dispose();
        }

        public override void Play()
        {
            _currentFrame = 1;

            _spark.MoveProp(_frames[0], FusionUtils.DirectionToRotation(_frames[_currentFrame], _frames[0], 0));

            IsPlaying = true;
        }

        public override void Tick()
        {
            if (!IsPlaying)
                return;

            if (_currentFrame > 0)
            {
                float totalLengthSquared = (_frames[_currentFrame - 1] - _frames[_currentFrame]).LengthSquared();
                float lengthToSpark = (_frames[_currentFrame - 1] - _spark.CurrentOffset).LengthSquared();

                if (lengthToSpark > totalLengthSquared)
                {
                    _currentFrame++;

                    // All frames done
                    if (_currentFrame > (_frames.Count - 1))
                    {
                        Stop();

                        return;
                    }
                }
            }

            _lastDirection = _spark.CurrentOffset.GetDirectionTo(_frames[_currentFrame]);
            _lastRotation = FusionUtils.DirectionToRotation(_frames[_currentFrame], _spark.CurrentOffset, 0);

            _spark.MoveProp(_spark.CurrentOffset + _lastDirection * Speed * Game.LastFrameTime, Vector3.Lerp(_spark.CurrentRotation, _lastRotation, Game.LastFrameTime * Speed));
        }

        public override void Stop()
        {
            _currentFrame = 0;
            _spark.Delete();
            IsPlaying = false;
        }
    }
}
