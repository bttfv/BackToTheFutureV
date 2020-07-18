using BackToTheFutureV.Utility;
using System.Windows.Forms;
using KlangRageAudioLibrary;
using GTA;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class StarterHandler : Handler
    {
        private bool _isRestarting;
        private bool _isDead;
        private bool _firstTimeTravel;

        private readonly AudioPlayer _restarter;
        private int _restartAt;
        private int _nextCheck;

        private int _deloreanMaxFuelLevel = 65;

        public StarterHandler(TimeCircuits circuits) : base(circuits)
        {
            _restarter = circuits.AudioEngine.Create("bttf1/engine/restart.wav", Presets.ExteriorLoudLoop);
            _restarter.SourceBone = "engine";
            _restarter.FadeOutMultiplier = 6f;
            _restarter.FadeInMultiplier = 4f;
            _restarter.MinimumDistance = 6f;

            TimeCircuits.OnTimeTravelComplete += OnTimeTravelComplete;
        }

        private void OnTimeTravelComplete()
        {
            if (Mods.Reactor == ReactorType.Nuclear)
                _firstTimeTravel = true;
        }

        public override void Process()
        {
            if (_isDead)
                Vehicle.FuelLevel = 0;

            if (Mods.Reactor != ReactorType.Nuclear && _firstTimeTravel)
            {
                if (_isDead)
                    Stop();

                _firstTimeTravel = false;
            }               

            if (Game.GameTime < _nextCheck || !_firstTimeTravel || !Vehicle.IsVisible) return;

            if (Vehicle.Speed == 0 && !_isDead && !IsFueled)
            {
                var random = Utils.Random.NextDouble();

                if(random > 0.75)
                {
                    _isDead = true;
                }
                else
                {
                    _nextCheck = Game.GameTime + 1000;
                    return;
                }
            }

            if (_isDead)
            {
                if((Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake)) && Main.PlayerVehicle == Vehicle)
                {
                    if (!_isRestarting)
                    {
                        _restarter.Play();
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
                    _isRestarting = false;
                    _restarter.Stop();
                }
            }

            _nextCheck = Game.GameTime + 100;
        }

        public override void KeyPress(Keys key) {}

        public override void Stop()
        {
            _isDead = false;
            _isRestarting = false;
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
            _restarter.Stop();
        }

        public override void Dispose()
        {
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
            _restarter.Dispose();
        }
    }
}