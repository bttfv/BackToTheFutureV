using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class RailroadHandler : HandlerPrimitive
    {
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
            Events.StartTrain += Start;
            Events.SetTrainSpeed += SetTrainSpeed;
        }

        public void SetTrainSpeed(float speed)
        {
            if (!Properties.IsOnTracks || CustomTrain == null || !CustomTrain.Exists)
                return;

            CustomTrain.Speed = speed;
        }

        public void SetWheelie(bool goUp)
        {
            CustomTrain?.SetWheelie?.Invoke(goUp);
        }

        public void OnTimeTravelStarted()
        {
            Properties.WasOnTracks = Properties.IsOnTracks;

            if (!Properties.IsOnTracks)
                return;

            if (CustomTrain.IsRogersSierra)
            {
                _forceFreightTrain = CustomTrain.RogersSierra.IsOnTrainMission;

                if (Properties.TimeTravelType == TimeTravelType.Instant && CustomTrain.RogersSierra.IsOnTrainMission)
                    MissionHandler.TrainMission.End();

                CustomTrain.RogersSierra.RejectAttach = true;

                Stop();
                return;
            }

            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Cutscene:
                    CustomTrain.SpeedMPH = 0;
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

            if (CustomTrain == null || !CustomTrain.Exists)
                Start(true);

            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Instant:
                    if (CustomTrain.SpeedMPH == 0)
                        CustomTrain.SpeedMPH = 88;
                    break;
                case TimeTravelType.Cutscene:
                    CustomTrain.SpeedMPH = 88;
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

            CustomTrain = CustomTrainHandler.CreateInvisibleTrain(Vehicle, _direction);

            if (!CustomTrain.Train.Heading.Near(Vehicle.Heading))
            {
                _direction = !_direction;
                CustomTrain.DeleteTrain();
                CustomTrain = CustomTrainHandler.CreateInvisibleTrain(Vehicle, _direction);

                if (!CustomTrain.Train.SameDirection(Vehicle))
                {
                    CustomTrain?.DeleteTrain();
                    CustomTrain = null;
                    return;
                }
            }

            CustomTrain.IsAccelerationOn = true;
            CustomTrain.IsAutomaticBrakeOn = true;

            CustomTrain.SetCollision(false);

            CustomTrain.SetToAttach(Vehicle, new Vector3(0, 4.5f, Mods.IsDMC12 ? 0 : Vehicle.HeightAboveGround - CustomTrain.Carriage(1).HeightAboveGround), 1, 0);

            CustomTrain.SetPosition(Vehicle.GetOffsetPosition(offset: Vector3.Zero.GetSingleOffset(Coordinate.Y, -1)));

            CustomTrain.OnVehicleAttached += CustomTrain_OnVehicleAttached;
            CustomTrain.OnTrainDeleted += CustomTrain_OnTrainDeleted;
        }

        private void CustomTrain_OnTrainDeleted()
        {
            CustomTrain.OnTrainDeleted -= CustomTrain_OnTrainDeleted;
            CustomTrain.OnVehicleAttached -= CustomTrain_OnVehicleAttached;
            CustomTrain = null;

            if (Properties.IsAttachedToRogersSierra)
                Start();
            else
                Stop();
        }

        private void CustomTrain_OnVehicleAttached(bool toRogersSierra = false)
        {
            CustomTrain.DisableToDestroy();

            CustomTrain.CruiseSpeedMPH = 0;
            CustomTrain.SpeedMPH = 0;
            CustomTrain.IsAutomaticBrakeOn = true;

            Properties.IsOnTracks = true;

            if (_isReentryOn)
            {
                if (CustomTrain.SpeedMPH == 0)
                    CustomTrain.SpeedMPH = 88;
            }
            else
                CustomTrain.Speed = Vehicle.IsGoingForward() ? _speed : -_speed;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!Properties.IsOnTracks || !CustomTrain.IsRogersSierra)
                return;

            CustomTrain.RogersSierra?.KeyDown(e.KeyCode);
        }

        public override void Tick()
        {
            if (Mods.Wheel != WheelType.RailroadInvisible)
            {
                if (Properties.IsOnTracks)
                    Stop();

                return;
            }

            if (Properties.IsOnTracks && CustomTrain == null)
                Stop();

            if (Properties.IsOnTracks)
            {
                Properties.IsAttachedToRogersSierra = CustomTrain.IsRogersSierra;

                if (Properties.IsAttachedToRogersSierra)
                {
                    if (Game.IsControlPressed(GTA.Control.VehicleAccelerate) && FusionUtils.PlayerVehicle == Vehicle && Vehicle.IsEngineRunning && !CustomTrain.RogersSierra.IsOnTrainMission)
                    {
                        CustomTrain.SwitchToRegular();
                        return;
                    }

                    if (CustomTrain.RogersSierra.Locomotive.Speed > 0 && !CustomTrain.RogersSierra.Locomotive.IsGoingForward())
                        CustomTrain.SwitchToRegular();

                    return;
                }
                else
                    CustomTrain.IsAccelerationOn = Vehicle.IsPlayerDriving() && Vehicle.IsVisible && Vehicle.IsEngineRunning;

                if (FusionUtils.PlayerVehicle == Vehicle)
                    Function.Call(Hash.DISABLE_CONTROL_ACTION, 27, 59, true);

                if (_isReentryOn && CustomTrain.AttachedToTarget && CustomTrain.SpeedMPH == 0)
                {
                    if (_forceFreightTrain || FusionUtils.Random.NextDouble() <= 0.25f)
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
                if (FusionUtils.GetWheelsPositions(Vehicle).TrueForAll(x => FusionUtils.IsWheelOnTracks(x, Vehicle)))
                {
                    CustomTrain?.DeleteTrain();
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

            if (CustomTrain != null)
            {
                if (CustomTrain.Exists && CustomTrain.AttachedToTarget)
                    CustomTrain.DetachTargetVehicle();

                CustomTrain.OnVehicleAttached -= CustomTrain_OnVehicleAttached;
                CustomTrain.OnTrainDeleted -= CustomTrain_OnTrainDeleted;

                if (CustomTrain.IsRogersSierra)
                    CustomTrain.SwitchToRegular();

                if (CustomTrain.Exists && !CustomTrain.IsRogersSierra)
                    CustomTrain.DeleteTrain();

                CustomTrain = null;
            }
        }
    }
}
