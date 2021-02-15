using BackToTheFutureV.Story;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class RailroadHandler : Handler
    {
        public CustomTrain customTrain;

        private bool _direction = false;

        private bool _isReentryOn = false;

        private int _attachDelay;

        private float _speed;

        private bool _forceFreightTrain;

        public RailroadHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SetWheelie += SetWheelie;
            Events.OnTimeTravelStarted += OnTimeTravelStarted;
            Events.OnReenterEnded += OnReenterEnded;
            Events.SetStopTracks += Stop;
        }

        public void SetWheelie(bool goUp)
        {
            customTrain?.SetWheelie?.Invoke(goUp);
        }

        public void OnTimeTravelStarted()
        {
            Properties.WasOnTracks = Properties.IsOnTracks;

            if (!Properties.IsOnTracks)
                return;

            if (customTrain.IsRogersSierra)
            {
                _forceFreightTrain = customTrain.RogersSierra.IsOnTrainMission;

                if (Properties.TimeTravelType == TimeTravelType.Instant && customTrain.RogersSierra.IsOnTrainMission)
                    MissionHandler.TrainMission.End();

                customTrain.RogersSierra.RejectAttach = true;

                Stop();
                return;
            }

            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Cutscene:
                    customTrain.SpeedMPH = 0;
                    break;
                case TimeTravelType.RC:
                    Stop();
                    break;
            }
        }

        public void OnReenterEnded()
        {
            if (!Properties.WasOnTracks)
                return;

            _isReentryOn = true;

            if (customTrain == null || !customTrain.Exists)
                Start(true);

            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Instant:
                    if (customTrain.SpeedMPH == 0)
                        customTrain.SpeedMPH = 88;
                    break;
                case TimeTravelType.Cutscene:
                    customTrain.SpeedMPH = 88;
                    break;
                case TimeTravelType.RC:
                    Start(true);
                    break;
            }
        }

        public void Start(bool force = false)
        {
            if (!force && !Vehicle.IsOnAllWheels)
                return;

            _speed = Vehicle.Speed;

            customTrain = CustomTrainHandler.CreateInvisibleTrain(Vehicle, _direction);

            if (!customTrain.Train.Heading.Near(Vehicle.Heading))
            {
                _direction = !_direction;
                customTrain.DeleteTrain();
                customTrain = CustomTrainHandler.CreateInvisibleTrain(Vehicle, _direction);

                if (!customTrain.Train.SameDirection(Vehicle))
                {
                    customTrain?.DeleteTrain();
                    customTrain = null;
                    return;
                }
            }

            customTrain.IsAccelerationOn = true;
            customTrain.IsAutomaticBrakeOn = true;

            customTrain.SetCollision(false);

            customTrain.SetToAttach(Vehicle, new Vector3(0, 4.5f, Mods.IsDMC12 ? 0 : Vehicle.HeightAboveGround - customTrain.Carriage(1).HeightAboveGround), 1, 0);

            customTrain.SetPosition(Vehicle.GetOffsetPosition(offset: Vector3.Zero.GetSingleOffset(Coordinate.Y, -1)));

            customTrain.OnVehicleAttached += customTrain_OnVehicleAttached;
            customTrain.OnTrainDeleted += customTrain_OnTrainDeleted;
        }

        private void customTrain_OnTrainDeleted()
        {
            customTrain.OnTrainDeleted -= customTrain_OnTrainDeleted;
            customTrain.OnVehicleAttached -= customTrain_OnVehicleAttached;
            customTrain = null;

            if (Properties.IsAttachedToRogersSierra)
                Start();
            else
                Stop();
        }

        private void customTrain_OnVehicleAttached(bool toRogersSierra = false)
        {
            customTrain.DisableToDestroy();

            customTrain.CruiseSpeedMPH = 0;
            customTrain.SpeedMPH = 0;
            customTrain.IsAutomaticBrakeOn = true;

            Properties.IsOnTracks = true;

            if (_isReentryOn)
            {
                if (customTrain.SpeedMPH == 0)
                    customTrain.SpeedMPH = 88;
            }
            else
                customTrain.Speed = Vehicle.IsGoingForward() ? _speed : -_speed;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(Keys key)
        {
            if (!Properties.IsOnTracks || !customTrain.IsRogersSierra)
                return;

            customTrain.RogersSierra.KeyDown(key);
        }

        public override void Process()
        {
            if (Mods.Wheel != WheelType.RailroadInvisible)
            {
                if (Properties.IsOnTracks)
                    Stop();

                return;
            }

            if (Properties.IsOnTracks && customTrain == null)
                Stop();

            if (Properties.IsOnTracks)
            {
                Properties.IsAttachedToRogersSierra = customTrain.IsRogersSierra;

                if (Properties.IsAttachedToRogersSierra)
                {
                    if (Game.IsControlPressed(GTA.Control.VehicleAccelerate) && Utils.PlayerVehicle == Vehicle && Vehicle.IsEngineRunning && !customTrain.RogersSierra.IsOnTrainMission)
                    {
                        customTrain.SwitchToRegular();
                        return;
                    }

                    if (customTrain.RogersSierra.Locomotive.Speed > 0 && !customTrain.RogersSierra.Locomotive.IsGoingForward())
                        customTrain.SwitchToRegular();

                    return;
                }
                else
                    customTrain.IsAccelerationOn = Vehicle.IsPlayerDriving() && Vehicle.IsVisible && Vehicle.IsEngineRunning;

                if (Utils.PlayerVehicle == Vehicle)
                    Function.Call(Hash.DISABLE_CONTROL_ACTION, 27, 59, true);

                if (_isReentryOn && customTrain.AttachedToTarget && customTrain.SpeedMPH == 0)
                {
                    if (_forceFreightTrain || Utils.Random.NextDouble() <= 0.25f)
                        CustomTrainHandler.CreateFreightTrain(Vehicle, !_direction).SetToDestroy(Vehicle, 35);

                    _isReentryOn = false;
                    _forceFreightTrain = false;
                    return;
                }

                if (Properties.MissionType == MissionType.Train)
                    return;

                Vehicle _train = World.GetClosestVehicle(Vehicle.Position, 25, ModelHandler.FreightModel, ModelHandler.FreightCarModel, ModelHandler.TankerCarModel, ModelHandler.SierraDebugModel, ModelHandler.SierraModel, ModelHandler.SierraTenderModel);

                if (Vehicle.IsVisible && _train != null && Vehicle.IsTouching(_train))
                {
                    Stop();

                    if (Vehicle.SameDirection(_train))
                    {
                        if (Math.Abs(_train.GetMPHSpeed() + Vehicle.GetMPHSpeed()) > 35)
                            Vehicle.Explode();
                    }
                    else
                        if (Math.Abs(_train.GetMPHSpeed() - Vehicle.GetMPHSpeed()) > 35)
                        Vehicle.Explode();

                    _attachDelay = Game.GameTime + 3000;
                }

                return;
            }

            if (_attachDelay < Game.GameTime && Mods.Wheel == WheelType.RailroadInvisible && !Properties.IsFlying)
            {
                if (Utils.GetWheelsPositions(Vehicle).TrueForAll(x => Utils.IsWheelOnTracks(x, Vehicle)))
                {
                    customTrain?.DeleteTrain();
                    Start(TimeMachine.Properties.TimeTravelPhase == TimeTravelPhase.Reentering);
                }
            }
        }

        public override void Stop()
        {
            Stop(0);
        }

        public void Stop(int delay = 0)
        {
            _isReentryOn = false;
            Properties.IsOnTracks = false;

            if (delay > 0)
                _attachDelay = Game.GameTime + delay;

            if (customTrain != null)
            {
                if (customTrain.Exists && customTrain.AttachedToTarget)
                    customTrain.DetachTargetVehicle();

                customTrain.OnVehicleAttached -= customTrain_OnVehicleAttached;
                customTrain.OnTrainDeleted -= customTrain_OnTrainDeleted;

                if (customTrain.IsRogersSierra)
                    customTrain.SwitchToRegular();

                if (customTrain.Exists && !customTrain.IsRogersSierra)
                    customTrain.DeleteTrain();

                customTrain = null;
            }
        }
    }
}
