using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class ReentryHandler : HandlerPrimitive
    {
        private float _handbrakeTimer = -1;
        private int _currentStep;
        private int _gameTimer;

        public ReentryHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterStarted += OnReenterStarted;
            Events.OnReenterEnded += OnReenterEnded;
        }

        public void OnReenterStarted()
        {
            Properties.TimeTravelPhase = TimeTravelPhase.Reentering;

            if (Driver != null && Driver == FusionUtils.PlayerPed)
            {
                Properties.PlayerUsed = true;
            }
        }

        public override void Tick()
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
            {
                return;
            }

            if (Game.GameTime < _gameTimer)
            {
                return;
            }

            switch (_currentStep)
            {
                case 0:

                    Sounds.Reenter?.Play();

                    Particles?.Flash?.Play();

                    Function.Call(Hash.ADD_SHOCKING_EVENT_AT_POSITION, 88, Vehicle.Position.X, Vehicle.Position.Y, Vehicle.Position.Z, 1f);

                    if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                    {
                        FusionUtils.SetPadShake(500, 200);
                    }

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

                    if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                    {
                        FusionUtils.SetPadShake(500, 200);
                    }

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

                    if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                    {
                        FusionUtils.SetPadShake(500, 200);
                    }

                    _currentStep++;
                    break;

                case 3:
                    Stop();

                    Events.OnReenterEnded?.Invoke();

                    break;
            }
        }

        private void OnReenterEnded()
        {
            if (Driver == FusionUtils.PlayerPed)
            {
                RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);
            }

            Vehicle.SetVisible(true);

            DMC12?.SetVoltValue?.Invoke(50);

            FusionUtils.HideGUI = false;

            PlayerSwitch.Disable = false;

            Game.Player.IgnoredByPolice = false;

            Function.Call(Hash.ENABLE_ALL_CONTROL_ACTIONS, 0);

            if (!Properties.WasOnTracks)
            {
                Vehicle.Velocity = Properties.LastVelocity;

                if (Vehicle.GetMPHSpeed() < Constants.SIDMaxAtSpeed)
                {
                    Vehicle.SetMPHSpeed(88);
                }
            }

            if (Properties.HasBeenStruckByLightning)
            {
                Properties.HasBeenStruckByLightning = false;

                Properties.PhotoFluxCapacitorActive = false;

                if (Properties.IsFlying)
                {
                    Properties.AreFlyingCircuitsBroken = true;

                    if (!Mods.IsDMC12 || Mods.Hoodbox == ModState.Off)
                    {
                        Events.SetTimeCircuitsBroken?.Invoke();
                    }
                }
                else
                {
                    Properties.PhotoGlowingCoilsActive = false;
                }
            }
            else if (Mods.IsDMC12)
            {
                Properties.ReactorCharge--;
            }

            if (!Properties.IsOnTracks && !Properties.IsFlying && Vehicle.Driver == null)
            {
                Vehicle.SteeringAngle = -35;
                Vehicle.IsHandbrakeForcedOn = true;
                Vehicle.Speed /= 2;

                VehicleControl.SetBrake(Vehicle, 1f);

                _handbrakeTimer = 0;
            }

            //Function.Call(Hash.SPECIAL_ABILITY_UNLOCK, CommonSettings.PlayerPed.Model);
            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);

            if (Driver != null && Driver != FusionUtils.PlayerPed)
            {
                Driver.TaskDrive().Add(DriveAction.BrakeUntilTimeEndsOrCarStops, 10000).Start();
            }

            Properties.TimeTravelPhase = TimeTravelPhase.Completed;
        }

        public override void Stop()
        {
            _currentStep = 0;
            _gameTimer = 0;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }
    }
}
