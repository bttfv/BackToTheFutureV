using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
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
            Events.SetCutsceneMode += SetCutsceneMode;

            if (Mods.IsDMC12)
                Props.LicensePlate.OnAnimCompleted += LicensePlate_OnAnimCompleted;

            TimeHandler.OnTimeChanged += TimeChanged;
        }

        private void LicensePlate_OnAnimCompleted(AnimationStep animationStep)
        {
            Props.LicensePlate?.ScatterProp(0.1f);

            if (Properties.TimeTravelPhase == TimeTravelPhase.InTime && Properties.TimeTravelType == TimeTravelType.Cutscene)
                gameTimer += 1500;
        }

        public static void TimeChanged(DateTime time)
        {
            RogersSierraRailway.TrainManager.RogersSierraList.ForEach(x => x.Delete());
            TimeMachineHandler.ExistenceCheck(time);

            if (!ModSettings.WaybackSystem)
                RemoteTimeMachineHandler.ExistenceCheck(time);
        }

        public void SetCutsceneMode(bool cutsceneOn)
        {
            Properties.CutsceneMode = cutsceneOn;

            TextHandler.ShowHelp("TimeTravelModeChange", true, Properties.CutsceneMode ? TextHandler.GetLocalizedText("Cutscene") : TextHandler.GetLocalizedText("Instant"));
        }

        public void StartTimeTravel(int delay = 0)
        {
            Properties.TimeTravelPhase = TimeTravelPhase.InTime;
            gameTimer = Game.GameTime + delay;
            _currentStep = 0;
        }

        public override void Tick()
        {
            if (Properties.TimeTravelPhase != TimeTravelPhase.InTime)
                return;

            if (FusionUtils.PlayerVehicle == Vehicle && !Properties.IsRemoteControlled && !FusionUtils.HideGUI)
                FusionUtils.HideGUI = true;

            if (FusionUtils.PlayerPed.IsInVehicle(Vehicle))
            {
                Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, FusionUtils.PlayerPed);
                Function.Call(Hash.STOP_CURRENT_PLAYING_SPEECH, FusionUtils.PlayerPed);
            }

            if (Game.GameTime < gameTimer)
                return;

            switch (_currentStep)
            {
                case 0:

                    if (Properties.IsRemoteControlled)
                        Properties.TimeTravelType = TimeTravelType.RC;
                    else if (Properties.IsWayback)
                        Properties.TimeTravelType = TimeTravelType.Wayback;
                    else
                    {
                        if (Vehicle.GetPedOnSeat(VehicleSeat.Driver) != FusionUtils.PlayerPed)
                            Properties.TimeTravelType = TimeTravelType.RC;
                        else
                        {
                            if (!Properties.CutsceneMode || FusionUtils.IsCameraInFirstPerson())
                                Properties.TimeTravelType = TimeTravelType.Instant;
                            else
                                Properties.TimeTravelType = TimeTravelType.Cutscene;
                        }
                    }

                    Properties.LastVelocity = Vehicle.Velocity;

                    // Set previous time
                    Properties.PreviousTime = FusionUtils.CurrentTime;

                    // Invoke delegate
                    Events.OnTimeTravelStarted?.Invoke();

                    Properties.TimeTravelDestPos = WaypointScript.WaypointPosition;

                    Properties.TimeTravelsCount++;

                    Properties.ReplicaGUID = Guid.NewGuid();

                    if (Properties.TimeTravelType == TimeTravelType.Instant)
                    {
                        // Create a copy of the current status of the time machine
                        TimeMachine.LastDisplacementClone = TimeMachine.Clone();

                        Sounds.TimeTravelInstant?.Play();

                        if (FusionUtils.IsCameraInFirstPerson())
                            Props.WhiteSphere.SpawnProp();
                        else
                            ScreenFlash.FlashScreen(0.25f);

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

                        TimeHandler.TimeTravelTo(Properties.DestinationTime);

                        // Invoke delegate
                        Events.OnTimeTravelEnded?.Invoke();
                        Events.OnReenterEnded?.Invoke();

                        //Add LastDisplacementCopy to remote time machines list
                        RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);

                        return;
                    }

                    Sounds.TimeTravelCutscene?.Play();

                    // Play the effects
                    Particles?.TimeTravelEffect?.Play();

                    // Play the light explosion
                    Particles?.LightExplosion?.Play();

                    trails = FireTrailsHandler.SpawnForTimeMachine(TimeMachine);

                    // If the Vehicle is remote controlled or the player is not the one in the driver seat
                    if (Properties.TimeTravelType == TimeTravelType.RC || Properties.TimeTravelType == TimeTravelType.Wayback)
                    {
                        if (Mods.IsDMC12 && !Properties.IsFlying && !Properties.IsOnTracks && Mods.Plate == PlateType.Outatime)
                        {
                            Sounds.Plate?.Play();
                            Props.LicensePlate?.Play(false, true);
                        }

                        Vehicle.SetMPHSpeed(0);

                        // Add to time travelled list
                        if (Properties.TimeTravelType == TimeTravelType.RC)
                            RemoteTimeMachineHandler.AddRemote(TimeMachine.Clone());

                        Vehicle.SetVisible(false);

                        // Invoke delegate
                        Events.OnTimeTravelEnded?.Invoke();

                        gameTimer = Game.GameTime + 300;

                        _currentStep++;
                        return;
                    }

                    // Create a copy of the current status of the time machine
                    TimeMachine.LastDisplacementClone = TimeMachine.Clone();

                    if (Mods.HoverUnderbody == ModState.On)
                        Properties.CanConvert = false;

                    Game.Player.IgnoredByPolice = true;

                    ExternalHUD.SetOff();

                    PlayerSwitch.Disable = true;

                    Vehicle.SetVisible(false);

                    if (Mods.IsDMC12 && !Properties.IsFlying && !Properties.IsOnTracks && Mods.Plate == PlateType.Outatime)
                    {
                        //if (Properties.TimeTravelType == TimeTravelType.Cutscene)
                        //    TimeMachine.CustomCamera = TimeMachineCamera.LicensePlate;

                        Sounds.Plate?.Play();
                        Props.LicensePlate?.Play(false, true);
                    }

                    gameTimer = Game.GameTime + 300;

                    _currentStep++;
                    break;

                case 1:

                    Particles.TimeTravelEffect?.Stop();

                    if (Properties.TimeTravelType == TimeTravelType.RC || Properties.TimeTravelType == TimeTravelType.Wayback)
                    {
                        // Stop remote controlling
                        if (Properties.IsRemoteControlled)
                            RemoteTimeMachineHandler.StopRemoteControl();

                        TimeMachineHandler.RemoveTimeMachine(TimeMachine, true, true);

                        Properties.TimeTravelPhase = TimeTravelPhase.Completed;

                        return;
                    }

                    gameTimer = Game.GameTime + 3700;

                    _currentStep++;

                    break;

                case 2:
                    if (Properties.MissionType == MissionType.Escape)
                        return;

                    Screen.FadeOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    _currentStep++;
                    break;

                case 3:

                    Props.LicensePlate?.Delete();

                    TimeMachine.CustomCameraManager.Stop();
                    FireTrailsHandler.RemoveTrail(trails);

                    if (Properties.TimeTravelType == TimeTravelType.Cutscene)
                        TimeHandler.TimeTravelTo(Properties.DestinationTime.AddMinutes(-1));
                    else
                        TimeHandler.TimeTravelTo(Properties.DestinationTime);

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

                    _currentStep++;
                    break;

                case 5:

                    //Add LastDisplacementCopy to remote time machines list
                    RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);

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
                Events.SetCutsceneMode?.Invoke(!Properties.CutsceneMode);
        }
    }
}