using BackToTheFutureV.Players;
using BackToTheFutureV.Settings;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using Control = GTA.Control;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class FlyingHandler : Handler
    {
        private bool _hasPlayedBoostSound;

        private readonly NativeInput _flyModeInput;

        private int _nextModeChangeAllowed;

        private int _nextForce;

        private bool _startHoverGlowLater;

        private bool _landingSmoke;
                
        private int _currentLightIndex;
        private int _nextLightChange;

        private Vector3 _forceToBeApplied = Vector3.Zero;

        private bool _hoverGlowUp;

        private bool _reload;

        public FlyingHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            if (Mods.HoverUnderbody == ModState.On)
                OnHoverUnderbodyToggle();

            _flyModeInput = new NativeInput(ModControls.Hover);
            _flyModeInput.OnControlLongPressed += OnFlyModeControlJustLongPressed;
            _flyModeInput.OnControlPressed += OnFlyModeControlJustPressed;

            Events.SetFlyMode += SetFlyMode;
            Events.SetAltitudeHold += SetHoverMode;
            Events.OnHoverUnderbodyToggle += OnHoverUnderbodyToggle;

            if (!Mods.IsDMC12)
                return;

            TimeHandler.OnDayNightChange += OnDayNightChange;

            OnDayNightChange();
        }
        
        private void OnDayNightChange()
        {            
            Props.HoverModeVentsGlow?.Delete();

            if (TimeHandler.IsNight)
                Props.HoverModeVentsGlow.SwapModel(ModelHandler.VentGlowingNight);
            else
                Props.HoverModeVentsGlow.SwapModel(ModelHandler.VentGlowing);
        }

        public void OnHoverUnderbodyToggle(bool reload = false)
        {
            _reload = reload;

            if (Mods.HoverUnderbody == ModState.Off && Properties.IsFlying && Mods.IsDMC12)
                SetFlyMode(false, true);

            if (Players.HoverModeWheels != null)
            {
                Players.HoverModeWheels.OnAnimCompleted -= OnAnimCompleted;
                Players.HoverModeWheels?.Dispose();
                Players.HoverModeWheels = null;
            }

            if (Mods.HoverUnderbody == ModState.On && Mods.IsDMC12)
            {
                Players.HoverModeWheels = new WheelAnimationPlayer(TimeMachine);
                Players.HoverModeWheels.OnAnimCompleted += OnAnimCompleted;

                Properties.AreFlyingCircuitsBroken = false;

                return;
            }

            if (Properties.IsFlying)
                SetFlyMode(false, true);

            foreach (AnimateProp x in Props.HoverModeUnderbodyLights)
                x?.Delete();
        }
        
        private void OnAnimCompleted()
        {
            if (Properties.AreWheelsInHoverMode)
            {
                _startHoverGlowLater = !Vehicle.IsEngineRunning;

                if (!_startHoverGlowLater)
                    SpawnHoverGlow();
            }

            Events.OnHoverTransformation?.Invoke();
        }

        private void SpawnHoverGlow()
        {
            if (Props.HoverModeWheelsGlow != null && Props.HoverModeWheelsGlow.IsSpawned)
                return;

            Props.HoverModeWheelsGlow?.SpawnProp();
            Sounds.HoverModeUp?.Play();
            _startHoverGlowLater = false;
        }

        private void OnFlyModeControlJustLongPressed()
        {
            if (ModControls.LongPressForHover)
            {
                if (Mods.HoverUnderbody == ModState.On && Properties.CanConvert && Utils.PlayerVehicle == Vehicle && Game.GameTime > _nextModeChangeAllowed)
                {
                    if (Properties.AreFlyingCircuitsBroken)
                    {
                        Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hover_Damaged"));

                        return;
                    }
                        
                    SetFlyMode(!Properties.AreWheelsInHoverMode);

                    _nextModeChangeAllowed = Game.GameTime + 1500;
                }
            }                
        }

        private void OnFlyModeControlJustPressed()
        {
            if (!ModControls.LongPressForHover)
            {
                if (Mods.HoverUnderbody == ModState.On && Properties.CanConvert && Utils.PlayerVehicle == Vehicle && Game.GameTime > _nextModeChangeAllowed)
                {
                    if (Properties.AreFlyingCircuitsBroken)
                    {
                        Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hover_Damaged"));

                        return;
                    }

                    SetFlyMode(!Properties.AreWheelsInHoverMode);

                    _nextModeChangeAllowed = Game.GameTime + 1500;
                }
            }
        }

        public void SetFlyMode(bool open, bool instant = false)
        {
            if (_reload)
                OnHoverUnderbodyToggle();

            if (open && Properties.AreFlyingCircuitsBroken)
            {
                if (VehicleControl.GetDeluxoTransformation(Vehicle) > 0)
                    VehicleControl.SetDeluxoTransformation(Vehicle, 0f);

                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hover_Damaged"));

                return;
            }

            if (Properties.IsOnTracks && !Mods.IsDMC12)
                Events.SetStopTracks?.Invoke(3000);

            Properties.AreWheelsInHoverMode = open;

            Properties.IsLanding = ModSettings.LandingSystem && Mods.IsDMC12 && !Properties.AreWheelsInHoverMode && !instant && Vehicle.HeightAboveGround < 20 && Vehicle.HeightAboveGround > 0.5f && !Vehicle.IsUpsideDown && VehicleControl.GetDeluxoTransformation(Vehicle) > 0;

            if (instant)
                Players.HoverModeWheels?.SetInstant(open);
            else
                Players.HoverModeWheels?.Play(open);

            // undocced native, changes delxuo transformation
            // from land to hover
            // (DOES NOT CHANGE FLY MODE!)
            if (!Properties.IsLanding)
                Function.Call((Hash)0x438b3d7ca026fe91, Vehicle, Properties.AreWheelsInHoverMode ? 1f : 0f);
            else
                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Input_VTOL_Tip").Replace("~INPUT_VEH_AIM~", (new ControlInfo(ModControls.HoverVTOL)).Button));

            if (Properties.AreWheelsInHoverMode && !instant)
            {
                Sounds.HoverModeOn?.Play();
                Particles?.HoverModeSmoke?.ForEach(x => x?.Play());
            }
            else if(!Properties.AreWheelsInHoverMode && !instant)
            {
                if (Properties.IsFlying)
                    Sounds.HoverModeOff?.Play();

                Props.HoverModeWheelsGlow?.Delete();
            }

            Properties.IsFlying = Properties.AreWheelsInHoverMode;

            _landingSmoke = false;

            if (Mods.IsDMC12)
                Function.Call(Hash._FORCE_VEHICLE_ENGINE_AUDIO, Vehicle, Properties.IsFlying ? "DELUXO" : "VIRGO");

            if (!Properties.IsLanding && !Properties.IsFlying)
                Properties.TorqueMultiplier = 1.4f;

            if (!Properties.IsFlying && Properties.IsAltitudeHolding)
                Properties.IsAltitudeHolding = false;

            Props.HoverModeVentsGlow?.Delete();
            Props.HoverModeWheelsGlow?.Delete();

            Events.OnHoverTransformation?.Invoke();
        }

        public override void KeyDown(Keys key)
        {
            if(key == ModControls.HoverAltitudeHold && Utils.PlayerVehicle == Vehicle && Properties.IsFlying)
                SetHoverMode(!Properties.IsAltitudeHolding);
        }

        public void SetHoverMode(bool mode)
        {
            Properties.IsAltitudeHolding = mode;
            Utils.DisplayHelpText($"{Game.GetLocalizedString("BTTFV_AltitudeHoldingModeChange")} {(Properties.IsAltitudeHolding ? Game.GetLocalizedString("BTTFV_On") : Game.GetLocalizedString("BTTFV_Off"))}");
        }

        public override void Process()
        {
            if (Mods.HoverUnderbody == ModState.Off)
                return;

            // Automatically fold wheels in if fly mode is exited in any other way
            // Example: getting out of vehicle, flipping vehicle over, etc
            if (Properties.IsFlying && VehicleControl.GetDeluxoTransformation(Vehicle) <= 0 )
                SetFlyMode(false);

            // Automatically set fly mode if deluxo is transformed in any other way.
            if (!Properties.IsFlying && VehicleControl.GetDeluxoTransformation(Vehicle) > 0 && !Properties.IsLanding)
                SetFlyMode(true);

            // Process the wheel animations
            Players.HoverModeWheels?.Process();

            Function.Call(Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player.Handle, Utils.PlayerVehicle != Vehicle);
                
            if (Vehicle == null || !Vehicle.IsVisible)
                return;

            // Process underbody lights
            UnderbodyLights();

            if (Properties.IsLanding)
            {
                // Reset force to be applied
                _forceToBeApplied = Vector3.Zero;

                // Process up/down movement
                UpDown();

                // Altitude holder
                //if (!Game.IsControlPressed(GTA.Control.VehicleFlyThrottleUp) && !Game.IsControlPressed(GTA.Control.VehicleFlyThrottleDown))
                //    HandleAltitudeHolding();

                // Apply force
                Vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);

                if (Vehicle.HeightAboveGround < 2 && !_landingSmoke)
                {
                    Particles?.HoverModeSmoke?.ForEach(x => x?.Play());
                    _landingSmoke = true;
                }

                if (!Vehicle.IsUpsideDown && Vehicle.HeightAboveGround > 0.5f && Vehicle.HeightAboveGround < 20)
                    return;

                SetFlyMode(false);
            }

            if (!Properties.IsFlying)
                return;
                
            if (ModSettings.TurbulenceEvent && (World.Weather == Weather.Clearing || World.Weather == Weather.Raining || World.Weather == Weather.ThunderStorm || World.Weather == Weather.Blizzard))
            {
                if (Game.GameTime > _nextForce)
                {
                    float _force = 0;

                    switch (World.Weather)
                    {
                        case Weather.Clearing:
                            _force = 0.5f;
                            break;
                        case Weather.Raining:
                            _force = 0.75f;
                            break;
                        case Weather.ThunderStorm:
                        case Weather.Blizzard:
                            _force = 1;
                            break;
                    }

                    _force *= (Vehicle.HeightAboveGround / 20f);

                    if (_force > 1)
                        _force = 1;

                    Vehicle.ApplyForce(Vector3.RandomXYZ() * _force, Vector3.RandomXYZ() * _force);

                    _nextForce = Game.GameTime + 100;
                }
            }

            if (Properties.AreFlyingCircuitsBroken)
            {
                var force = Vehicle.UpVector;

                if (!Vehicle.IsUpsideDown)
                    force.Z = -force.Z;

                force *= 12 * Game.LastFrameTime;

                Vehicle.ApplyForce(force, Vector3.Zero);

                if(Vehicle.HeightAboveGround < 2)
                    SetFlyMode(false);

                return;
            }

            // Reset force to be applied
            _forceToBeApplied = Vector3.Zero;

            if (Vehicle.IsEngineRunning && (!Mods.IsDMC12 || !Players.HoverModeWheels.IsPlaying))
            {
                // Process boost
                HandleBoosting();

                // Process up/down movement
                UpDown();

                // Altitude holder
                if (Properties.IsAltitudeHolding)
                    HandleAltitudeHolding();
            }            

            // Apply force
            Vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);

            // Force fly mode
            if (ModSettings.ForceFlyMode && Utils.PlayerVehicle == Vehicle)
                VehicleControl.SetDeluxoFlyMode(Vehicle, 1f);

            // Force brake lights on if flying
            if (Properties.IsFlying)
                Vehicle.AreBrakeLightsOn = true;

            if (_startHoverGlowLater && Vehicle.IsEngineRunning)
                SpawnHoverGlow();
        }

        public void HandleBoosting()
        {
            // First of all, check if vehicle is in fly mode, if its not just return
            if (Utils.PlayerVehicle != Vehicle) 
                return;

            // If the Handbrake control is pressed
            // Using this so that controllers are also supported
            if(Game.IsControlPressed(ModControls.HoverBoost) && Vehicle.IsEngineRunning)
            {
                // Boost!
                Boost();

                Properties.IsHoverBoosting = true;
            }
            else
            {
                // Set vent effect invisible
                Props.HoverModeVentsGlow?.Delete();

                // Reset flag
                _hasPlayedBoostSound = false;

                Properties.IsHoverBoosting = false;
            }
        }

        private void HandleAltitudeHolding()
        {            
            var velocity = Vehicle.Velocity;
            var zVel = velocity.Z;

            // Apply opposite of zVel.
            _forceToBeApplied.Z += -zVel;
        }

        private void UpDown()
        {
            // What are you doing 
            if (Utils.PlayerVehicle != Vehicle)
                return;

            // Get how much value is moved up/down
            float upNormal = 0;

            if (Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleUp))
            {
                if (Vehicle.DecreaseSpeedAndWait(Vehicle.IsGoingForward() ? 20 : 10))
                {
                    Game.DisableControlThisFrame(Control.VehicleSelectNextWeapon);
                    Game.DisableControlThisFrame(Control.VehicleFlyThrottleUp);

                    if (!Properties.IsLanding && Mods.IsDMC12 && !_hoverGlowUp)
                    {
                        SpawnHoverGlow();
                        _hoverGlowUp = true;
                    }

                    upNormal = 1;
                }
            }
            else if (Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleDown))
            {
                if (Vehicle.DecreaseSpeedAndWait(Vehicle.IsGoingForward() ? 10 : 20))
                {
                    Game.DisableControlThisFrame(Control.VehicleSelectNextWeapon);
                    Game.DisableControlThisFrame(Control.VehicleFlyThrottleDown);

                    upNormal = -1;
                }
            }

            if (upNormal == 0 && _hoverGlowUp)
                _hoverGlowUp = false;

            // Apply force
            GoUpDown(upNormal);
        }

        public void GoUpDown(float upNormal)
        {
            if (!Properties.IsLanding || upNormal == 1)
                _forceToBeApplied += Vehicle.UpVector * 15f * upNormal * Game.LastFrameTime;
            else if (upNormal == -1)
                _forceToBeApplied.Z = -Vehicle.Velocity.Z - 1;

            if (upNormal != 0 && !Properties.IsHoverBoosting)
            {
                _forceToBeApplied.Y = -Vehicle.Velocity.Y;
                _forceToBeApplied.X = -Vehicle.Velocity.X;
            }
        }

        private void UnderbodyLights()
        {
            // Check if we're up to next light change
            if (Game.GameTime < _nextLightChange) return;

            // Update light index
            _currentLightIndex--;

            if (_currentLightIndex < 0)
                _currentLightIndex = 4;

            // Update the actual lights
            for(int i = Props.HoverModeUnderbodyLights.Count-1; i >= 0; i--)
            {
                if (_currentLightIndex != i)
                    Props.HoverModeUnderbodyLights[i]?.Delete();
                else
                    Props.HoverModeUnderbodyLights[i]?.SpawnProp();
            }

            // Update next change time
            _nextLightChange = Game.GameTime + 100;
        }

        public void Boost()
        {
            // Set the vent boost effect visible
            Props.HoverModeVentsGlow?.SpawnProp();

            // Play boost sound
            if (!_hasPlayedBoostSound)
            {
                Sounds.HoverModeBoost?.Play();
                _hasPlayedBoostSound = true;
            }

            // Apply force
            _forceToBeApplied += Vehicle.ForwardVector * 12f * Game.LastFrameTime;
        }

        public override void Stop()
        {
            Players.HoverModeWheels?.Stop();
        }

        public override void Dispose()
        {
            Players.HoverModeWheels?.Dispose();
        }
    }
}
