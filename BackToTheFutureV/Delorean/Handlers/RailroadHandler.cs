using System;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class RailroadHandler : Handler
    {
        private TrainHandler trainHandler;

        private bool _direction = false;

        private bool _isReentryOn = false;

        private float _speedDifference;
        private int _checkTime;

        public RailroadHandler(TimeCircuits circuits) : base(circuits)
        {
            
        }

        public void StartDriving(bool isReentering = false)
        {
            if (trainHandler != null && trainHandler.Exists && isReentering)
            {                                
                trainHandler.SpeedMPH = 88;
                _isReentryOn = true;
                return;
            }

            trainHandler = TrainManager.CreateInvisibleTrain(Vehicle, _direction);

            if (!(trainHandler.Train.Heading >= Vehicle.Heading - 45 && trainHandler.Train.Heading <= Vehicle.Heading + 45))
            {
                _direction = !_direction;
                trainHandler.DeleteTrain();
                trainHandler = TrainManager.CreateInvisibleTrain(Vehicle, _direction);
            }

            trainHandler.IsAccelerationOn = true;
            trainHandler.IsAutomaticBrakeOn = true;
            trainHandler.SetToAttach(Vehicle, Vector3.Zero, 1, 0);
            trainHandler.SetToDestroy(1);

            trainHandler.SetPosition(Vehicle.Position);

            trainHandler.OnVehicleAttached += TrainHandler_OnVehicleAttached;
            trainHandler.OnTrainDeleted += TrainHandler_OnTrainDeleted;

            _isReentryOn = isReentering;
        }

        private void TrainHandler_OnTrainDeleted()
        {
            Stop();
        }

        private void TrainHandler_OnVehicleAttached()
        {
            trainHandler.DisableToDestroy();
            IsOnTracks = true;

            if (_isReentryOn)
                trainHandler.SpeedMPH = 88;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyPress(Keys key)
        {
            
        }

        private void UpdateAttachOffset()
        {
            trainHandler.AttachOffset = trainHandler.Carriage(1).GetPositionOffset(trainHandler.Train.Position);
            trainHandler.AttachOffset.Z -= 1.35f;
        }

        public override void Process()
        {
            if (trainHandler != null && trainHandler.Exists)
            {
                UpdateAttachOffset();

                trainHandler.IsAccelerationOn = Main.PlayerVehicle == Vehicle && Vehicle.IsVisible && Vehicle.IsEngineRunning;
                trainHandler.Carriage(trainHandler.CarriageIndexForAttach).IsCollisionEnabled = trainHandler.IsAccelerationOn;
            }
                
            if (IsOnTracks)
            {
                if (RogersSierra.Manager.RogersSierra != null && RogersSierra.Manager.RogersSierra.isDeLoreanAttached && RogersSierra.Manager.RogersSierra.AttachedDeLorean == Vehicle)
                    return;

                if (Game.GameTime > _checkTime)
                {
                    _checkTime = Game.GameTime + 1000;

                    var train = World.GetClosestVehicle(Vehicle.Position, 25, ModelHandler.FreightModel);
                    if (train != null)
                    {
                        // Speed difference between delorean and train
                        var trainSpeed = train.Speed;
                        _speedDifference = Math.Abs(trainSpeed - Vehicle.Speed);
                    }
                }

                if (Function.Call<bool>(Hash.IS_ENTITY_TOUCHING_MODEL, Vehicle, ModelHandler.FreightModel))
                {
                    Stop();

                    if (_speedDifference > 20)
                        Vehicle.Explode();

                    return;
                }

                if (!trainHandler.AttachedToTarget || Mods.Wheel != WheelType.RailroadInvisible)
                {
                    Stop();
                    return;
                }

                if (_isReentryOn && trainHandler.AttachedToTarget && trainHandler.SpeedMPH == 0)
                {
                    if (Utils.Random.NextDouble() <= 0.25f)
                        TrainManager.CreateFreightTrain(Vehicle, !_direction).SetToDestroy(Vehicle, 35);

                    _isReentryOn = false;
                    return;
                }

                if (Main.PlayerVehicle == Vehicle)
                    Function.Call(Hash.DISABLE_CONTROL_ACTION, 27, 59, true);

                return;
            }

            if (Mods.Wheel == WheelType.RailroadInvisible && (trainHandler == null || !trainHandler.Exists))
            {
                if (Main.PlayerVehicle == Vehicle && Main.PlayerVehicle.Speed < 1)
                {
                    var wheelPos = new List<Vector3>
                    {
                        Vehicle.Bones["wheel_lf"].Position,
                        Vehicle.Bones["wheel_rf"].Position,
                        Vehicle.Bones["wheel_rr"].Position,
                        Vehicle.Bones["wheel_lr"].Position
                    };

                    if (wheelPos.TrueForAll(x => Utils.IsWheelOnTracks(x, Vehicle)))
                        StartDriving();
                }
            }
        }

        public void StopTrain()
        {
            trainHandler.SpeedMPH = 0;
        }

        public override void Stop()
        {
            _isReentryOn = false;
            IsOnTracks = false;

            if (trainHandler != null)
            {
                if (trainHandler.Exists && trainHandler.AttachedToTarget)
                    trainHandler.DetachTargetVehicle();

                trainHandler.OnVehicleAttached -= TrainHandler_OnVehicleAttached;
                trainHandler.OnTrainDeleted -= TrainHandler_OnTrainDeleted;

                if (trainHandler.Exists)
                    trainHandler.DeleteTrain();
            }
        }
    }
}
