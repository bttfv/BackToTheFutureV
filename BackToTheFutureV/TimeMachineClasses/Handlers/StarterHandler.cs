using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class StarterHandler : Handler
    {
        private bool _isRestarting;        
        private bool _firstTimeTravel;
        
        private int _restartAt;
        private int _nextCheck;

        private TimedEventHandler timedEventManager;

        private bool _lightsOn;
        private bool _highbeamsOn;

        private float _lightsBrightness;

        private int _deloreanMaxFuelLevel = 65;

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

            Events.OnReenterCompleted += OnReenterCompleted;
            Events.SetEngineStall += SetEngineStall;
        }

        private void OnReenterCompleted()
        {
            if (ModSettings.EngineStallEvent && Mods.Reactor == ReactorType.Nuclear)
                _firstTimeTravel = true;
        }

        private void SetEngineStall(bool state)
        {
            if (!state)
            {
                if (Properties.IsEngineStalling)
                    Stop();

                return;
            }

            Vehicle.GetLightsState(out _lightsOn, out _highbeamsOn);

            if (_highbeamsOn)
                Vehicle.AreHighBeamsOn = false;

            _lightsBrightness = 1;

            timedEventManager.ResetExecution();

            Properties.IsEngineStalling = true;

            _firstTimeTravel = true;
            _isRestarting = false;
            _nextCheck = Game.GameTime + 100;
        }

        public override void Process()
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
                
            if (Mods.Reactor != ReactorType.Nuclear && _firstTimeTravel)
            {
                if (Properties.IsEngineStalling)
                    Stop();

                _firstTimeTravel = false;
            }               

            if (Game.GameTime < _nextCheck || !_firstTimeTravel || !Vehicle.IsVisible)
                return;

            if (Vehicle.Speed == 0 && !Properties.IsEngineStalling && !Properties.IsFueled)
            {                
                if(Utils.Random.NextDouble() < 0.25)
                {
                    SetEngineStall(true);
                }
                else
                {
                    _nextCheck = Game.GameTime + 15000;
                    return;
                }
            }

            if (Properties.IsEngineStalling)
            {
                if (!ModSettings.EngineStallEvent && !Properties.PhotoEngineStallActive)
                {
                    Stop();
                    Vehicle.FuelLevel = _deloreanMaxFuelLevel;
                    Vehicle.IsEngineRunning = true;
                    _nextCheck = Game.GameTime + 10000;
                    return;
                }

                if((Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake)) && Utils.PlayerVehicle == Vehicle)
                {
                    if (timedEventManager.AllExecuted())
                        timedEventManager.ResetExecution();
                    
                    timedEventManager.RunEvents();

                    if (!_isRestarting)
                    {
                        Utils.PlayerPed.Task.PlayAnimation("veh@low@front_ds@base", "start_engine", 8f, -1, AnimationFlags.Loop | AnimationFlags.CancelableWithMovement);

                        Sounds.EngineRestarter?.Play();
                        _restartAt = Game.GameTime + Utils.Random.Next(3000, 10000);
                        _isRestarting = true;
                    }

                    if (Game.GameTime > _restartAt)
                    {
                        Stop();
                        Vehicle.FuelLevel = _deloreanMaxFuelLevel;
                        Vehicle.IsEngineRunning = true;
                        _nextCheck = Game.GameTime + 10000;
                        return;
                    }
                }
                else
                {
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
                _lightsBrightness = 1;

            _lightsBrightness += timedEvent.CurrentFloat;
        }

        public override void KeyDown(Keys key) {}

        public override void Stop()
        {
            Utils.PlayerPed.Task.ClearAnimation("veh@low@front_ds@base", "start_engine");

            Properties.IsEngineStalling = false;
            Properties.PhotoEngineStallActive = false;
            _isRestarting = false;
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
            Sounds.EngineRestarter?.Stop();

            if (_lightsOn)
            {
                Vehicle.SetLightsBrightness(1);
                Vehicle.SetLightsMode(LightsMode.Default);

                Vehicle.AreHighBeamsOn = _highbeamsOn;
            }
        }

        public override void Dispose()
        {
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;            
        }
    }
}