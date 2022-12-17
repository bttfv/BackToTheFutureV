using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using KlangRageAudioLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AudioPlayer = KlangRageAudioLibrary.AudioPlayer;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;

// TODO: Improve acceleration sound handling (first sound plays only when u burst tires)
// TODO: Fix reverse prediction
// TODO: Reverse based on real acceleration

namespace BackToTheFutureV
{
    internal delegate void OnGearUp();

    internal delegate void OnGearDown();

    internal delegate void OnGearChanged();

    internal class EngineHandler : HandlerPrimitive
    {
        #region PUBLIC_FIELDS

        /// <summary>
        /// Whether vehicle engine is idling.
        /// </summary>
        public bool IsIdle { get; private set; }

        /// <summary>
        /// Whether vehicle engine is revving.
        /// </summary>
        public bool IsRevving { get; private set; }

        /// <summary>
        /// Whether vehicle driving neutral.
        /// </summary>
        public bool IsNeutral { get; private set; }

        /// <summary>
        /// Whether vehicle is accelerating.
        /// </summary>
        public bool IsAccelerating { get; private set; }

        /// <summary>
        /// Whether vehicle is breaking.
        /// </summary>
        public bool IsBraking { get; private set; }

        /// <summary>
        /// Whether vehicle is driving reverse.
        /// </summary>
        public bool IsReversing { get; private set; }

        /// <summary>
        /// Returns vehicle engine rpm.
        /// </summary>
        public float EngineRpm => Vehicle.CurrentRPM;

        /// <summary>
        /// Returns vehicle speed relatively to world in MPH.
        /// </summary>
        public float Speed => Vehicle.GetMPHSpeed();

        /// <summary>
        /// Returns rear wheel speed in MPH.
        /// </summary>
        public float WheelSpeed { get; private set; }

        /// <summary>
        /// Returns current vehicle gear number.
        /// </summary>
        public int CurrentGear => Vehicle.CurrentGear;

        /// <summary>
        /// Returns previous vehicle gear number.
        /// </summary>
        public int PreviousGear { get; private set; }

        /// <summary>
        /// Returns current vehicle acceleration.
        /// </summary>
        public float Acceleration { get; private set; }

        /// <summary>
        /// Called when vehicle switching gear up.
        /// </summary>
        public OnGearUp OnGearUp { get; }

        /// <summary>
        /// Called when vehicle switching gear down.
        /// </summary>
        public OnGearDown OnGearDown { get; }

        /// <summary>
        /// Called when vehicle switching gear.
        /// </summary>
        public OnGearChanged OnGearChanged { get; }

        #endregion

        #region PRIVATE_FIELDS

        private readonly List<AudioPlayer> _engineSounds;
        private readonly List<AudioPlayer> _accelSounds;

        private readonly AudioPlayer _engineIdleSound;
        private readonly AudioPlayer _engineRevvingSound;

        private readonly AudioPlayer _engineAccel1Sound;
        private readonly AudioPlayer _engineAccel2Sound;
        private readonly AudioPlayer _engineAccel3Sound;
        private readonly AudioPlayer _engineAccel4Sound;

        private readonly AudioPlayer _engineDecelSound;
        private readonly AudioPlayer _engineNeutralSound;
        private readonly AudioPlayer _engineReverseSound;

        private AudioPlayer _currentAccel;

        private const string SoundsFolder = "general\\engine\\";

        // Doesnt allow to play rev more than once when player revving engine
        private bool _isRevPlayed;

        // Timer checks for acceleration caclulation
        private float _prevAccel;
        private int _check;
        private int _checkInterval;

        // Used for acceleration "prediction"
        private float _carAngle;
        private bool _possibleFastAccel;
        private bool _isPosChanged;
        private readonly List<MaterialHash> _allowedSurfaces;
        private List<Vector3> _wheelPos;
        private List<Vector3> _wheelPosPrev;

        #endregion

        #region CONSTRUCTORS

        public EngineHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            OnGearUp += OnGearUpEvent;
            OnGearDown += OnGearDownEvent;
            OnGearChanged += OnGearChangedEvent;

            AudioEngine audioEngine = Sounds.AudioEngine;
            _engineSounds = new List<AudioPlayer>
            {
                (_engineIdleSound = audioEngine.Create($"{SoundsFolder}idle.wav", Presets.ExteriorLoop)),
                (_engineRevvingSound = audioEngine.Create($"{SoundsFolder}revving.wav", Presets.Exterior)),

                (_engineAccel1Sound = audioEngine.Create($"{SoundsFolder}acceleration1.wav", Presets.Exterior)),
                (_engineAccel2Sound = audioEngine.Create($"{SoundsFolder}acceleration2.wav", Presets.Exterior)),
                (_engineAccel3Sound = audioEngine.Create($"{SoundsFolder}acceleration3.wav", Presets.Exterior)),
                (_engineAccel4Sound = audioEngine.Create($"{SoundsFolder}acceleration4.wav", Presets.Exterior)),

                (_engineDecelSound = audioEngine.Create($"{SoundsFolder}deceleration.wav", Presets.Exterior)),
                (_engineNeutralSound = audioEngine.Create($"{SoundsFolder}neutral.wav", Presets.ExteriorLoop)),
                (_engineReverseSound = audioEngine.Create($"{SoundsFolder}reverse.wav", Presets.Exterior))
            };

            // Configure audio properties
            foreach (AudioPlayer sound in _engineSounds)
            {
                sound.SourceBone = "engine";
                sound.Volume = 0.65f;
                sound.MinimumDistance = 3f;
                sound.StopFadeOut = true;
            }

            _accelSounds = new List<AudioPlayer>
            {
                _engineAccel1Sound, _engineAccel2Sound, _engineAccel3Sound, _engineAccel4Sound
            };

            _engineNeutralSound.FadeInMultiplier = 0.05f;
            _engineIdleSound.FadeInMultiplier = 0.05f;

            _engineIdleSound.FadeInMultiplier = 0.6f;
            _engineNeutralSound.FadeOutMultiplier = 0.4f;
            _engineDecelSound.FadeOutMultiplier = 0.3f;
            _engineReverseSound.FadeOutMultiplier = 0.7f;

            _engineNeutralSound.MinimumDistance = 2f;
            _engineIdleSound.MinimumDistance = 1f;
            _engineIdleSound.Volume = 0.35f;

            foreach (AudioPlayer sound in _accelSounds)
            {
                sound.FadeOutMultiplier = 0.9f;
            }

            // Create allowed materials list for acceleration prediction
            _allowedSurfaces = new List<MaterialHash>
            {
                MaterialHash.Concrete,
                MaterialHash.ConcreteDusty,
                MaterialHash.MetalSolidRoadSurface,
                MaterialHash.Brick,
                MaterialHash.PavingSlab,
                MaterialHash.Tarmac,
                MaterialHash.TarmacPainted
            };
        }

        #endregion

        #region PUBLIC_METHODS

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {
            if (_engineSounds.Any(x => x == null))
            {
                return;
            }

            if (Vehicle.IsVisible == false || !Vehicle.IsEngineRunning || !ModSettings.PlayEngineSounds || Main.FirstTick)
            {
                Stop();
                return;
            }

            // Update wheel positions
            _wheelPos = new List<Vector3>
            {
                Vehicle.Bones["wheel_lf"].Position,
                Vehicle.Bones["wheel_rf"].Position,
                Vehicle.Bones["wheel_rr"].Position,
                Vehicle.Bones["wheel_lr"].Position
            };

            // Slower interval works more correct on high speed
            _checkInterval = Speed > 5 ? 250 : 25;

            // Acceleration prediction, cuz if angle is too high e.g. > 6 car wont drive fast,
            // or if its not all wheels / on dirt
            PredictAccleration();

            // Get speed of rear wheel because delorean is rear wheel drive so
            // we can accurately detect acceleration
            GetAccelerationSound();

            // Parse some engine info
            CheckIdle();
            CheckRevving();
            CheckDeceleration();
            CheckNeutral();
            CheckReverse();

            // Parse engine information just before check
            CheckGear();
            if (Game.GameTime > _check - 5)
            {
                CheckAcceleration();

                Acceleration = VehicleAcceleration() * 100;
            }


            // Simulate Doppler Effect
            _engineSounds.ForEach(x => x.Velocity = Vehicle.Velocity);

            // Play neutral sound
            if (Speed > 35)
            {
                if (!_engineNeutralSound.IsAnyInstancePlaying)
                {
                    _engineNeutralSound.Play();
                }
            }
            else
            {
                _engineNeutralSound.Stop();
            }

            HandleRegularSounds();

            HandleHoverSounds();

            // We don't process acceleration each tick
            // because we won't get accurate results
            if (Game.GameTime < _check)
            {
                return;
            }

            HandleAccelerationSound();

            // Show engine information on screen
            Debug(-1);

            // Re-check acceleration and reset timer
            if (Game.GameTime <= _check)
            {
                return;
            }

            _prevAccel = FusionUtils.Magnitude(Vehicle.Velocity);

            // Next time check
            _check = Game.GameTime + _checkInterval;
        }

        public override void Stop()
        {
            _engineSounds.ForEach(x => x.Stop());
            _accelSounds.ForEach(x => x.Stop());
        }

        public override void Dispose()
        {
            _engineSounds.ForEach(x => x.Dispose());
            _accelSounds.ForEach(x => x.Dispose());
        }

        #endregion

        #region PRIVATE_METHODS

        #region SOUND_PLAYERS

        private void HandleRegularSounds()
        {
            if (Properties.IsFlying || Properties.IsLanding)
            {
                return;
            }

            // Play Idle sound
            if (IsIdle)
            {
                if (!_engineIdleSound.IsAnyInstancePlaying)
                {
                    _engineIdleSound.Play();
                }
            }
            else
            {
                _engineIdleSound.Stop();
            }

            // Play revving sound
            if (IsRevving)
            {
                if (!_engineRevvingSound.IsAnyInstancePlaying || (_engineRevvingSound.Last?.PlayPosition > 1000 && !_isRevPlayed))
                {
                    _engineRevvingSound.Play();
                    _isRevPlayed = true;
                }
            }
            else
            {
                _isRevPlayed = false;
            }

            // Play deceleration sound
            if (IsBraking && Speed > 30 && !Mods.Wheels.AnyBurst && !Game.IsControlPressed(Control.VehicleAccelerate) &&
                !Vehicle.IsInWater)
            {
                if (!_engineDecelSound.IsAnyInstancePlaying || _engineDecelSound.Last?.PlayPosition > 1000)
                {
                    _engineDecelSound.Play();
                }
            }
            else
            {
                _engineDecelSound.Stop();
            }

            // Play reverse sound
            if (IsReversing && WheelSpeed < -3 && _possibleFastAccel && !Mods.Wheels.AnyBurst &&
                !Vehicle.IsInWater)
            {
                if (!_engineReverseSound.IsAnyInstancePlaying)
                {
                    _engineReverseSound.Play();
                }
            }
            else
            {
                _engineReverseSound.Stop();
            }
        }

        private void HandleHoverSounds()
        {
            if (!Properties.IsFlying)
            {
                return;
            }

            // Play acceleration sound
            if (Properties.IsHoverBoosting)
            {
                if (_engineAccel2Sound.IsAnyInstancePlaying)
                {
                    if (_engineAccel2Sound.Last?.PlayPosition >= 7000)
                    {
                        _engineAccel2Sound.Play();
                    }
                }
                else
                {
                    _engineAccel2Sound.Play();
                }
            }
            else
            {
                _engineAccel2Sound.Stop();
            }

            // Play deceleration sound
            if (Game.IsControlPressed(Control.VehicleBrake) && Speed > 15 && Vehicle.RelativeVelocity().Y > 0)
            {
                if (!_engineDecelSound.IsAnyInstancePlaying || _engineDecelSound.Last?.PlayPosition > 1000)
                {
                    _engineDecelSound.Play();
                }
            }
            else
            {
                _engineDecelSound.Stop();
            }
        }

        private void HandleAccelerationSound()
        {
            if (Properties.IsFlying || Properties.IsLanding)
            {
                return;
            }

            //Stop acceleration sounds if car is braking / driving neutral
            if (Acceleration < -10f || !Game.IsControlPressed(Control.VehicleAccelerate) ||
                Game.IsControlPressed(Control.VehicleBrake) || Mods.Wheels.AnyBurst || Vehicle.IsInWater)
            {
                _accelSounds.ForEach(x => x.Stop());
                return;
            }

            // Play acceleration sound if player accelerates after driving neutral
            if (IsAccelerating)
            {
                if (_currentAccel != _engineAccel1Sound)
                {
                    if (_accelSounds.Any(x => x.IsAnyInstancePlaying))
                    {
                        if (_currentAccel.Last?.PlayPosition >= 7000)
                        {
                            _currentAccel.Play();
                        }
                    }
                    else if (!_accelSounds.Any(x => x.IsAnyInstancePlaying))
                    {
                        _currentAccel.Play();
                    }
                }
            }
        }

        #endregion

        #region EVENTS
        private void OnGearUpEvent()
        {
            if (Mods.Wheels.AnyBurst)
            {
                return;
            }

            if ((WheelSpeed < 12) || (Acceleration < 0.2f))
            {
                return;
            }

            if (Speed <= 3.5f && !_possibleFastAccel)
            {
                return;
            }

            if (_accelSounds.Any(x => x.IsAnyInstancePlaying))
            {
                if (_accelSounds.Any(x => x.Last?.PlayPosition < 1000))
                {
                    return;
                }
            }

            _currentAccel.Play();
        }

        private void OnGearDownEvent()
        {

        }

        private void OnGearChangedEvent()
        {
            if (PreviousGear < CurrentGear)
            {
                OnGearUp?.Invoke();
            }
            else
            {
                OnGearDown.Invoke();
            }

            PreviousGear = CurrentGear;
        }

        #endregion

        #region ENGINE_MANAGER
        private void CheckIdle()
        {
            if (!IsRevving && Speed < 3)
            {
                IsIdle = true;

                PreviousGear = -1;
            }
            else
            {
                IsIdle = false;
            }
        }

        private void CheckRevving()
        {
            if (IsPlayerRevving(Vehicle) && Speed < 1)
            {
                IsRevving = true;
            }
            else
            {
                IsRevving = false;
            }
        }

        private void CheckAcceleration()
        {
            if (Acceleration > 2.25f && WheelSpeed > 2f && EngineRpm < 0.90f && WheelSpeed > 15)
            {
                IsAccelerating = true;
            }
            else
            {
                IsAccelerating = false;
            }
        }

        private void CheckDeceleration()
        {
            if (IsPlayerBraking(Vehicle) && !IsReversing)
            {
                IsBraking = true;
            }
            else
            {
                IsBraking = false;
            }
        }

        private void CheckNeutral()
        {
            if (!IsAccelerating && !IsPlayerBraking(Vehicle) && !IsIdle)
            {
                IsNeutral = true;
            }
            else
            {
                IsNeutral = false;
            }
        }

        private void CheckReverse()
        {
            if (Vehicle.RelativeVelocity().Y < 0 && WheelSpeed < 0 && Game.IsControlPressed(Control.VehicleBrake))
            {
                IsReversing = true;
            }
            else
            {
                IsReversing = false;
            }
        }

        private void CheckGear()
        {
            if (PreviousGear != CurrentGear)
            {
                OnGearChanged?.Invoke();
            }
        }

        #endregion

        #region UTILS

        private static bool IsPlayerBraking(Vehicle vehicle, bool accountHandbrake = true)
        {
            if (FusionUtils.PlayerVehicle != vehicle)
            {
                return false;
            }

            if (accountHandbrake)
            {
                return Game.IsControlPressed(Control.VehicleBrake) || Game.IsControlPressed(Control.VehicleHandbrake);
            }

            return Game.IsControlPressed(Control.VehicleBrake) && !Game.IsControlPressed(Control.VehicleAccelerate);
        }

        private static bool IsPlayerRevving(Vehicle vehicle)
        {
            if (FusionUtils.PlayerVehicle != vehicle)
            {
                return false;
            }

            return Game.IsControlPressed(Control.VehicleAccelerate) && Game.IsControlPressed(Control.VehicleHandbrake)
                && !Game.IsControlPressed(Control.VehicleBrake);
        }

        private float VehicleAcceleration()
        {
            return (FusionUtils.Magnitude(Vehicle.Velocity) - _prevAccel) / _checkInterval;
        }

        private void PredictAccleration()
        {
            _possibleFastAccel = true; // get in default state

            _carAngle = Vehicle.Rotation.X;

            // Check if any wheel is not on allowed surface
            foreach (Vector3 wheelPos in _wheelPos)
            {
                RaycastResult surface = World.Raycast(wheelPos, wheelPos + new Vector3(0, 0, -10),
                    IntersectFlags.Everything, Vehicle);

                if (!_allowedSurfaces.Contains(surface.MaterialHash))
                {
                    _possibleFastAccel = false;
                }
            }

            // Check if car angle is too high or not on all wheels
            if (_possibleFastAccel)
            {
                if (_carAngle > 7 || !Vehicle.IsOnAllWheels || Mods.Wheels.AnyBurst)
                {
                    _possibleFastAccel = false;
                }
            }

            // Check for player bursting car tires
            if (VehicleControl.GetWheelRotationSpeeds(Vehicle)[0] < 1f)
            {
                // Check if car is on same place
                if (Speed < 1)
                {
                    _wheelPosPrev = _wheelPos;
                    _isPosChanged = false;
                }

                if (_wheelPosPrev != null)
                {
                    foreach (Vector3 pos in _wheelPos)
                    {
                        if (pos.Round(1) != _wheelPosPrev[
                            _wheelPos.IndexOf(pos)].Round(1))
                        {
                            _isPosChanged = true;
                            break;
                        }
                    }
                }

                if (_isPosChanged && _possibleFastAccel)
                {
                    _possibleFastAccel = false;
                }
            }
        }

        private void GetAccelerationSound()
        {
            WheelSpeed = VehicleControl.GetWheelRotationSpeeds(Vehicle)[3];

            // Define engine sound variation based on wheel speed
            _currentAccel = _engineAccel1Sound;

            if (CurrentGear == 2)
            {
                _currentAccel = _engineAccel2Sound;
            }

            if (CurrentGear == 3)
            {
                _currentAccel = _engineAccel3Sound;
            }

            if (CurrentGear >= 4)
            {
                _currentAccel = _engineAccel4Sound;
            }
        }

        private void Debug(int id)
        {
            switch (id)
            {
                case 0:
                    Screen.ShowSubtitle($"WSpeed: {Math.Round(WheelSpeed)} " +
                                        $"RPM: {Math.Round(EngineRpm, 2)} " +
                                        $"Breaking: {IsBraking} " +
                                        $"Idle: {IsIdle} " +
                                        $"Neutral: {IsNeutral} " +
                                        $"Accel: {IsAccelerating} " +
                                        $"Revers: {IsReversing} " +
                                        $"Revving: {IsRevving}");
                    break;
                case 1:
                    Screen.ShowSubtitle($"Accel: {_engineAccel1Sound.IsAnyInstancePlaying} {_engineAccel1Sound.InstancesNumber}  " +
                                        $"Decel: {_engineDecelSound.IsAnyInstancePlaying} {_engineDecelSound.InstancesNumber}  " +
                                        $"Neutral: {_engineNeutralSound.IsAnyInstancePlaying} {_engineNeutralSound.InstancesNumber}  " +
                                        $"Reverse: {_engineReverseSound.IsAnyInstancePlaying} {_engineReverseSound.InstancesNumber}  " +
                                        $"Idle: {_engineIdleSound.IsAnyInstancePlaying} {_engineIdleSound.InstancesNumber}  " +
                                        $"Rev: {_engineRevvingSound.IsAnyInstancePlaying} {_engineRevvingSound.InstancesNumber} ");
                    break;
                case 2:
                    Screen.ShowSubtitle($"WSpeed: {Math.Round(WheelSpeed, 2)} Accel: {Math.Round(Acceleration, 2)} IsAccel: {IsAccelerating} " +
                                        $"RPM: {Math.Round(EngineRpm, 2)} ");
                    break;
            }
        }

        #endregion

        #endregion
    }
}
