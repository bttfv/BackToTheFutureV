using BackToTheFutureV.Delorean;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Players
{
    public class SparkPlayer : Player
    {
        public Vehicle Vehicle { get; }

        public float Speed { get; set; }

        public SparkPlayer(Vehicle vehicle, IEnumerable<Vector3> frames, Model model, float speed = 20f)
        {
            Vehicle = vehicle;

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
            _spark.Model = model;

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
            _spark.Offset = _frames[0];
            _spark.Rotation = Utils.DirectionToRotation(_frames[_currentFrame], _spark.Offset, 0);
            _spark.SpawnProp();
            IsPlaying = true;
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            if(_currentFrame > 0)
            {
                float totalLengthSquared = (_frames[_currentFrame - 1] - _frames[_currentFrame]).LengthSquared();
                float lengthToSpark = (_frames[_currentFrame - 1] - _spark.Offset).LengthSquared();

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

            _lastDirection = (_frames[_currentFrame] - _spark.Offset).Normalized;
            _lastRotation = Utils.DirectionToRotation(_frames[_currentFrame], _spark.Offset, 0);

            _spark.Offset += _lastDirection * Speed * Game.LastFrameTime;
            _spark.Rotation = Vector3.Lerp(_spark.Rotation, _lastRotation, Game.LastFrameTime * Speed);
            _spark.SpawnProp(false);
        }

        public override void Stop()
        {
            _currentFrame = 0;
            _spark.DeleteProp();
            IsPlaying = false;
        }
    }
}
