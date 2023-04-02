using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class StarterHandler : HandlerPrimitive
    {
        private bool _isRestarting;

        private int _restartAt;
        private int _nextCheck;

        private readonly TimedEventHandler timedEventManager;

        private bool _lightsOn;
        private bool _highbeamsOn;

        private float _lightsBrightness;

        private readonly int _deloreanMaxFuelLevel = 65;

        public StarterHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            timedEventManager = new TimedEventHandler();

            int _timeStart = 0;
            int _timeEnd = _timeStart + 99;

            for (int i = 0; i < 3; i++)
            {
                timedEventManager.Add(0, 0, _timeStart, 0, 0, _timeEnd);
                timedEventManager.Last.SetFloat(1, 0.1f);
                timedEventManager.Last.OnExecute += Last_OnExecute;

                _timeStart = _timeEnd + 1;
                _timeEnd = _timeStart + 99;
            }

            for (int i = 0; i < 3; i++)
            {
                timedEventManager.Add(0, 0, _timeStart, 0, 0, _timeEnd);
                timedEventManager.Last.SetFloat(1, 0.1f);
                timedEventManager.Last.OnExecute += Last_OnExecute;

                _timeStart = _timeEnd + 1;
                _timeEnd = _timeStart + 199;
            }

            for (int i = 0; i < 3; i++)
            {
                timedEventManager.Add(0, 0, _timeStart, 0, 0, _timeEnd);
                timedEventManager.Last.SetFloat(1, 0.1f);
                timedEventManager.Last.OnExecute += Last_OnExecute;

                _timeStart = _timeEnd + 1;
                _timeEnd = _timeStart + 99;
            }

            Events.OnReenterEnded += OnReenterEnded;
            Events.SetEngineStall += SetEngineStall;
        }

        private void OnReenterEnded()
        {
            if (ModSettings.EngineStallEvent && Mods.Reactor == ReactorType.Nuclear && !Properties.IsRemoteControlled && Driver != null && !Properties.IsWayback)
            {
                IsPlaying = true;
            }
        }

        private void SetEngineStall(bool state)
        {
            if (!state)
            {
                if (Properties.IsEngineStalling)
                {
                    Stop();
                }

                return;
            }
            else
            {
                Driver?.Task?.ClearAnimation("veh@low@front_ds@base", "start_engine");
            }

            if (Properties.IsFlying)
            {
                Events.SetFlyMode?.Invoke(false);
            }

            Vehicle.GetLightsState(out _lightsOn, out _highbeamsOn);

            if (_highbeamsOn)
            {
                Vehicle.AreHighBeamsOn = false;
            }

            _lightsBrightness = 1;

            timedEventManager.ResetExecution();

            Properties.IsEngineStalling = true;

            _isRestarting = false;
            _nextCheck = Game.GameTime + 100;
            IsPlaying = true;
        }

        public override void Tick()
        {
            if (Properties.IsEngineStalling)
            {
                Vehicle.FuelLevel = 0;

                if (_lightsOn)
                {
                    Vehicle.SetLightsMode(LightsMode.AlwaysOn);
                    Vehicle.SetLightsBrightness(_lightsBrightness);
                }
            }

            if (Sounds.HeadHorn.IsAnyInstancePlaying && !DMC12.forcedDucking)
            {
                DMC12.forcedDucking = true;
            }
            else if (!Sounds.HeadHorn.IsAnyInstancePlaying && DMC12.forcedDucking)
            {
                DMC12.forcedDucking = false;
            }

            if (Mods.Reactor != ReactorType.Nuclear && IsPlaying && !Properties.PhotoEngineStallActive)
            {
                if (Properties.IsEngineStalling)
                {
                    Stop();
                }

                IsPlaying = false;
            }

            if (IsPlaying && !Vehicle.IsVisible && !Vehicle.IsEngineRunning && !Properties.IsEngineStalling)
            {
                Properties.PhotoEngineStallActive = true;
            }

            if (Constants.ReadyForLightningRun && Properties.AlarmSet && Properties.AlarmTime.Between(new DateTime(1955, 11, 12, 22, 03, 40), new DateTime(1955, 11, 12, 22, 3, 50)) && !Properties.IsEngineStalling && Vehicle.GetMPHSpeed() == 0 && FusionUtils.CurrentTime == Properties.AlarmTime.AddSeconds(-30))
            {
                Properties.PhotoEngineStallActive = true;
                Properties.BlockEngineRecover = true;
                Properties.AlarmTime = Properties.AlarmTime.AddSeconds(-11);
            }

            if (Game.GameTime < _nextCheck || !IsPlaying || !Vehicle.IsVisible || MenuHandler.GarageMenu.Visible)
            {
                return;
            }

            if (Vehicle.Speed == 0 && !(Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake)) && Constants.TimeTravelCooldown == -1 && !Properties.IsEngineStalling && !Properties.IsFueled && !Properties.IsRemoteControlled && Driver != null && !(FusionUtils.CurrentTime.Between(new DateTime(1955, 11, 12, 20, 0, 0), new DateTime(1955, 11, 12, 22, 4, 10)) && (Vehicle.GetStreetInfo().Street == LightningRun.LightningRunStreet || Vehicle.GetStreetInfo().Crossing == LightningRun.LightningRunStreet)))
            {
                if (FusionUtils.Random.NextDouble() < 0.25)
                {
                    Properties.PhotoEngineStallActive = true;
                }
                else
                {
                    _nextCheck = Game.GameTime + 15000;
                    return;
                }
            }

            if (Properties.IsEngineStalling)
            {
                if ((!ModSettings.EngineStallEvent && !Properties.PhotoEngineStallActive) || (Constants.ReadyForLightningRun && !Properties.PhotoEngineStallActive && !Properties.BlockEngineRecover))
                {
                    Stop();
                    _nextCheck = Game.GameTime + 10000;
                    return;
                }

                if ((Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake)) && FusionUtils.PlayerVehicle == Vehicle)
                {
                    if (timedEventManager.AllExecuted())
                    {
                        timedEventManager.ResetExecution();
                    }

                    timedEventManager.RunEvents();

                    if (!_isRestarting)
                    {
                        //Would be cool to loop this animation at the key-turning step...
                        Driver?.Task?.PlayAnimation("veh@low@front_ds@base", "start_engine", 8f, -1, AnimationFlags.Loop);
                        Sounds.EngineRestarter?.Play();
                        _restartAt = Game.GameTime + FusionUtils.Random.Next(3000, 10000);
                        _isRestarting = true;
                    }

                    if ((!Properties.BlockEngineRecover && Game.GameTime > _restartAt))
                    {
                        Stop();
                        _nextCheck = Game.GameTime + 10000;
                        return;
                    }

                    if (Properties.BlockEngineRecover && Properties.PhotoEngineStallActive && FusionUtils.CurrentTime == Properties.AlarmTime.AddSeconds(+11) && (Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake)))
                    {
                        Sounds.HeadHorn?.Play();
                        Stop();
                        _nextCheck = Game.GameTime + 10000;
                        return;
                    }
                }
                else
                {
                    Driver?.Task?.ClearAnimation("veh@low@front_ds@base", "start_engine");
                    _lightsBrightness = 1;
                    timedEventManager.ResetExecution();
                    _isRestarting = false;
                    Sounds.EngineRestarter?.Stop();
                }
            }

            _nextCheck = Game.GameTime + 100;
        }

        private void Last_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                _lightsBrightness = 1;
            }

            _lightsBrightness = timedEvent.CurrentFloat;
        }

        public override void KeyDown(KeyEventArgs e) { }

        public override void Stop()
        {
            Properties.IsEngineStalling = false;
            Properties.PhotoEngineStallActive = false;
            Properties.BlockEngineRecover = false;
            _isRestarting = false;

            if (!Properties.IsDefrosting)
            {
                IsPlaying = false;
            }

            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
            Sounds.EngineRestarter?.Stop();

            if (_lightsOn)
            {
                Vehicle.SetLightsBrightness(1);
                Vehicle.SetLightsMode(LightsMode.Default);

                Vehicle.AreHighBeamsOn = _highbeamsOn;
            }

            Vehicle.IsEngineRunning = true;
            Driver?.Task?.ClearAnimation("veh@low@front_ds@base", "start_engine");
        }

        public override void Dispose()
        {
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
        }
    }
}
