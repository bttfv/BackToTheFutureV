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

namespace BackToTheFutureV.Delorean.Handlers
{
    public class TimeTravelHandler : Handler
    {
        public bool CutsceneMode { get; set; } = true;

        public bool IsTimeTravelling;

        private AudioPlayer timeTravelAudioCutscene;
        private AudioPlayer timeTravelAudioInstant;
        private int _currentStep;
        private float gameTimer;
        private bool noTimeSet;
        private bool is99;
        private FireTrail trails;
        private readonly PtfxEntityPlayer _lightExplosion;
        private readonly PtfxEntityPlayer _timeTravelEffect;
        private readonly AnimateProp _whiteSphere;
        private float _reentryTimer;

        public TimeTravelHandler(TimeCircuits circuits) : base(circuits)
        {
            _lightExplosion = new PtfxEntityPlayer("scr_josh3", "scr_josh3_light_explosion", Vehicle, Vector3.Zero, Vector3.Zero, 4f);
            _timeTravelEffect = new PtfxEntityPlayer("core", "veh_exhaust_spacecraft", Vehicle, new Vector3(0, 4, 0), Vector3.Zero, 8f, true);
            _whiteSphere = new AnimateProp(Vehicle, ModelHandler.WhiteSphere, Vector3.Zero, Vector3.Zero)
            {
                Duration = 0.25f
            };

            LoadRes();
        }

        public void LoadRes()
        {
            timeTravelAudioCutscene?.Dispose();
            timeTravelAudioInstant?.Dispose();

            timeTravelAudioCutscene = TimeCircuits.AudioEngine.Create($"{LowerCaseDeloreanType}/timeTravel/mode/cutscene.wav", Presets.ExteriorLoud);
            timeTravelAudioInstant = TimeCircuits.AudioEngine.Create($"{LowerCaseDeloreanType}/timeTravel/mode/instant.wav", Presets.ExteriorLoud);
        }

        public void StartTimeTravelling(bool is99 = false, int delay = 0)
        {
            this.is99 = is99;
            IsTimeTravelling = true;
            gameTimer = Game.GameTime + delay;
        }

        public void SetCutsceneMode(bool cutsceneOn)
        {
            CutsceneMode = cutsceneOn;

            Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_TimeTravelModeChange") + " " + (CutsceneMode ? Game.GetLocalizedString("BTTFV_Cutscene") : Game.GetLocalizedString("BTTFV_Instant")));
        }

        public override void Process()
        {
            _reentryTimer += Game.LastFrameTime;
            if (_reentryTimer > 2)
            {
                if(Vehicle.Driver == null)
                {
                    Vehicle.IsHandbrakeForcedOn = false;
                    Vehicle.SteeringAngle = 0;
                }
            }

            if (!IsTimeTravelling) 
                return;

            if (!Vehicle.IsVisible)
                Vehicle.IsEngineRunning = false;

            if (Vehicle == null) 
                return;

            if (Game.GameTime < gameTimer) 
                return;

            Main.PlayerPed.IsVisible = false;
            Vehicle.IsVisible = false;

            switch(_currentStep)
            {
                case 0:
                    // Sets a new remote Delorean using last copy
                    RemoteDeloreansHandler.AddDelorean(TimeCircuits.Delorean.LastDisplacementCopy);

                    TimeCircuits.Delorean.LastVelocity = Vehicle.Velocity;

                    TimeCircuits.WasOnTracks = TimeCircuits.IsOnTracks;
                                      
                    // Set previous time
                    PreviousTime = Utils.GetWorldTime();

                    // Invoke delegate
                    TimeCircuits.OnTimeTravel?.Invoke();
                    
                    if (!IsRemoteControlled && Vehicle.GetPedOnSeat(VehicleSeat.Driver) == Main.PlayerPed && (!CutsceneMode || Utils.IsPlayerUseFirstPerson()))
                    {
                        // Create a copy of the current status of the Delorean
                        TimeCircuits.Delorean.LastDisplacementCopy = TimeCircuits.Delorean.Copy;

                        timeTravelAudioInstant.Play();

                        if (Utils.IsPlayerUseFirstPerson())
                            _whiteSphere.SpawnProp();
                        else
                            ScreenFlash.FlashScreen(0.25f);                        

                        // Have to call SetupJump manually here.
                        TimeHandler.TimeTravelTo(TimeCircuits, DestinationTime);

                        Stop();

                        TimeCircuits.GetHandler<SparksHandler>().StartTimeTravelCooldown();

                        if (TimeCircuits.WasOnTracks)
                            TimeCircuits.GetHandler<RailroadHandler>().StartDriving(true);

                        if (!is99)
                            IsFueled = false;

                        TimeCircuits.GetHandler<FreezeHandler>().StartFreezeHandling(!is99);

                        if (Mods.Hoodbox == ModState.On && !TimeCircuits.IsWarmedUp)
                            TimeCircuits.GetHandler<HoodboxHandler>().SetInstant();

                        if (Mods.Hook == HookState.On)
                            Mods.Hook = HookState.Removed;

                        if (Mods.Plate == PlateType.Outatime)
                            Mods.Plate = PlateType.Empty;

                        // Invoke delegate
                        TimeCircuits.OnTimeTravelComplete?.Invoke();

                        // Stop handling
                        Stop();
                        return;
                    }

                    timeTravelAudioCutscene.Play();

                    trails = FireTrailsHandler.SpawnForDelorean(
                        TimeCircuits,
                        is99,
                        (is99 || (Mods.HoverUnderbody == ModState.On && TimeCircuits.GetHandler<FlyingHandler>().IsFlying)) ? 1f : 45,
                        is99 ? -1 : 15,
                        DeloreanType == DeloreanType.BTTF1, Mods.Wheel == WheelType.RailroadInvisible ? 75 : 50);

                    // If the Vehicle is remote controlled or the player is not the one in the driver seat
                    if (IsRemoteControlled || Vehicle.GetPedOnSeat(VehicleSeat.Driver) != Main.PlayerPed)
                    {                        
                        MPHSpeed = 0;

                        // Stop remote controlling
                        TimeCircuits.GetHandler<RcHandler>()?.StopRC();

                        // Add to time travelled list
                        RemoteDeloreansHandler.AddDelorean(TimeCircuits.Delorean.Copy);

                        // Delete delorean
                        DeloreanHandler.RemoveDelorean(TimeCircuits.Delorean);
                        return;
                    }

                    // Create a copy of the current status of the Delorean
                    TimeCircuits.Delorean.LastDisplacementCopy = TimeCircuits.Delorean.Copy;

                    if (Mods.HoverUnderbody == ModState.On)
                        TimeCircuits.GetHandler<FlyingHandler>().CanConvert = false;

                    Game.Player.IgnoredByPolice = true;

                    Main.HideGui = true;

                    Main.DisablePlayerSwitching = true;

                    Utils.HideVehicle(Vehicle, true);

                    // Play the effects
                    _timeTravelEffect.Play();

                    // Play the light explosion
                    _lightExplosion.Play();

                    TimeCircuits.Delorean.IsInTime = true;

                    gameTimer = Game.GameTime + 300;

                    _currentStep++;
                    break;

                case 1:
                    _timeTravelEffect.Stop();
                    gameTimer = Game.GameTime + 3700;
                    _currentStep++;

                    break;

                case 2:
                    Screen.FadeOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    _currentStep++;
                    break;

                case 3:
                    TimeHandler.TimeTravelTo(TimeCircuits, DestinationTime);
                    FireTrailsHandler.RemoveTrail(trails);                    

                    gameTimer = Game.GameTime + 1000;

                    _currentStep++;
                    break;

                case 4:
                    TimeCircuits.OnTimeTravelComplete?.Invoke();

                    gameTimer = Game.GameTime + 2000;
                    Screen.FadeIn(1000);

                    _currentStep++;
                    break;

                case 5:
                    Reenter();

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
            timeTravelAudioCutscene.Dispose();
            timeTravelAudioInstant.Dispose();
            _whiteSphere?.DeleteProp();
        }

        public override void KeyPress(Keys key)
        {
            if (Main.PlayerVehicle != Vehicle) return;

            if (key == Keys.Multiply)
            {
                SetCutsceneMode(!CutsceneMode);
            }
        }

        public void ResetFields()
        {
            _currentStep = 0;
            IsTimeTravelling = false;
            gameTimer = 0;
        }

        public void Reenter(bool noTimeSet = false)
        {
            var reentryHandler = TimeCircuits.GetHandler<ReentryHandler>();

            this.noTimeSet = noTimeSet;

            if(!reentryHandler.IsReentering)
            {
                TimeCircuits.OnReentryComplete = OnReentryComplete;
                reentryHandler.StartReentering(noTimeSet);
            }
        }

        private void OnReentryComplete()
        {
            Stop();

            _reentryTimer = 0;

            if (!noTimeSet)
                TimeCircuits.GetHandler<SparksHandler>().StartTimeTravelCooldown();

            noTimeSet = false;

            if (TimeCircuits.WasOnTracks)
                TimeCircuits.GetHandler<RailroadHandler>().StartDriving(true);
            else
            {
                Vehicle.Velocity = TimeCircuits.Delorean.LastVelocity;

                if (MPHSpeed == 0)
                    MPHSpeed = 88;
            }

            if (!is99)
                IsFueled = false;

            if (!IsOnTracks && Vehicle.Driver == null)
            {
                Random rand = new Random();

                int angle = rand.Next(-100, 100) >= 0 ? 35 : -35;

                Vehicle.SteeringAngle = angle;
                Vehicle.IsHandbrakeForcedOn = true;
                Vehicle.Speed = Vehicle.Speed / 2;

                VehicleControl.SetBrake(Vehicle, 1f);
            }

            TimeCircuits.Delorean.IsInTime = false;

            TimeCircuits.GetHandler<FreezeHandler>().StartFreezeHandling(!is99);
        }

    }
}