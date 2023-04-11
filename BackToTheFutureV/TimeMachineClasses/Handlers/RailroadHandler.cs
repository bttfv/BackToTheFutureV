using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class RailroadHandler : HandlerPrimitive
    {
        private CustomTrain customTrain;

        private bool _direction = false;

        private bool _isReentryOn = false;

        private int _attachDelay;

        private float _speed;

        private bool _forceFreightTrain;

        private bool _exploded = false;

        public RailroadHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnTimeTravelStarted += OnTimeTravelStarted;
            Events.OnReenterEnded += OnReenterEnded;
            Events.SetStopTracks += Stop;
            Events.StartTrain += Start;
            Events.SetTrainSpeed += SetTrainSpeed;
        }

        public void SetTrainSpeed(float speed)
        {
            if (!Properties.IsOnTracks || customTrain == null || !customTrain.Exists)
            {
                return;
            }

            customTrain.Speed = speed;
        }

        public void OnTimeTravelStarted()
        {
            Properties.WasOnTracks = Properties.IsOnTracks;

            if (!Properties.IsOnTracks)
            {
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
            {
                return;
            }

            _isReentryOn = true;

            if (customTrain == null || !customTrain.Exists)
            {
                Start(true);
            }

            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Instant:
                    if (customTrain.SpeedMPH == 0)
                    {
                        customTrain.SpeedMPH = 88;
                    }

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
            if ((!force && !Vehicle.IsOnAllWheels) || (Vehicle == FusionUtils.PlayerVehicle?.TowedVehicle))
            {
                return;
            }

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

            customTrain.OnVehicleAttached += CustomTrain_OnVehicleAttached;
            customTrain.OnTrainDeleted += CustomTrain_OnTrainDeleted;
        }

        private void CustomTrain_OnTrainDeleted()
        {
            customTrain.OnTrainDeleted -= CustomTrain_OnTrainDeleted;
            customTrain.OnVehicleAttached -= CustomTrain_OnVehicleAttached;
            customTrain = null;

            Stop();
        }

        private void CustomTrain_OnVehicleAttached()
        {
            customTrain.DisableToDestroy();

            customTrain.CruiseSpeedMPH = 0;
            customTrain.SpeedMPH = 0;
            customTrain.IsAutomaticBrakeOn = true;

            Properties.IsOnTracks = true;
            Mods.Wheels.Burst = false;

            if (_isReentryOn)
            {
                if (customTrain.SpeedMPH == 0)
                {
                    customTrain.SpeedMPH = 88;
                }
            }
            else
            {
                customTrain.Speed = Vehicle.RunningDirection() == RunningDirection.Forward ? _speed : -_speed;
            }
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {
            if (Mods.Wheel != WheelType.RailroadInvisible)
            {
                if (Properties.IsOnTracks)
                {
                    Stop();
                }

                return;
            }

            if (Properties.IsOnTracks && ((customTrain == null) || (FusionUtils.PlayerVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.TowedVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.TowedVehicle == Vehicle && !Vehicle.IsOnAllWheels)))
            {
                Stop();
            }

            if (Properties.IsOnTracks)
            {
                if (Mods.Wheels.Burst != false)
                    Mods.Wheels.Burst = false;

                customTrain.IsAccelerationOn = Vehicle.IsPlayerDriving() && Vehicle.IsVisible && Vehicle.IsEngineRunning;

                if (FusionUtils.PlayerVehicle == Vehicle)
                {
                    Game.DisableControlThisFrame(GTA.Control.VehicleMoveLeftRight);
                }

                if (_isReentryOn && customTrain.AttachedToTarget && customTrain.SpeedMPH == 0)
                {
                    if (_forceFreightTrain || (ModSettings.TrainEvent && FusionUtils.Random.NextDouble() <= 0.25f))
                    {
                        CustomTrainHandler.CreateFreightTrain(Vehicle, !_direction).SetToDestroy(Vehicle, 35);
                    }

                    _isReentryOn = false;
                    _forceFreightTrain = false;
                    return;
                }

                Vehicle _train = World.GetClosestVehicle(Vehicle.Position, 25, ModelHandler.FreightModel, ModelHandler.FreightCarModel, ModelHandler.FreightContModel1, 
                    ModelHandler.FreightContModel2, ModelHandler.GrainCarModel, ModelHandler.TankerCarModel);

                if (Vehicle.IsVisible && _train != null && Vehicle.IsTouching(_train))
                {
                    Stop();

                    if (Math.Abs(_train.GetMPHSpeed() - Vehicle.GetMPHSpeed()) > 33 && !_exploded)
                    {
                        if (TimeMachine.Properties.IsRemoteControlled && !TimeMachine.Properties.IsWayback)
                        {
                            RemoteTimeMachineHandler.StopRemoteControl();
                        }
                        Vehicle.Explode();
                        _exploded = true;
                    }
                    // TODO: SameDirection only accounts for the model rotation; doesn't account for the velocity vector direction
                    else if (!Vehicle.SameDirection(_train, 90f) && _train.GetMPHSpeed() + Vehicle.GetMPHSpeed() > 33 && !_exploded)
                    {
                        if (TimeMachine.Properties.IsRemoteControlled && !TimeMachine.Properties.IsWayback)
                        {
                            RemoteTimeMachineHandler.StopRemoteControl();
                        }
                        Vehicle.Explode();
                        _exploded = true;
                    }

                    _attachDelay = Game.GameTime + 3000;
                }

                return;
            }

            if (_attachDelay < Game.GameTime && Mods.Wheel == WheelType.RailroadInvisible && !Properties.IsFlying)
            {
                if (Vehicle.IsOnTracks())
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

            if (Mods.Wheel == WheelType.RailroadInvisible && Vehicle.IsVisible && Mods.Wheels.Burst != true)
            {
                Mods.Wheels.Burst = true;
            }

            if (delay > 0)
            {
                _attachDelay = Game.GameTime + delay;
            }

            if (customTrain != null)
            {
                if (customTrain.Exists && customTrain.AttachedToTarget)
                {
                    customTrain.DetachTargetVehicle();
                }

                customTrain.OnVehicleAttached -= CustomTrain_OnVehicleAttached;
                customTrain.OnTrainDeleted -= CustomTrain_OnTrainDeleted;

                if (customTrain.Exists)
                {
                    customTrain.DeleteTrain();
                }

                customTrain = null;
            }
        }
    }
}
