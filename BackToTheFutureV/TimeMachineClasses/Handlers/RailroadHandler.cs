using System;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using BackToTheFutureV.Story;
using BackToTheFutureV.Vehicles;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class RailroadHandler : Handler
    {
        public CustomTrain customTrain;

        private bool _direction = false;

        private bool _isReentryOn = false;

        private float _speedDifference;
        private int _checkTime;

        private Vehicle _train;

        public RailroadHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SetRailroadMode += SetRailroadMode;
        }

        public void SetRailroadMode(bool state, bool isReentering = false)
        {
            if (state)
                StartDriving(isReentering);
            else
                StopTrain();
        }

        public void StartDriving(bool isReentering = false)
        {
            if (customTrain != null && customTrain.Exists && isReentering)
            {
                customTrain.SpeedMPH = 88;
                _isReentryOn = true;
                return;
            }

            customTrain = TrainHandler.CreateInvisibleTrain(Vehicle, _direction);

            if (!(customTrain.Train.Heading >= Vehicle.Heading - 45 && customTrain.Train.Heading <= Vehicle.Heading + 45))
            {
                _direction = !_direction;
                customTrain.DeleteTrain();
                customTrain = TrainHandler.CreateInvisibleTrain(Vehicle, _direction);
            }

            customTrain.IsAccelerationOn = true;
            customTrain.IsAutomaticBrakeOn = true;

            customTrain.SetCollision(false);

            customTrain.SetToAttach(Vehicle, new Vector3(0, 4.28f, Vehicle.HeightAboveGround - customTrain.Carriage(1).HeightAboveGround - 0.05f), 1, 0); //new Vector3(0, 4.48f, 0)

            //customTrain.CruiseSpeedMPH = 1;

            customTrain.SetPosition(Vehicle.Position);

            customTrain.OnVehicleAttached += customTrain_OnVehicleAttached;
            customTrain.OnTrainDeleted += customTrain_OnTrainDeleted;

            _isReentryOn = isReentering;
        }

        private void customTrain_OnTrainDeleted()
        {
            Stop();
        }

        private void customTrain_OnVehicleAttached()
        {
            customTrain.DisableToDestroy();

            customTrain.CruiseSpeedMPH = 0;
            customTrain.SpeedMPH = 0;
            customTrain.IsAutomaticBrakeOn = true;

            Properties.IsOnTracks = true;

            if (_isReentryOn)
                customTrain.SpeedMPH = 88;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(Keys key)
        {
            
        }

        public override void Process()
        {
            if (Mods.Wheel != WheelType.RailroadInvisible)
            {
                if (Properties.IsOnTracks)
                    Stop();

                return;
            }

            if (Properties.IsOnTracks)
            {
                Properties.IsAttachedToRogersSierra = customTrain.IsRogersSierra;

                if (Properties.IsAttachedToRogersSierra)
                {
                    if (Main.PlayerVehicle == Vehicle && Game.IsControlPressed(GTA.Control.VehicleAccelerate))
                        customTrain.SwitchToRegular();

                    if (Main.RogersSierra.Locomotive.Speed > 0 && Utils.EntitySpeedVector(Main.RogersSierra.Locomotive).Y < 0)
                        customTrain.SwitchToRegular();

                    return;
                } else
                    customTrain.IsAccelerationOn = Main.PlayerVehicle == Vehicle && Vehicle.IsVisible && Vehicle.IsEngineRunning;

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

                if (_isReentryOn && customTrain.AttachedToTarget && customTrain.SpeedMPH == 0)
                {
                    if (Utils.Random.NextDouble() <= 0.25f)
                        TrainHandler.CreateFreightTrain(Vehicle, !_direction).SetToDestroy(Vehicle, 35);

                    _isReentryOn = false;
                    return;
                }

                return;
            }

            //if (Utils.IsVehicleOnTracks(Vehicle))
            //    StartDriving();

            if (Mods.Wheel == WheelType.RailroadInvisible && (customTrain == null || !customTrain.Exists))
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
            if (Properties.IsAttachedToRogersSierra)
            {
                if (MissionHandler.TrainMission.IsPlaying)
                    MissionHandler.TrainMission.StartExplodingScene();
                else
                    customTrain.SwitchToRegular();
            }                
            else
                customTrain.SpeedMPH = 0;
        }

        public override void Stop()
        {
            _isReentryOn = false;
            Properties.IsOnTracks = false;

            if (customTrain != null)
            {
                if (customTrain.Exists && customTrain.AttachedToTarget)
                    customTrain.DetachTargetVehicle();

                customTrain.OnVehicleAttached -= customTrain_OnVehicleAttached;
                customTrain.OnTrainDeleted -= customTrain_OnTrainDeleted;

                if (customTrain.Exists && !customTrain.IsRogersSierra)
                    customTrain.DeleteTrain();

                customTrain = null;
            }
        }
    }
}
