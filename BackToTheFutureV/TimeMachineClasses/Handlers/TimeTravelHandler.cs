using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using BackToTheFutureV.Players;
using System.Windows.Forms;
using System;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.Memory;
using BackToTheFutureV.Story;
using BackToTheFutureV.Entities;
using GTA.Native;
using KlangRageAudioLibrary;
using BackToTheFutureV.Vehicles;
using BackToTheFutureV.TimeMachineClasses.RC;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class TimeTravelHandler : Handler
    {        
        private int _currentStep;
        private float gameTimer;
        private FireTrail trails;
        
        public TimeTravelHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            SFX.LightExplosion = new PtfxEntityPlayer("scr_josh3", "scr_josh3_light_explosion", Vehicle, Vector3.Zero, Vector3.Zero, 4f);
            SFX.TimeTravelEffect = new PtfxEntityPlayer("core", "veh_exhaust_spacecraft", Vehicle, new Vector3(0, 4, 0), Vector3.Zero, 8f, true);

            Props.WhiteSphere = new AnimateProp(Vehicle, ModelHandler.WhiteSphere, Vector3.Zero, Vector3.Zero)
            {
                Duration = 0.25f
            };

            LoadRes();

            Events.StartTimeTravel += StartTimeTravel;
            Events.SetCutsceneMode += SetCutsceneMode;
        }

        public void LoadRes()
        {
            Sounds.TimeTravelCutscene?.Dispose();
            Sounds.TimeTravelInstant?.Dispose();

            Sounds.TimeTravelCutscene = Sounds.AudioEngine.Create($"{Properties.LowerWormholeType}/timeTravel/mode/cutscene.wav", Presets.ExteriorLoud);
            Sounds.TimeTravelInstant = Sounds.AudioEngine.Create($"{Properties.LowerWormholeType}/timeTravel/mode/instant.wav", Presets.ExteriorLoud);
        }

        public void StartTimeTravel(int delay = 0)
        {
            Properties.TimeTravelPhase = TimeTravelPhase.InTime;
            gameTimer = Game.GameTime + delay;
        }

        public void SetCutsceneMode(bool cutsceneOn)
        {
            Properties.CutsceneMode = cutsceneOn;

            Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_TimeTravelModeChange") + " " + (Properties.CutsceneMode ? Game.GetLocalizedString("BTTFV_Cutscene") : Game.GetLocalizedString("BTTFV_Instant")));
        }

        public override void Process()
        {
            if (Properties.TimeTravelPhase != TimeTravelPhase.InTime) 
                return;

            if (!Vehicle.IsVisible)
                Vehicle.IsEngineRunning = false;

            if (Vehicle == null) 
                return;

            if (Game.GameTime < gameTimer) 
                return;

            switch(_currentStep)
            {
                case 0:
                    Properties.LastVelocity = Vehicle.Velocity;

                    Properties.WasOnTracks = Properties.IsOnTracks;

                    if (Properties.IsOnTracks)
                        Events.SetRailroadMode?.Invoke(false);

                    // Set previous time
                    Properties.PreviousTime = Utils.GetWorldTime();

                    // Invoke delegate
                    Events.OnTimeTravelStarted?.Invoke();
                    
                    if (!Properties.IsRemoteControlled && Vehicle.GetPedOnSeat(VehicleSeat.Driver) == Main.PlayerPed && (!Properties.CutsceneMode || Utils.IsPlayerUseFirstPerson()))
                    {
                        // Create a copy of the current status of the Delorean
                        TimeMachine.LastDisplacementClone = TimeMachine.Clone;

                        Sounds.TimeTravelInstant.Play();

                        if (Utils.IsPlayerUseFirstPerson())
                            Props.WhiteSphere.SpawnProp();
                        else
                            ScreenFlash.FlashScreen(0.25f);                        

                        // Have to call SetupJump manually here.
                        TimeHandler.TimeTravelTo(TimeMachine, Properties.DestinationTime);

                        Stop();

                        if (Properties.WasOnTracks)
                            Events.SetRailroadMode?.Invoke(true, true);

                        if (!Properties.HasBeenStruckByLightning && Mods.IsDMC12)
                            Properties.IsFueled = false;

                        Events.SetFreeze?.Invoke(!Properties.HasBeenStruckByLightning);

                        if (Mods.Hook == HookState.On)
                            Mods.Hook = HookState.Removed;

                        if (Mods.Plate == PlateType.Outatime)
                            Mods.Plate = PlateType.Empty;

                        // Invoke delegate
                        Events.OnTimeTravelCompleted?.Invoke();
                        Events.OnReenterCompleted?.Invoke();

                        // Stop handling
                        Stop();

                        //Add LastDisplacementCopy to remote Deloreans list
                        RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);

                        return;
                    }

                    Sounds.TimeTravelCutscene.Play();

                    // Play the effects
                    SFX.TimeTravelEffect.Play();

                    // Play the light explosion
                    SFX.LightExplosion.Play();

                    trails = FireTrailsHandler.SpawnForDelorean(
                        TimeMachine,
                        Properties.HasBeenStruckByLightning,
                        (Properties.HasBeenStruckByLightning || (Mods.HoverUnderbody == ModState.On && Properties.IsFlying)) ? 1f : 45,
                        Properties.HasBeenStruckByLightning ? -1 : 15,
                        Mods.WormholeType == WormholeType.BTTF1, Mods.Wheel == WheelType.RailroadInvisible ? 75 : 50);

                    // If the Vehicle is remote controlled or the player is not the one in the driver seat
                    if (Properties.IsRemoteControlled || Vehicle.GetPedOnSeat(VehicleSeat.Driver) != Main.PlayerPed)
                    {                        
                        Vehicle.SetMPHSpeed(0);

                        // Stop remote controlling
                        Events.SetRCMode?.Invoke(false);

                        // Add to time travelled list
                        RemoteTimeMachineHandler.AddRemote(TimeMachine.Clone);

                        Utils.HideVehicle(Vehicle, true);

                        gameTimer = Game.GameTime + 300;

                        _currentStep++;
                        return;
                    }

                    // Create a copy of the current status of the Delorean
                    TimeMachine.LastDisplacementClone = TimeMachine.Clone;

                    if (Mods.HoverUnderbody == ModState.On)
                        Properties.CanConvert = false;

                    Game.Player.IgnoredByPolice = true;

                    Main.HideGui = true;

                    Main.DisablePlayerSwitching = true;

                    Utils.HideVehicle(Vehicle, true);

                    gameTimer = Game.GameTime + 300;

                    _currentStep++;
                    break;

                case 1:
                    SFX.TimeTravelEffect.Stop();

                    if (Vehicle.GetPedOnSeat(VehicleSeat.Driver) != Main.PlayerPed)
                    {
                        TimeMachineHandler.RemoveTimeMachine(TimeMachine, true, true);
                        return;
                    }

                    gameTimer = Game.GameTime + 3700;
                    _currentStep++;

                    break;

                case 2:
                    Screen.FadeOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    _currentStep++;
                    break;

                case 3:
                    TimeHandler.TimeTravelTo(TimeMachine, Properties.DestinationTime);
                    FireTrailsHandler.RemoveTrail(trails);                    

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
                    //Add LastDisplacementCopy to remote Deloreans list
                    RemoteTimeMachineHandler.AddRemote(TimeMachine.LastDisplacementClone);

                    Events.OnReenter?.Invoke();

                    ResetFields();
                    break;
            }
        }

        public override void Stop()
        {
            ResetFields();

            Utils.HideVehicle(Vehicle, false);

            Main.HideGui = false;

            Main.DisablePlayerSwitching = false;

            Game.Player.IgnoredByPolice = false;
        }

        public override void Dispose()
        {
            Sounds.TimeTravelCutscene?.Dispose();
            Sounds.TimeTravelInstant?.Dispose();
            Props.WhiteSphere?.DeleteProp();
            SFX.LightExplosion?.Dispose();
            SFX.TimeTravelEffect?.Dispose();
        }

        public override void KeyPress(Keys key)
        {
            if (Main.PlayerVehicle != Vehicle) return;

            if (key == Keys.Multiply)
                SetCutsceneMode(!Properties.CutsceneMode);
        }

        public void ResetFields()
        {
            _currentStep = 0;
            gameTimer = 0;
        }
    }
}