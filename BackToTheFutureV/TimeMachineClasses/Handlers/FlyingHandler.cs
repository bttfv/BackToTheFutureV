using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using Control = GTA.Control;

namespace BackToTheFutureV
{
    internal class FlyingHandler : HandlerPrimitive
    {
        private bool _hasPlayedBoostSound;

        private readonly NativeInput _flyModeInput;

        private int _nextModeChangeAllowed;

        private int _nextForce;

        private bool _startHoverGlowLater;

        private bool _landingSmoke;

        private Vector3 _forceToBeApplied = Vector3.Zero;

        private bool _reload;

        private Hash _defaultHorn;

        public FlyingHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            if (Mods.HoverUnderbody == ModState.On)
                OnHoverUnderbodyToggle();

            _defaultHorn = Function.Call<Hash>(Hash._GET_VEHICLE_HORN_HASH, Vehicle);

            _flyModeInput = new NativeInput(ModControls.Hover);
            _flyModeInput.OnControlLongPressed += OnFlyModeControlJustLongPressed;
            _flyModeInput.OnControlPressed += OnFlyModeControlJustPressed;

            Events.SetFlyMode += SetFlyMode;
            Events.SetAltitudeHold += SetHoverMode;
            Events.OnHoverUnderbodyToggle += OnHoverUnderbodyToggle;

            Events.SimulateHoverBoost += SimulateHoverBoost;
            Events.SimulateHoverGoingUpDown += SpawnHoverGlow;

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

        private void SimulateHoverBoost(bool state)
        {
            Properties.IsHoverBoosting = state;

            if (state)
                Boost();
            else
            {
                _hasPlayedBoostSound = false;
                Sounds.HoverModeBoost?.Stop();
                Props.HoverModeVentsGlow?.Delete();
            }
        }

        public void OnHoverUnderbodyToggle(bool reload = false)
        {
            _reload = reload;

            if (Mods.HoverUnderbody == ModState.Off && Properties.IsFlying)
                SetFlyMode(false, true);

            if (Players.HoverModeWheels != null)
            {
                Players.HoverModeWheels.OnPlayerCompleted -= OnCompleted;
                Players.HoverModeWheels?.Dispose();
                Players.HoverModeWheels = null;
            }

            if (Mods.HoverUnderbody == ModState.On)
            {
                Players.HoverModeWheels = new WheelAnimationPlayer(TimeMachine);
                Players.HoverModeWheels.OnPlayerCompleted += OnCompleted;

                Properties.AreFlyingCircuitsBroken = false;

                return;
            }

            if (Properties.IsFlying)
                SetFlyMode(false, true);

            Props.HoverModeUnderbodyLights?.Delete();
        }

        private void OnCompleted()
        {
            if (Properties.IsFlying)
            {
                _startHoverGlowLater = !Vehicle.IsEngineRunning;

                if (!_startHoverGlowLater)
                    SpawnHoverGlow();
            }
        }

        private void SpawnHoverGlow(bool state = true)
        {
            Properties.IsHoverGoingUpDown = state;

            if (!state)
                return;

            if (Sounds.HoverModeUp.IsAnyInstancePlaying)
                return;

            if (Mods.Wheel != WheelType.RedInvisible && Props.HoverModeWheelsGlow != null && !Props.HoverModeWheelsGlow.IsSpawned)
                Props.HoverModeWheelsGlow?.SpawnProp();

            Sounds.HoverModeUp?.Play();
            _startHoverGlowLater = false;
        }

        private void OnFlyModeControlJustLongPressed()
        {
            if (ModControls.LongPressForHover)
            {
                if (Mods.HoverUnderbody == ModState.On && Properties.CanConvert && FusionUtils.PlayerVehicle == Vehicle && Game.GameTime > _nextModeChangeAllowed && !Properties.IsEngineStalling)
                {
                    if (Properties.AreFlyingCircuitsBroken)
                    {
                        TextHandler.ShowHelp("HoverDamaged");

                        return;
                    }

                    SetFlyMode(!Properties.IsFlying);

                    _nextModeChangeAllowed = Game.GameTime + 2000;
                }
            }
        }

        private void OnFlyModeControlJustPressed()
        {
            if (!ModControls.LongPressForHover)
            {
                if (Mods.HoverUnderbody == ModState.On && Properties.CanConvert && FusionUtils.PlayerVehicle == Vehicle && Game.GameTime > _nextModeChangeAllowed && !Properties.IsEngineStalling)
                {
                    if (Properties.AreFlyingCircuitsBroken)
                    {
                        TextHandler.ShowHelp("HoverDamaged");

                        return;
                    }

                    SetFlyMode(!Properties.IsFlying);

                    _nextModeChangeAllowed = Game.GameTime + 2000;
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

                TextHandler.ShowHelp("HoverDamaged");

                return;
            }

            if (Properties.IsOnTracks)
                Events.SetStopTracks?.Invoke(3000);

            Properties.IsFlying = open;

            Properties.IsLanding = ModSettings.LandingSystem && !Properties.IsFlying && !Properties.AreFlyingCircuitsBroken && !instant && Vehicle.HeightAboveGround < 20 && Vehicle.HeightAboveGround > 0.5f && !Vehicle.IsUpsideDown && VehicleControl.GetDeluxoTransformation(Vehicle) > 0;

            if (instant)
                Players.HoverModeWheels?.SetInstant(open);
            else
                Players.HoverModeWheels?.Play(open);

            // undocced native, changes delxuo transformation
            // from land to hover
            // (DOES NOT CHANGE FLY MODE!)
            if (!Properties.IsLanding)
                Function.Call((Hash)0x438b3d7ca026fe91, Vehicle, Properties.IsFlying ? 1f : 0f);
            else
                TextHandler.ShowHelp("VTOLTip", true, new ControlInfo(ModControls.HoverVTOL).Button);

            if (Properties.IsFlying && !instant)
            {
                Sounds.HoverModeOn?.Play();
                Particles?.HoverModeSmoke?.ForEach(x => x?.Play());
            }
            else if (!Properties.IsFlying && !instant)
            {
                if (!Properties.IsFlying)
                    Sounds.HoverModeOff?.Play();

                Props.HoverModeWheelsGlow?.Delete();
            }

            _landingSmoke = false;

            if (Mods.IsDMC12)
                Function.Call(Hash._FORCE_VEHICLE_ENGINE_AUDIO, Vehicle, Properties.IsFlying ? "DELUXO" : "VIRGO");

            if (Mods.IsDMC12)
                Function.Call(Hash.OVERRIDE_VEH_HORN, Vehicle, true, _defaultHorn);

            if (!Properties.IsLanding && !Properties.IsFlying)
                Properties.TorqueMultiplier = 1.4f;

            if (!Properties.IsFlying && Properties.IsAltitudeHolding)
                Properties.IsAltitudeHolding = false;

            Props.HoverModeVentsGlow?.Delete();
            Props.HoverModeWheelsGlow?.Delete();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == ModControls.HoverAltitudeHold && FusionUtils.PlayerVehicle == Vehicle && Properties.IsFlying)
                SetHoverMode(!Properties.IsAltitudeHolding);
        }

        public void SetHoverMode(bool mode)
        {
            Properties.IsAltitudeHolding = mode;
            TextHandler.ShowHelp("AltitudeHoldChange", true, Properties.IsAltitudeHolding ? TextHandler.GetLocalizedText("On") : TextHandler.GetLocalizedText("Off"));
        }

        public override void Tick()
        {
            if (Mods.HoverUnderbody == ModState.Off)
                return;

            // Automatically fold wheels in if fly mode is exited in any other way
            // Example: getting out of vehicle, flipping vehicle over, etc
            if (Properties.IsFlying && VehicleControl.GetDeluxoTransformation(Vehicle) <= 0)
                SetFlyMode(false);

            // Automatically set fly mode if deluxo is transformed in any other way.
            if (!Properties.IsFlying && VehicleControl.GetDeluxoTransformation(Vehicle) > 0 && !Properties.IsLanding)
                SetFlyMode(true);

            // Process the wheel animations
            Players.HoverModeWheels?.Tick();

            if (!Vehicle.IsVisible)
            {
                if (Mods.IsDMC12 && Props.HoverModeUnderbodyLights.IsSequencePlaying)
                    Props.HoverModeUnderbodyLights.Delete();

                return;
            }

            // Process underbody lights
            if (Mods.IsDMC12 && !Props.HoverModeUnderbodyLights.IsSequencePlaying)
                Props.HoverModeUnderbodyLights.Play();

            if (Properties.IsLanding)
            {
                // Reset force to be applied
                _forceToBeApplied = Vector3.Zero;

                // Process up/down movement
                UpDown();

                // Apply force
                Vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);

                if (Vehicle.HeightAboveGround < 2 && !_landingSmoke)
                {
                    Particles?.HoverModeSmoke?.ForEach(x => x?.Play());
                    _landingSmoke = true;
                }

                if (!Vehicle.IsUpsideDown && Vehicle.HeightAboveGround > 0.5f && Vehicle.HeightAboveGround < 20)
                    return;

                SetFlyMode(false, true);
            }

            if (!Properties.IsFlying)
                return;

            if (Properties.HasBeenStruckByLightning || (ModSettings.TurbulenceEvent && (World.Weather == Weather.Clearing || World.Weather == Weather.Raining || World.Weather == Weather.ThunderStorm || World.Weather == Weather.Blizzard)))
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

                    if (Properties.HasBeenStruckByLightning)
                        _force = 1;

                    _force *= (Vehicle.HeightAboveGround / 20f);

                    if (_force > 1)
                        _force = 1;

                    Vehicle.ApplyForce(Vector3.RandomXYZ() * _force, Vector3.RandomXYZ() * _force);

                    _nextForce = Game.GameTime + 100;
                }
            }

            if (Mods.IsDMC12 && Props.HoverModeVentsGlow.IsSpawned && Driver == null)
                Props.HoverModeVentsGlow?.Delete();

            if (Properties.AreFlyingCircuitsBroken)
            {
                Vector3 force = Vehicle.UpVector;

                if (!Vehicle.IsUpsideDown)
                    force.Z = -force.Z;

                force *= 18 * Game.LastFrameTime;

                Vehicle.ApplyForce(force, Vector3.Zero);

                if (Vehicle.HeightAboveGround < 2)
                    SetFlyMode(false);

                return;
            }

            // Reset force to be applied
            _forceToBeApplied = Vector3.Zero;

            if (Vehicle.IsEngineRunning && !Players.HoverModeWheels.IsPlaying)
            {
                // Process boost
                HandleBoosting();

                // Process up/down movement
                UpDown();

                // Altitude holder
                if (Properties.IsAltitudeHolding)
                    HandleAltitudeHolding();

                if (Game.IsControlPressed(Control.VehicleHandbrake) && !Game.IsControlPressed(Control.VehicleAccelerate) && !Game.IsControlPressed(Control.VehicleBrake) && Vehicle.GetMPHSpeed() > 1)
                    Properties.Boost = Vehicle.IsGoingForward() ? -0.4f : 0.4f;
            }

            // Apply force
            Vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);

            // Force fly mode
            if (ModSettings.ForceFlyMode && FusionUtils.PlayerVehicle == Vehicle)
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
            if (FusionUtils.PlayerVehicle != Vehicle)
                return;

            // If the Handbrake control is pressed
            // Using this so that controllers are also supported
            if (Game.IsControlPressed(ModControls.HoverBoost) && Game.IsControlPressed(Control.VehicleAccelerate) && Vehicle.IsEngineRunning)
            {
                if (Game.IsControlJustPressed(ModControls.HoverBoost))
                    FusionUtils.SetPadShake(100, 200);

                // Boost!
                if (Vehicle.GetMPHSpeed() <= 95)
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
            Vector3 velocity = Vehicle.Velocity;
            float zVel = velocity.Z;

            // Apply opposite of zVel.
            _forceToBeApplied.Z += -zVel;
        }

        private void UpDown()
        {
            // What are you doing 
            if (FusionUtils.PlayerVehicle != Vehicle)
                return;

            // Get how much value is moved up/down
            float upNormal = 0;

            if (Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleUp))
            {
                if (Vehicle.DecreaseSpeedAndWait(Vehicle.IsGoingForward() ? 20 : 10))
                {
                    Game.DisableControlThisFrame(Control.VehicleSelectNextWeapon);
                    Game.DisableControlThisFrame(Control.VehicleFlyThrottleUp);

                    if (!Properties.IsLanding && !Properties.IsHoverGoingUpDown)
                        SpawnHoverGlow();

                    upNormal = 1;
                }
                else
                    FusionUtils.SetPadShake(100, 80);
            }
            else if (Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleDown))
            {
                if (Vehicle.DecreaseSpeedAndWait(Vehicle.IsGoingForward() ? 10 : 20))
                {
                    Game.DisableControlThisFrame(Control.VehicleSelectNextWeapon);
                    Game.DisableControlThisFrame(Control.VehicleFlyThrottleDown);

                    upNormal = -1;
                }
                else
                    FusionUtils.SetPadShake(100, 80);
            }

            if (upNormal == 0 && Properties.IsHoverGoingUpDown)
                Properties.IsHoverGoingUpDown = false;

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
