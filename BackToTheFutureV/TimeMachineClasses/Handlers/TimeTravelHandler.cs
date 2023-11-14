﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Chrono;
using GTA.Math;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV
{
    internal class TimeTravelHandler : HandlerPrimitive
    {
        private int _currentStep;
        private float gameTimer;
        private FireTrail trails;

        public TimeTravelHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.StartTimeTravel += StartTimeTravel;

            if (Mods.IsDMC12)
            {
                Props.LicensePlate.OnAnimCompleted += LicensePlate_OnAnimCompleted;
            }

            TimeHandler.OnTimeChanged += TimeChanged;
        }

        private void LicensePlate_OnAnimCompleted(AnimationStep animationStep)
        {
            Props.LicensePlate?.ScatterProp(0.1f);

            if (Properties.TimeTravelPhase == TimeTravelPhase.InTime && Properties.TimeTravelType == TimeTravelType.Cutscene)
            {
                gameTimer += 1500;
            }
        }

        public static void TimeChanged(GameClockDateTime time)
        {
            TimeMachineHandler.ExistenceCheck(time);
            RemoteTimeMachineHandler.ExistenceCheck(time);
        }

        public void StartTimeTravel(int delay = 0)
        {
            gameTimer = Game.GameTime + delay;
            _currentStep = 0;
        }

        public override void Tick()
        {
            if (Properties.TimeTravelPhase != TimeTravelPhase.InTime)
            {
                return;
            }

            if (FusionUtils.PlayerVehicle == Vehicle && !Properties.IsRemoteControlled && !FusionUtils.HideGUI)
            {
                FusionUtils.HideGUI = true;
                Game.DisableAllControlsThisFrame();
            }

            if (FusionUtils.PlayerPed.IsInVehicle(Vehicle))
            {
                FusionUtils.PlayerPed.StopCurrentPlayingSpeech();
                FusionUtils.PlayerPed.StopCurrentPlayingAmbientSpeech();
            }

            if (Game.GameTime < gameTimer)
            {
                return;
            }

            switch (_currentStep)
            {
                case 0:

                    if (Properties.IsRemoteControlled)
                    {
                        Properties.TimeTravelType = TimeTravelType.RC;
                    }
                    else
                    {
                        if (Vehicle.GetPedOnSeat(VehicleSeat.Driver) != FusionUtils.PlayerPed)
                        {
                            Properties.TimeTravelType = TimeTravelType.RC;
                        }
                        else
                        {
                            if ((!ModSettings.CutsceneMode && !Properties.HasBeenStruckByLightning) || FusionUtils.IsCameraInFirstPerson())
                            {
                                Properties.TimeTravelType = TimeTravelType.Instant;
                            }
                            else
                            {
                                Properties.TimeTravelType = TimeTravelType.Cutscene;
                            }
                        }
                    }

                    RemoteTimeMachine remoteTimeMachine = RemoteTimeMachineHandler.GetRemoteTimeMachineFromGUID(Properties.GUID);

                    if (remoteTimeMachine != null)
                    {
                        if (Properties.DestinationTime > GameClock.Now)
                            remoteTimeMachine.TimeMachineClone.ExistsUntil = GameClockDateTime.MaxValue;
                        else
                            remoteTimeMachine.TimeMachineClone.ExistsUntil = GameClock.Now;
                    }

                    Properties.LastVelocity = Vehicle.Velocity;

                    // Set previous time
                    Properties.PreviousTime = GameClock.Now;

                    // Invoke delegate
                    Events.OnTimeTravelStarted?.Invoke();

                    Properties.TimeTravelDestPos = WaypointScript.WaypointPosition;

                    Properties.TimeTravelsCount++;

                    if (Properties.TimeTravelType == TimeTravelType.Instant)
                    {
                        RemoteTimeMachineHandler.RemoteTimeMachines.FindAll(x => x.TimeMachineClone.Properties.GUID == Properties.GUID).ForEach(x => x.TimeMachineClone.Properties.PlayerUsed = true);
                        // Create a copy of the current status of the time machine
                        TimeMachine.LastDisplacementClone = TimeMachine.Clone();
                        Sounds.TimeTravelInstant?.Play();

                        if (FusionUtils.IsCameraInFirstPerson())
                        {
                            Props.WhiteSphere.SpawnProp();
                        }
                        else
                        {
                            ScreenFlash.FlashScreen(0.25f);
                        }

                        if (Properties.TimeTravelDestPos != Vector3.Zero)
                        {
                            if (Properties.IsOnTracks)
                            {
                                Events.SetStopTracks?.Invoke(3000);
                                Properties.WasOnTracks = false;

                                TimeMachine.LastDisplacementClone.Properties.WasOnTracks = false;
                            }

                            Vehicle.TeleportTo(Properties.TimeTravelDestPos);
                        }

                        Properties.TimeTravelDestPos = Vector3.Zero;

                        if (!Properties.IsWayback)
                            Properties.NewGUID();

                        TimeHandler.TimeTravelTo(Properties.DestinationTime);

                        // Invoke delegate
                        Events.OnTimeTravelEnded?.Invoke();
                        Events.OnReenterEnded?.Invoke();

                        return;
                    }

                    Sounds.TimeTravelCutscene?.Play();

                    // Play the effects
                    Particles?.TimeTravelEffect?.Play();

                    // Play the light explosion
                    Particles?.LightExplosion?.Play();

                    trails = FireTrailsHandler.SpawnForTimeMachine(TimeMachine);

                    // If the Vehicle is remote controlled or the player is not the one in the driver seat
                    if (Properties.TimeTravelType == TimeTravelType.RC)
                    {
                        if (!Properties.IsRemoteControlled)
                            RemoteTimeMachineHandler.RemoteTimeMachines.FindAll(x => x.TimeMachineClone.Properties.GUID == Properties.GUID).FindLast(x => x.TimeMachineClone.Properties.PlayerUsed = true);
                        else
                            RemoteTimeMachineHandler.RemoteTimeMachines.FindAll(x => x.TimeMachineClone.Properties.GUID == Properties.GUID).FindLast(x => x.TimeMachineClone.Properties.PlayerUsed = false);

                        if (Mods.IsDMC12 && !Properties.IsFlying && !Properties.IsOnTracks && Mods.Plate == PlateType.Outatime)
                        {
                            Sounds.Plate?.Play();
                            Props.LicensePlate?.Play(false, true);
                        }

                        Vehicle.SetMPHSpeed(0);

                        // Add to time travelled list
                        if (Properties.TimeTravelType == TimeTravelType.RC && !Properties.IsWayback)
                        {
                            RemoteTimeMachineHandler.AddRemote(TimeMachine.Clone());
                        }

                        if (Properties.DestinationTime <= GameClock.Now)
                            TimeMachine.Vehicle.SetAlpha(AlphaLevel.L0);
                        else
                            Vehicle.SetVisible(false);

                        // Invoke delegate
                        Events.OnTimeTravelEnded?.Invoke();

                        gameTimer = Game.GameTime + 300;

                        _currentStep++;
                        return;
                    }

                    RemoteTimeMachineHandler.RemoteTimeMachines.FindAll(x => x.TimeMachineClone.Properties.GUID == Properties.GUID).ForEach(x => x.TimeMachineClone.Properties.PlayerUsed = true);
                    // Create a copy of the current status of the time machine
                    TimeMachine.LastDisplacementClone = TimeMachine.Clone();

                    if (Mods.HoverUnderbody == ModState.On)
                    {
                        Properties.CanConvert = false;
                    }

                    Game.Player.IgnoredByPolice = true;

                    PlayerSwitch.Disable = true;

                    Vehicle.SetVisible(false);

                    if (Mods.IsDMC12 && !Properties.IsFlying && !Properties.IsOnTracks && Mods.Plate == PlateType.Outatime)
                    {
                        Sounds.Plate?.Play();
                        Props.LicensePlate?.Play(false, true);
                    }

                    gameTimer = Game.GameTime + 300;

                    _currentStep++;
                    break;

                case 1:

                    Particles.TimeTravelEffect?.Stop();

                    if (Properties.TimeTravelType == TimeTravelType.RC)
                    {
                        // Stop remote controlling
                        if (Properties.IsRemoteControlled && !Properties.IsWayback)
                        {
                            RemoteTimeMachineHandler.StopRemoteControl();
                        }

                        if (Properties.DestinationTime <= GameClock.Now)
                        {
                            TimeMachine.StartTurnVisible();
                            Events.OnTimeTravelEnded?.Invoke();
                            Properties.TimeTravelPhase = TimeTravelPhase.Completed;
                        }                            
                        else
                            TimeMachineHandler.RemoveTimeMachine(TimeMachine, true, true);

                        return;
                    }

                    gameTimer = Game.GameTime + 3700;

                    _currentStep++;

                    break;

                case 2:
                    if (Properties.MissionType == MissionType.Escape)
                    {
                        return;
                    }

                    Screen.FadeOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    _currentStep++;
                    break;

                case 3:

                    Props.LicensePlate?.Delete();

                    TimeMachine.CustomCameraManager.Stop();
                    FireTrailsHandler.RemoveTrail(trails);

                    ScriptCameraDirector.StopRendering();

                    if (!Properties.IsWayback)
                        Properties.NewGUID();

                    if (TimeHandler.RealTime)
                    {
                        Properties.DestinationTime.TryAdd(GameClockDuration.FromSeconds(-4), out GameClockDateTime temp);
                        TimeHandler.TimeTravelTo(temp);
                    }
                    else
                    {
                        Properties.DestinationTime.TryAdd(GameClockDuration.FromMinutes(-2), out GameClockDateTime temp);
                        TimeHandler.TimeTravelTo(temp);
                    }

                    if (Properties.TimeTravelDestPos != Vector3.Zero)
                    {
                        if (Properties.IsOnTracks)
                        {
                            Events.SetStopTracks?.Invoke(3000);
                            Properties.WasOnTracks = false;

                            TimeMachine.LastDisplacementClone.Properties.WasOnTracks = false;
                        }

                        Vehicle.TeleportTo(Properties.TimeTravelDestPos);
                    }

                    Properties.TimeTravelDestPos = Vector3.Zero;

                    gameTimer = Game.GameTime + 1000;

                    _currentStep++;
                    break;

                case 4:
                    gameTimer = Game.GameTime + 2000;
                    Screen.FadeIn(1000);
                    GameplayCamera.RelativeHeading = 0f;

                    _currentStep++;
                    break;

                case 5:

                    Events.OnTimeTravelEnded?.Invoke();
                    Events.OnReenterStarted?.Invoke();
                    break;
            }
        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == ModControls.CutsceneToggle)
            {
                ModSettings.CutsceneMode = !ModSettings.CutsceneMode;
                TextHandler.Me.ShowHelp("TimeTravelModeChange", true, ModSettings.CutsceneMode ? TextHandler.Me.GetLocalizedText("Cutscene") : TextHandler.Me.GetLocalizedText("Instant"));
            }
        }
    }
}
