using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class ReentryHandler : Handler
    {
        private float _handbrakeTimer = -1;
        private int _currentStep;
        private int _gameTimer;

        public ReentryHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenter += OnReenter;
            Events.OnReenterCompleted += OnReenterCompleted;
        }

        public void OnReenter()
        {
            Properties.TimeTravelPhase = TimeTravelPhase.Reentering;
        }

        public override void Process()
        {
            if (_handbrakeTimer > -1)
            {
                _handbrakeTimer += Game.LastFrameTime;

                if (_handbrakeTimer >= 2)
                {
                    Vehicle.IsHandbrakeForcedOn = false;

                    _handbrakeTimer = -1;
                }
            }

            if (Properties.TimeTravelPhase != TimeTravelPhase.Reentering)
                return;

            if (Game.GameTime < _gameTimer)
                return;

            // Time will be fixed to your destination time until reentry is completed.
            Utils.SetWorldTime(Properties.DestinationTime);

            switch (_currentStep)
            {
                case 0:

                    Sounds.Reenter?.Play();

                    Particles?.Flash?.Play();

                    Function.Call(Hash.ADD_SHOCKING_EVENT_AT_POSITION, 88, Vehicle.Position.X, Vehicle.Position.Y, Vehicle.Position.Z, 1f);

                    Utils.SetPadShake(500, 200);

                    int timeToAdd = 500;

                    switch (Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            timeToAdd = 100;
                            break;
                        case WormholeType.BTTF2:
                        case WormholeType.BTTF3:
                            timeToAdd = 600;
                            break;
                    }

                    _gameTimer = Game.GameTime + timeToAdd;
                    _currentStep++;
                    break;

                case 1:

                    Particles?.Flash?.Play();

                    Utils.SetPadShake(500, 200);

                    timeToAdd = 500;

                    switch (Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            timeToAdd = 300;
                            break;
                        case WormholeType.BTTF2:
                        case WormholeType.BTTF3:
                            timeToAdd = 600;
                            break;
                    }

                    _gameTimer = Game.GameTime + timeToAdd;
                    _currentStep++;
                    break;

                case 2:

                    Particles?.Flash?.Play();

                    Utils.SetPadShake(500, 200);

                    _currentStep++;
                    break;

                case 3:
                    Stop();

                    Events.OnReenterCompleted?.Invoke();

                    break;
            }
        }

        private void OnReenterCompleted()
        {
            Properties.TimeTravelPhase = TimeTravelPhase.Completed;

            Vehicle.SetVisible(true);

            Utils.HideGUI = false;

            PlayerSwitch.Disable = false;

            Game.Player.IgnoredByPolice = false;

            if (!Properties.WasOnTracks)
            {
                Vehicle.Velocity = Properties.LastVelocity;

                if (Vehicle.GetMPHSpeed() < Constants.PlayDiodeSoundAtSpeed)
                    Vehicle.SetMPHSpeed(88);
            }

            if (Properties.HasBeenStruckByLightning)
            {
                Events.SetSIDLedsState?.Invoke(true, true);

                Properties.HasBeenStruckByLightning = false;

                if (Properties.IsFlying)
                {
                    Properties.AreFlyingCircuitsBroken = true;

                    if (!Mods.IsDMC12 || Mods.Hoodbox == ModState.Off)
                        Events.SetTimeCircuitsBroken?.Invoke();
                }
            }
            else if (Mods.IsDMC12)
                Properties.IsFueled = false;

            if (!Properties.IsOnTracks && !Properties.IsFlying && Vehicle.Driver == null)
            {
                Vehicle.SteeringAngle = Utils.Random.NextDouble() >= 0.5f ? 35 : -35;
                Vehicle.IsHandbrakeForcedOn = true;
                Vehicle.Speed /= 2;

                VehicleControl.SetBrake(Vehicle, 1f);

                _handbrakeTimer = 0;
            }

            //Function.Call(Hash.SPECIAL_ABILITY_UNLOCK, CommonSettings.PlayerPed.Model);
            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);

            //if (Vehicle.Driver != Utils.PlayerPed)
            //    Events.StartDriverAI?.Invoke(true);
        }

        public override void Stop()
        {
            _currentStep = 0;
            _gameTimer = 0;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(Keys key)
        {

        }
    }
}