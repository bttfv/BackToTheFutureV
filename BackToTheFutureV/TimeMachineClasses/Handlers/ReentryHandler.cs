﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Chrono;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class ReentryHandler : HandlerPrimitive
    {
        private float _handbrakeTimer = -1;
        private bool _skid = false;
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

            if (_skid)
            {
                if (Vehicle.Speed < 2f && Vehicle.SteeringAngle < 0)
                {
                    Vehicle.SteeringAngle += 4f;
                }
                else if (Vehicle.SteeringAngle >= 0)
                {
                    Vehicle.SteeringAngle = 0;
                    _skid = false;
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

                    if ((GameClock.Now.WithSecond(0) - Properties.DestinationTime).TotalMinutes > 0)
                    {
                        _currentStep = 3;
                        break;
                    }

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

            if (Driver == FusionUtils.PlayerPed)
            {
                FusionUtils.HideGUI = false;
                PlayerSwitch.Disable = false;
                Game.Player.IgnoredByPolice = false;
                Game.EnableAllControlsThisFrame();
            }

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

                if (Mods.Hook != HookState.On && Mods.Hoodbox != ModState.On)
                    Events.SetTimeCircuitsBroken?.Invoke();

                if (Properties.IsFlying)
                {
                    Properties.AreFlyingCircuitsBroken = true;
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
                Events.SetTimeCircuits?.Invoke(false, true);
                Vehicle.IsEngineRunning = false;
                Vehicle.SteeringAngle = -35;
                Vehicle.IsHandbrakeForcedOn = true;
                Vehicle.Speed /= 2;

                Vehicle.BrakePower = 1f;

                _handbrakeTimer = 0;
                _skid = true;

                if (Mods.IsDMC12 && Mods.Wheel != WheelType.RailroadInvisible)
                {
                    Sounds.SlideStop?.Play();
                }
            }

            if (Driver == FusionUtils.PlayerPed)
                Game.Player.IsSpecialAbilityEnabled = true;

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
