using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class EmitterSparkPlayer : Players.Player
    {
        private Vector3 _offset;

        private Vector3 _destination;

        private AnimateProp _spark;

        private SparkType _sparkType;

        private float _speed;

        public EmitterSparkPlayer(TimeMachine timeMachine, SparkType sparkType) : base(timeMachine)
        {
            _sparkType = sparkType;

            switch (_sparkType)
            {
                case SparkType.WHE:
                    _offset = new Vector3(0, 0, 1.1f);
                    break;
                case SparkType.Left:
                    _offset = new Vector3(-0.96f, -0.8f, 0.75f);
                    break;
                case SparkType.Right:
                    _offset = new Vector3(0.96f, -0.8f, 0.75f);
                    break;
            }

            _spark = new AnimateProp(Constants.SparkModel, Vehicle, _offset, Vector3.Zero);

            if (Mods.IsDMC12)
                _destination = Vehicle.Bones["bttf_wormhole"].RelativePosition;
            else
                _destination = new Vector3(0, Vehicle.Model.Dimensions.frontTopRight.Y + 1, 0.4f);

            _destination.Y += 0.8f;

            switch (_sparkType)
            {
                case SparkType.Left:
                    _destination.X -= 0.5f;
                    break;
                case SparkType.Right:
                    _destination.X += 0.5f;
                    break;
            }
        }

        public override void Dispose()
        {
            _spark?.Dispose();
        }

        public override void Play()
        {
            _speed = (float)FusionUtils.Random.NextDouble(15, 21);

            Vector3 dir = _offset.GetDirectionTo(_destination) * Game.LastFrameTime * _speed;
            Vector3 pos = _offset + dir;
            Vector3 rot = FusionUtils.DirectionToRotation(pos, _offset, 0);

            _spark.MoveProp(_offset, rot);

            _spark.Visible = FusionUtils.Random.Next(1, 11) <= 5;

            IsPlaying = true;
        }

        public override void Stop()
        {
            _spark?.Delete();
            IsPlaying = false;
        }

        public override void Tick()
        {
            if (!IsPlaying)
                return;

            Vector3 dir = _spark.CurrentOffset.GetDirectionTo(_destination) * Game.LastFrameTime * _speed;
            Vector3 pos = _spark.CurrentOffset + dir;
            Vector3 rot = FusionUtils.DirectionToRotation(pos, _spark.CurrentOffset, 0);

            _spark.MoveProp(pos, Vector3.Lerp(_spark.CurrentRotation, rot, Game.LastFrameTime * _speed));

            if (pos.DistanceToSquared2D(_destination) <= 0.08f)
            {
                _spark.MoveProp(_offset, FusionUtils.DirectionToRotation(_spark.CurrentOffset, _offset, 0));
                _spark.Visible = FusionUtils.Random.Next(1, 11) <= 5;
                _speed = (float)FusionUtils.Random.NextDouble(15, 21);
            }
        }
    }
}
