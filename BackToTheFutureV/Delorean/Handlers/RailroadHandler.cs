using System;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using BackToTheFutureV.Story;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class RailroadHandler : Handler
    {
        public TrainHandler trainHandler;

        private bool _direction = false;

        private bool _isReentryOn = false;

        private float _speedDifference;
        private int _checkTime;

        private Vehicle _train;

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

            trainHandler.SetCollision(false);

            trainHandler.SetToAttach(Vehicle, new Vector3(0, 4.28f, 0), 1, 0); //new Vector3(0, 4.48f, 0)

            //trainHandler.CruiseSpeedMPH = 1;

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

            trainHandler.CruiseSpeedMPH = 0;
            trainHandler.SpeedMPH = 0;
            trainHandler.IsAutomaticBrakeOn = true;

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

        public override void Process()
        {
            if (Mods.Wheel != WheelType.RailroadInvisible)
            {
                if (IsOnTracks)
                    Stop();

                return;
            }

            if (IsOnTracks)
            {
                IsAttachedToRogersSierra = trainHandler.IsRogersSierra;

                if (IsAttachedToRogersSierra)
                {
                    if (Main.PlayerVehicle == Vehicle && Game.IsControlPressed(GTA.Control.VehicleAccelerate))
                        trainHandler.SwitchToRegular();

                    if (Main.RogersSierra.Locomotive.Speed > 0 && Utils.EntitySpeedVector(Main.RogersSierra.Locomotive).Y < 0)
                        trainHandler.SwitchToRegular();

                    return;
                } else
                    trainHandler.IsAccelerationOn = Main.PlayerVehicle == Vehicle && Vehicle.IsVisible && Vehicle.IsEngineRunning;

                if (Main.PlayerVehicle == Vehicle)
                    Function.Call(Hash.DISABLE_CONTROL_ACTION, 27, 59, true);

                //if (Game.GameTime > _checkTime)
                //{
                //    _checkTime = Game.GameTime + 1000;

                //    _train = World.GetClosestVehicle(Vehicle.Position, 25, ModelHandler.FreightModel, ModelHandler.SierraModel, ModelHandler.SierraTenderModel, ModelHandler.SierraDebugModel);

                //    if (_train != null)
                //        _speedDifference = Math.Abs(_train.GetMPHSpeed() - Vehicle.GetMPHSpeed());
                //    else
                //        _train = null;

                //}

                //if (Vehicle.IsTouching(_train))
                //{
                //    Stop();

                //    if (_speedDifference > 20)
                //        Vehicle.Explode();

                //    return;
                //}

                if (_isReentryOn && trainHandler.AttachedToTarget && trainHandler.SpeedMPH == 0)
                {
                    if (Utils.Random.NextDouble() <= 0.25f)
                        TrainManager.CreateFreightTrain(Vehicle, !_direction).SetToDestroy(Vehicle, 35);

                    _isReentryOn = false;
                    return;
                }

                return;
            }

            //if (Utils.IsVehicleOnTracks(Vehicle))
            //    StartDriving();

            if (Mods.Wheel == WheelType.RailroadInvisible && (trainHandler == null || !trainHandler.Exists))
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

        public void StopTrain()
        {
            if (IsAttachedToRogersSierra)
            {
                if (MissionHandler.TrainMission.IsPlaying)
                    MissionHandler.TrainMission.StartExplodingScene();
                else
                    trainHandler.SwitchToRegular();
            }                
            else
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

                if (trainHandler.Exists && !trainHandler.IsRogersSierra)
                    trainHandler.DeleteTrain();

                trainHandler = null;
            }
        }
    }
}
