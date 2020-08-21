using BackToTheFutureV.Entities;
using BackToTheFutureV.Memory;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class FlyingHandler : Handler
    {
        public bool Open { get; private set; }
        public bool IsFlying { get; private set; }
        public bool IsBoosting { get; private set; }
        public bool IsAltitudeHolding { get; private set; }
        public bool CanConvert { get; set; } = true;
        public bool IsPlayingAnim => wheelAnims != null && wheelAnims.IsPlaying;

        public bool FlyingCircuitsBroken;

        private WheelAnimationPlayer wheelAnims;

        private readonly AudioPlayer _flyOn;
        private readonly AudioPlayer _flyOff;
        private readonly AudioPlayer _upSound;
        private readonly AudioPlayer _boostSound;

        private bool _hasPlayedBoostSound;

        private readonly AnimateProp _hoverGlowing;
        private readonly AnimateProp ventGlowing;

        private readonly NativeInput _flyModeInput;

        private int _nextModeChangeAllowed;

        private int _nextForce;

        private bool _startHoverGlowLater;

        private readonly List<PtfxEntityPlayer> _smokeParticles = new List<PtfxEntityPlayer>();

        private readonly List<AnimateProp> _underbodyLights = new List<AnimateProp>();
        private int _currentLightIndex;
        private int _nextLightChange;

        private Vector3 _forceToBeApplied = Vector3.Zero;

        public FlyingHandler(TimeCircuits circuits) : base(circuits)
        {
            if (Mods.HoverUnderbody == ModState.On)
                LoadWheelAnim();

            _flyOn = circuits.AudioEngine.Create("bttf2/hover/toHover.wav", Presets.Exterior);
            _flyOff = circuits.AudioEngine.Create("bttf2/hover/toRegular.wav", Presets.Exterior);
            _upSound = circuits.AudioEngine.Create("bttf2/hover/hoverUp.wav", Presets.Exterior);
            _boostSound = circuits.AudioEngine.Create("bttf2/hover/boost.wav", Presets.Exterior);

            foreach (var wheelPos in Utils.GetWheelPositions(Vehicle))
            {
                var ptfx = new PtfxEntityPlayer("cut_trevor1", "cs_meth_pipe_smoke", Vehicle, wheelPos.Value, new Vector3(-90, 0, 0), 7f);
                _smokeParticles.Add(ptfx);
            }

            for(int i = 1; i < 6; i++)
            {
                if (!ModelHandler.UnderbodyLights.TryGetValue(i, out var model)) 
                    continue;

                ModelHandler.RequestModel(model);
                var prop = new AnimateProp(Vehicle, model, Vector3.Zero, Vector3.Zero);
                _underbodyLights.Add(prop);
            }

            _flyModeInput = new NativeInput((GTA.Control)357);
            _flyModeInput.OnControlLongPressed += OnFlyModeControlJustPressed;

            _hoverGlowing = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.HoverGlowing), Vector3.Zero, Vector3.Zero)
            {
                Duration = 1.7f
            };

            ventGlowing = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.VentGlowing), Vector3.Zero, Vector3.Zero);
        }

        public void LoadWheelAnim()
        {
            DeleteWheelAnim();

            wheelAnims = new WheelAnimationPlayer(TimeCircuits);
            wheelAnims.OnAnimCompleted += OnAnimCompleted;
        }

        public void DeleteWheelAnim()
        {
            if (IsFlying)
                SetFlyMode(false, true);

            if (wheelAnims != null)
            {
                wheelAnims.OnAnimCompleted -= OnAnimCompleted;
                wheelAnims.Dispose();
                wheelAnims = null;
            }

            foreach (AnimateProp x in _underbodyLights)
                x?.DeleteProp();

            FlyingCircuitsBroken = false;
        }
        
        private void OnAnimCompleted()
        {
            if (Open)
            {
                _startHoverGlowLater = !Vehicle.IsEngineRunning;

                if (!_startHoverGlowLater)
                    SpawnHoverGlow();
            }
        }

        private void SpawnHoverGlow()
        {
            _hoverGlowing.SpawnProp();
            _upSound.Play();
            _startHoverGlowLater = false;
        }

        private void OnFlyModeControlJustPressed()
        {
            if(Mods.HoverUnderbody == ModState.On && CanConvert && !FlyingCircuitsBroken && !Game.IsControlPressed(GTA.Control.CharacterWheel) && !Main.MenuPool.IsAnyMenuOpen() && Main.PlayerVehicle == Vehicle && Game.GameTime > _nextModeChangeAllowed)
            {
                if (TimeCircuits.HasBeenStruckByLightning)
                    return;

                SetFlyMode(!Open);

                _nextModeChangeAllowed = Game.GameTime + 1500;
            }
        }

        public void SetFlyMode(bool open, bool instant = false)
        {
            Open = open;

            if (open && TimeCircuits.HasBeenStruckByLightning)
                return;

            if (wheelAnims == null)
                LoadWheelAnim();

            if (instant)
                wheelAnims.SetInstant(open);
            else
                wheelAnims.Play(open);

            // undocced native, changes delxuo transformation
            // from land to hover
            // (DOES NOT CHANGE FLY MODE!)
            Function.Call((Hash)0x438b3d7ca026fe91, Vehicle, Open ? 1f : 0f);

            if (Open && !instant)
            {
                _flyOn.Play();
                _smokeParticles.ForEach(x => x.Play());
            }
            else if(!Open && !instant)
            {
                _flyOff.Play();
                _hoverGlowing.DeleteProp();
            }

            IsFlying = Open;

            Function.Call(Hash._FORCE_VEHICLE_ENGINE_AUDIO, Vehicle, IsFlying ? "DELUXO" : "VIRGO");

            if (!IsFlying)
            {
                Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, false);
                Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, false);
            }

            ventGlowing?.DeleteProp();
            _hoverGlowing?.DeleteProp();
        }

        public override void KeyPress(Keys key)
        {
            if(key == Keys.G && Main.PlayerVehicle == Vehicle)
            {
                SetHoverMode(!IsAltitudeHolding);
            }
        }

        public void SetHoverMode(bool mode)
        {
            IsAltitudeHolding = mode;
            Utils.DisplayHelpText($"{Game.GetLocalizedString("BTTFV_AltitudeHoldingModeChange")} {(IsAltitudeHolding ? Game.GetLocalizedString("BTTFV_On") : Game.GetLocalizedString("BTTFV_Off"))}");
        }

        public override void Process()
        {
            if (Mods.HoverUnderbody == ModState.Off)
                return;

            // Automatically fold wheels in if fly mode is exited in any other way
            // Example: getting out of vehicle, flipping vehicle over, etc
            if (IsFlying && VehicleControl.GetDeluxoTransformation(Vehicle) <= 0 )
            {
                SetFlyMode(false);
            }

            // Automatically set fly mode if deluxo is transformed in any other way.
            if (!IsFlying && VehicleControl.GetDeluxoTransformation(Vehicle) > 0)
            {
                SetFlyMode(true);
            }

            // Process the wheel animations
            wheelAnims?.Process();

            _flyModeInput.Process();

            if (Main.PlayerVehicle == Vehicle)
                Function.Call(Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player.Handle, false);
            else
                Function.Call(Hash.SET_PLAYER_CAN_DO_DRIVE_BY, Game.Player.Handle, true);

            if (Vehicle == null || !Vehicle.IsVisible) return;

            // Process underbody lights
            UnderbodyLights();

            if (!IsFlying)
                return;

            if (ModSettings.TurbolenceEvent && (World.Weather == Weather.Clearing || World.Weather == Weather.Raining || World.Weather == Weather.ThunderStorm || World.Weather == Weather.Blizzard))
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

            if (TimeCircuits.HasBeenStruckByLightning)
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

            // Process boost
            HandleBoosting();

            // Process up/down movement
            UpDown();
            
            // Altitude holder
            HandleAltitudeHolding();

            // Apply force
            Vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);

            if(ModSettings.ForceFlyMode)
            {
                // Force fly mode
                VehicleControl.SetDeluxoFlyMode(Vehicle, 1f);
            }

            // Force brake lights on if flying
            if (IsFlying)
            {
                Vehicle.AreBrakeLightsOn = true;
            }

            if (_startHoverGlowLater && Vehicle.IsEngineRunning)
                SpawnHoverGlow();
        }

        public void HandleBoosting()
        {
            // First of all, check if vehicle is in fly mode, if its not just return
            if (Main.PlayerVehicle != Vehicle) return;

            // If the Handbrake control is pressed
            // Using this so that controllers are also supported
            if(Game.IsControlPressed(GTA.Control.VehicleHandbrake) && Vehicle.IsEngineRunning)
            {
                // Boost!
                Boost();

                IsBoosting = true;
            }
            else
            {
                // Set vent effect invisible
                ventGlowing.DeleteProp();

                // Reset flag
                _hasPlayedBoostSound = false;

                IsBoosting = false;
            }
        }

        private void HandleAltitudeHolding()
        {
            if (!IsAltitudeHolding || !IsFlying)
            {
                return;
            }

            var velocity = Vehicle.Velocity;
            var zVel = velocity.Z;

            // Apply opposite of zVel.
            _forceToBeApplied.Z += -zVel;
        }

        private void UpDown()
        {
            // What are you doing 
            if (!IsFlying || Main.PlayerVehicle != Vehicle && IsAltitudeHolding) return;

            // Get how much value is moved up/down
            float upNormal = 0;

            if (Game.IsControlPressed(GTA.Control.VehicleSelectNextWeapon) && Game.IsControlPressed(GTA.Control.VehicleFlyThrottleUp))
            {
                Game.DisableControlThisFrame(GTA.Control.VehicleSelectNextWeapon);
                Game.DisableControlThisFrame(GTA.Control.VehicleFlyThrottleUp);

                upNormal = 1;
            }
            else if (Game.IsControlPressed(GTA.Control.VehicleSelectNextWeapon) && Game.IsControlPressed(GTA.Control.VehicleFlyThrottleDown))
            {
                Game.DisableControlThisFrame(GTA.Control.VehicleSelectNextWeapon);
                Game.DisableControlThisFrame(GTA.Control.VehicleFlyThrottleDown);

                upNormal = -1;
            }

            // Apply force
            GoUpDown(upNormal);
        }

        public void GoUpDown(float upNormal)
        {
            _forceToBeApplied += Vehicle.UpVector * 15f * upNormal * Game.LastFrameTime;
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
            for(int i = _underbodyLights.Count-1; i >= 0; i--)
            {
                if (_currentLightIndex != i)
                    _underbodyLights[i]?.DeleteProp();
                else
                    _underbodyLights[i].SpawnProp();
            }

            // Update next change time
            _nextLightChange = Game.GameTime + 100;
        }

        public void Boost()
        {
            // Set the vent boost effect visible
            ventGlowing.SpawnProp();

            // Play boost sound
            if (!_hasPlayedBoostSound)
            {
                _boostSound.Play();
                _hasPlayedBoostSound = true;
            }

            // Apply force
            _forceToBeApplied += Vehicle.ForwardVector * 12f * Game.LastFrameTime;
        }

        public override void Stop()
        {
            wheelAnims?.Stop();
        }

        public override void Dispose()
        {
            wheelAnims?.Dispose();
            _hoverGlowing?.Dispose();
            ventGlowing?.Dispose();

            foreach(var prop in _underbodyLights)
            {
                prop.Dispose();
            }
        }
    }
}
