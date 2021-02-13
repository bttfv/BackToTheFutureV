using BackToTheFutureV.Players;
using BackToTheFutureV.Settings;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using System.Windows.Forms;
using static FusionLibrary.Enums;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class TimeTravelHandler : Handler
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
            RemoteTimeMachineHandler.ExistenceCheck(time);

            WaybackMachineHandler.Stop();
        }

        public void SetCutsceneMode(bool cutsceneOn)
        {
            Properties.CutsceneMode = cutsceneOn;

            Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_TimeTravelModeChange") + " " + (Properties.CutsceneMode ? Game.GetLocalizedString("BTTFV_Cutscene") : Game.GetLocalizedString("BTTFV_Instant")));
        }

        public void StartTimeTravel(int delay = 0)
        {
            Properties.TimeTravelPhase = TimeTravelPhase.InTime;
            gameTimer = Game.GameTime + delay;
            _currentStep = 0;
        }

        public override void Process()
        {
            if (Properties.TimeTravelPhase != TimeTravelPhase.InTime || Game.GameTime < gameTimer)
                return;

            switch (_currentStep)
            {
                case 0:

                    if (Properties.IsRemoteControlled)
                        Properties.TimeTravelType = TimeTravelType.RC;
                    else
                    {
                        if (Vehicle.GetPedOnSeat(VehicleSeat.Driver) != Utils.PlayerPed)
                        {
                            if (TimeMachine.IsWaybackPlaying)
                                Properties.TimeTravelType = TimeTravelType.Wayback;
                            else
                                Properties.TimeTravelType = TimeTravelType.RC;
                        }
                        else
                        {
                            if (!Properties.CutsceneMode || Utils.IsPlayerUseFirstPerson())
                                Properties.TimeTravelType = TimeTravelType.Instant;
                            else
                                Properties.TimeTravelType = TimeTravelType.Cutscene;
                        }
                    }

                    Properties.LastVelocity = Vehicle.Velocity;

                    // Set previous time
                    Properties.PreviousTime = Utils.GetWorldTime();

                    // Invoke delegate
                    Events.OnTimeTravelStarted?.Invoke();

                    Properties.TimeTravelDestPos = Secondary.WaypointPosition;

                    Properties.TimeTravelsCount++;

                    if (Properties.TimeTravelType == TimeTravelType.Instant)
                    {
                        // Create a copy of the current status of the time machine
                        TimeMachine.LastDisplacementClone = TimeMachine.Clone;

                        Sounds.TimeTravelInstant?.Play();

                        if (Utils.IsPlayerUseFirstPerson())
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
                        Events.OnTimeTravelCompleted?.Invoke();
                        Events.OnReenterCompleted?.Invoke();

                        //Add LastDisplacementCopy to remote time machines list
                        RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);

                        return;
                    }

                    Sounds.TimeTravelCutscene?.Play();

                    // Play the effects
                    Particles?.TimeTravelEffect?.Play();

                    // Play the light explosion
                    Particles?.LightExplosion?.Play();

                    bool is99 = Properties.HasBeenStruckByLightning && Properties.IsFlying;

                    trails = FireTrailsHandler.SpawnForTimeMachine(
                        TimeMachine,
                        is99,
                        (is99 || Properties.IsFlying) ? 1f : 45,
                        is99 ? -1 : 15,
                        Mods.WormholeType == WormholeType.BTTF1, Mods.Wheel == WheelType.RailroadInvisible ? 75 : 50);

                    // If the Vehicle is remote controlled or the player is not the one in the driver seat
                    if (Properties.TimeTravelType == TimeTravelType.RC || Properties.TimeTravelType == TimeTravelType.Wayback)
                    {
                        if (!Properties.IsFlying && Mods.Plate == PlateType.Outatime)
                        {
                            Sounds.Plate?.Play();
                            Props.LicensePlate?.Play(false, true);
                        }

                        Vehicle.SetMPHSpeed(0);

                        // Stop remote controlling
                        if (Properties.IsRemoteControlled)
                            RCManager.StopRemoteControl();

                        // Add to time travelled list
                        if (Properties.TimeTravelType == TimeTravelType.RC)
                            RemoteTimeMachineHandler.AddRemote(TimeMachine.Clone);

                        Vehicle.SetVisible(false);

                        // Invoke delegate
                        Events.OnTimeTravelCompleted?.Invoke();

                        gameTimer = Game.GameTime + 300;

                        _currentStep++;
                        return;
                    }

                    // Create a copy of the current status of the time machine
                    TimeMachine.LastDisplacementClone = TimeMachine.Clone;

                    if (Mods.HoverUnderbody == ModState.On)
                        Properties.CanConvert = false;

                    Game.Player.IgnoredByPolice = true;

                    Utils.HideGUI = true;

                    ExternalHUD.SetOff();

                    PlayerSwitch.Disable = true;

                    Vehicle.SetVisible(false);

                    if (!Properties.IsFlying && Mods.Plate == PlateType.Outatime)
                    {
                        if (Properties.TimeTravelType == TimeTravelType.Cutscene)
                            TimeMachine.CustomCamera = TimeMachineCamera.LicensePlate;

                        Sounds.Plate?.Play();
                        Props.LicensePlate?.Play(false, true);
                    }

                    gameTimer = Game.GameTime + 300;

                    _currentStep++;
                    break;

                case 1:
                    Particles?.TimeTravelEffect.Stop();

                    if (Properties.TimeTravelType == TimeTravelType.RC || Properties.TimeTravelType == TimeTravelType.Wayback)
                    {
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
                    Events.OnTimeTravelCompleted?.Invoke();

                    gameTimer = Game.GameTime + 2000;
                    Screen.FadeIn(1000);

                    _currentStep++;
                    break;

                case 5:
                    //Add LastDisplacementCopy to remote time machines list
                    RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);

                    Events.OnReenter?.Invoke();
                    break;
            }
        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(Keys key)
        {
            if (key == ModControls.CutsceneToggle)
                SetCutsceneMode(!Properties.CutsceneMode);
        }
    }
}