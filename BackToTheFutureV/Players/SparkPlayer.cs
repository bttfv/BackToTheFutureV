using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV.Players
{
    public class SparkPlayer : Player
    {
        public float Speed { get; set; }

        public SparkPlayer(TimeMachine timeMachine, IEnumerable<Vector3> frames, Model model, float speed = 20f) : base(timeMachine)
        {            
            _frames = frames.ToList();
            _spark = new AnimateProp(Vehicle, model, _frames[0], Vector3.Zero);

            Speed = speed;
        }

        private AnimateProp _spark;

        private readonly List<Vector3> _frames = new List<Vector3>()
        {
        };

        private int _currentFrame;

        private Vector3 _lastDirection;
        private Vector3 _lastRotation;

        public void UpdateSparkModel(Model model)
        {            
            _spark.SwapModel(model);

            if (_spark.IsSpawned)
                _spark.SpawnProp();
        }

        public override void Dispose()
        {
            _spark.Dispose();
        }

        public override void Play()
        {
            _currentFrame = 1;

            _spark.MoveProp(_frames[0], Utils.DirectionToRotation(_frames[_currentFrame], _spark.CurrentOffset, 0));

            IsPlaying = true;
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            if(_currentFrame > 0)
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

            _lastDirection = (_frames[_currentFrame] - _spark.CurrentOffset).Normalized;
            _lastRotation = Utils.DirectionToRotation(_frames[_currentFrame], _spark.CurrentOffset, 0);

            _spark.MoveProp(_spark.CurrentOffset + _lastDirection * Speed * Game.LastFrameTime, _spark.Rotation + Vector3.Lerp(_spark.CurrentRotation, _lastRotation, Game.LastFrameTime * Speed));
        }

        public override void Stop()
        {
            _currentFrame = 0;
            _spark.Delete();
            IsPlaying = false;
        }
    }
}
