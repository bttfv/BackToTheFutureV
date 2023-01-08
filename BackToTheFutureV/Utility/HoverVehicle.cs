using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal delegate void OnSwitchHoverMode(bool state);
    internal delegate void OnHoverBoost(bool state);
    internal delegate void OnVerticalBoost(bool state, bool up);
    internal delegate void OnHoverLanding();

    internal class HoverVehicle
    {
        private static List<HoverVehicle> GlobalHoverVehicles { get; } = new List<HoverVehicle>();
        private static List<HoverVehicle> _hoverVehiclesToRemove { get; } = new List<HoverVehicle>();

        public static HoverVehicle GetFromVehicle(Vehicle vehicle)
        {
            if (!vehicle.IsFunctioning())
                return null;

            HoverVehicle hoverVehicle = GlobalHoverVehicles.SingleOrDefault(x => x.Vehicle == vehicle);

            if (hoverVehicle != default)
                return hoverVehicle;

            return new HoverVehicle(vehicle);
        }

        public static void Clean()
        {
            GlobalHoverVehicles.ForEach(x => 
            {
                if (!x.Vehicle.IsFunctioning())
                    _hoverVehiclesToRemove.Add(x);
            });

            if (_hoverVehiclesToRemove.Count > 0)
            {
                _hoverVehiclesToRemove.ForEach(x => GlobalHoverVehicles.Remove(x));
                _hoverVehiclesToRemove.Clear();
            }
        }

        public static event OnSwitchHoverMode OnSwitchHoverMode;
        public static event OnHoverBoost OnHoverBoost;
        public static event OnVerticalBoost OnVerticalBoost;
        public static event OnHoverLanding OnHoverLanding;

        public Vehicle Vehicle { get; }
        private Decorator decorator { get; }
        private ParticlePlayerHandler HoverLandingSmoke;
        private CVehicleWheels cVehicleWheels { get; }

        private int _nextForce;

        public bool IsHoverModeAllowed
        {
            get
            {
                if (!Vehicle.IsFunctioning())
                    return false;

                if (!decorator.Exists(BTTFVDecors.AllowHoverMode))
                    IsHoverModeAllowed = false;

                return decorator.GetBool(BTTFVDecors.AllowHoverMode);
            }

            set
            {
                if (!Vehicle.IsFunctioning())
                    return;

                if (Vehicle.Model == ModelHandler.DMC12 || Vehicle.Model == ModelHandler.Deluxo)
                    value = true;

                if (Vehicle.IsBoat || Vehicle.IsBicycle || Vehicle.IsBike || Vehicle.IsBlimp || Vehicle.IsAircraft || Vehicle.IsHelicopter || Vehicle.IsMotorcycle)
                    value = false;

                decorator.SetBool(BTTFVDecors.AllowHoverMode, value);

                Function.Call(Hash.SET_SPECIAL_FLIGHT_MODE_ALLOWED, Vehicle, value);

                if (!value && IsInHoverMode)
                    SwitchMode();
            }
        }

        public bool IsInHoverMode
        {
            get
            {
                bool value = decorator.GetBool(BTTFVDecors.IsInHoverMode);
                bool realValue = VehicleControl.GetDeluxoTransformation(Vehicle) > 0f;

                if (value == realValue)
                    return value;

                SetMode(realValue);

                return realValue;
            }

            private set => decorator.SetBool(BTTFVDecors.IsInHoverMode, value);
        }

        public bool IsHoverBoosting
        {
            get => decorator.GetBool(BTTFVDecors.IsHoverBoosting);
            set => decorator.SetBool(BTTFVDecors.IsHoverBoosting, value);
        }

        public bool IsVerticalBoosting
        {
            get => decorator.GetBool(BTTFVDecors.IsVerticalBoosting);
            set => decorator.SetBool(BTTFVDecors.IsVerticalBoosting, value);
        }

        public bool IsHoverLanding
        {
            get => decorator.GetBool(BTTFVDecors.IsHoverLanding);
            set => decorator.SetBool(BTTFVDecors.IsHoverLanding, value);
        }

        public bool IsWaitForLanding
        {
            get => decorator.GetBool(BTTFVDecors.IsWaitForLanding);
            set => decorator.SetBool(BTTFVDecors.IsWaitForLanding, value);
        }

        public bool IsAltitudeHolding
        {
            get => decorator.GetBool(BTTFVDecors.IsAltitudeHolding);
            set => decorator.SetBool(BTTFVDecors.IsAltitudeHolding, value);
        }

        private HoverVehicle(Vehicle vehicle)
        {
            Vehicle = vehicle;
            decorator = Vehicle.Decorator();
            cVehicleWheels = new CVehicleWheels(Vehicle);

            GlobalHoverVehicles.Add(this);

            if (decorator.Exists(BTTFVDecors.AllowHoverMode))
                return;

            IsHoverModeAllowed = false;
            IsHoverBoosting = false;
            IsVerticalBoosting = false;
            IsHoverLanding = false;
            IsWaitForLanding = false;
            IsAltitudeHolding = false;
        }

        public void Tick()
        {
            if (!IsInHoverMode)
                return;

            if (Vehicle.GetMPHSpeed() >= 3f)
                VehicleControl.SetDeluxoFlyMode(Vehicle, 1f);

            if (ModSettings.TurbulenceEvent)
            {
                if (Game.GameTime > _nextForce)
                {
                    float _force = 0;

                    switch (World.Weather)
                    {
                        case Weather.Clearing:
                            _force = 0.1f;
                            break;
                        case Weather.Raining:
                            _force = 0.2f;
                            break;
                        case Weather.ThunderStorm:
                        case Weather.Blizzard:
                            _force = 1;
                            break;
                    }

                    _force *= Vehicle.HeightAboveGround / 20f;

                    if (_force > 1)
                        _force = 1;

                    Vehicle.ApplyForce(Vector3.RandomXYZ() * _force, Vector3.RandomXYZ() * _force);

                    _nextForce = Game.GameTime + 100;
                }
            }

            if (IsHoverLanding)
            {
                if (Vehicle.HeightAboveGround < 2 && !IsWaitForLanding)
                {
                    if (HoverLandingSmoke == null)
                    {
                        HoverLandingSmoke = new ParticlePlayerHandler();

                        foreach (CVehicleWheel wheel in new CVehicleWheels(Vehicle))
                        {
                            HoverLandingSmoke.Add("cut_trevor1", "cs_meth_pipe_smoke", ParticleType.NonLooped, Vehicle, wheel.Position, new Vector3(-90, 0, 0), 5f);
                        }
                    }

                    HoverLandingSmoke.Play();

                    IsWaitForLanding = true;
                }

                if (Vehicle.HeightAboveGround > 5 && IsWaitForLanding)
                    IsWaitForLanding = false;

                if (Vehicle.IsUpsideDown || Vehicle.HeightAboveGround <= 0.5f || Vehicle.HeightAboveGround >= 20 || Vehicle.GetMPHSpeed() > 30f)
                {                    
                    IsHoverLanding = false;
                    IsWaitForLanding = false;

                    SetMode(false);

                    return;
                }
            }

            Vector3 _forceToBeApplied = Vector3.Zero;

            if (IsAltitudeHolding)
                _forceToBeApplied.Z += -Vehicle.Velocity.Z;

            // If the Handbrake control is pressed
            // Using this so that controllers are also supported
            if (Vehicle == FusionUtils.PlayerVehicle && !IsHoverLanding && Game.IsControlPressed(ModControls.HoverBoost) && Game.IsControlPressed(Control.VehicleAccelerate) && Vehicle.IsEngineRunning)
            {
                if (Game.IsControlJustPressed(ModControls.HoverBoost))
                {
                    FusionUtils.SetPadShake(100, 200);
                }

                if (Vehicle.GetMPHSpeed() <= 120)
                {
                    // Apply force
                    _forceToBeApplied += Vehicle.ForwardVector * 12f * Game.LastFrameTime;

                    if (!IsHoverBoosting)
                    {
                        IsHoverBoosting = true;
                        OnHoverBoost?.Invoke(true);
                    }
                }
            }
            else if (IsHoverBoosting)
            {
                IsHoverBoosting = false;
                OnHoverBoost?.Invoke(false);
            }

            if (Vehicle == FusionUtils.PlayerVehicle && Game.IsControlPressed(Control.VehicleHandbrake) && !Game.IsControlPressed(Control.VehicleAccelerate) && !Game.IsControlPressed(Control.VehicleBrake) && Vehicle.GetMPHSpeed() > 1)
            {
                _forceToBeApplied += Vehicle.ForwardVector * (Vehicle.RunningDirection() == RunningDirection.Forward ? -0.4f : 0.4f);
            }

            // Get how much value is moved up/down
            int upNormal = 0;

            if (Vehicle == FusionUtils.PlayerVehicle && !IsAltitudeHolding && Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleUp))
            {
                if (Vehicle.DecreaseSpeedAndWait(Vehicle.RunningDirection() == RunningDirection.Forward ? 20 : 10))
                {
                    Game.DisableControlThisFrame(Control.VehicleSelectNextWeapon);
                    Game.DisableControlThisFrame(Control.VehicleFlyThrottleUp);

                    upNormal = 1;
                }
                else
                {
                    FusionUtils.SetPadShake(100, 80);
                }
            }
            else if (Vehicle == FusionUtils.PlayerVehicle && !IsAltitudeHolding && Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleDown))
            {
                if (Vehicle.DecreaseSpeedAndWait(Vehicle.RunningDirection() == RunningDirection.Forward ? 10 : 20))
                {
                    Game.DisableControlThisFrame(Control.VehicleSelectNextWeapon);
                    Game.DisableControlThisFrame(Control.VehicleFlyThrottleDown);

                    upNormal = -1;
                }
                else
                {
                    FusionUtils.SetPadShake(100, 80);
                }
            }

            if (upNormal != 0)
            {
                if (!IsVerticalBoosting)
                {
                    IsVerticalBoosting = true;
                    OnVerticalBoost?.Invoke(true, upNormal == 1);
                }

                _forceToBeApplied += Vehicle.UpVector * 12f * upNormal * Game.LastFrameTime;

                _forceToBeApplied.Y = -Vehicle.Velocity.Y;
                _forceToBeApplied.X = -Vehicle.Velocity.X;
            }
            else if (IsVerticalBoosting)
            {
                IsVerticalBoosting = false;
                OnVerticalBoost?.Invoke(false, false);
            }

            Vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);
        }

        public void SetMode(bool state)
        {
            if (!IsHoverModeAllowed)
                return;

            if (ModSettings.LandingSystem)
            {
                if (state)
                {
                    if (IsHoverLanding)
                    {
                        IsHoverLanding = false;
                        IsWaitForLanding = false;
                        OnSwitchHoverMode?.Invoke(true);

                        return;
                    }
                }
                else
                {
                    if (IsHoverLanding)
                    {
                        IsHoverLanding = false;
                        IsWaitForLanding = false;
                        OnSwitchHoverMode?.Invoke(true);

                        return;
                    }
                    else if (!Vehicle.IsUpsideDown && Vehicle.HeightAboveGround > 0.5f && Vehicle.HeightAboveGround < 20 && Vehicle.GetMPHSpeed() <= 30f)
                    {
                        IsHoverLanding = true;
                        IsWaitForLanding = false;
                        OnHoverLanding?.Invoke();

                        if (FusionUtils.PlayerVehicle == Vehicle)
                        {
                            TextHandler.Me.ShowHelp("VTOLTip", true, new ControlInfo(ModControls.HoverVTOL).Button);
                        }

                        return;
                    }
                }
            }

            VehicleControl.SetDeluxoTransformation(Vehicle, state ? 1f : 0f);

            if (state)
            {
                Function.Call(Hash.FORCE_USE_AUDIO_GAME_OBJECT, Vehicle, "DELUXO");
            }
            else
            {
                Function.Call(Hash.FORCE_USE_AUDIO_GAME_OBJECT, Vehicle, Vehicle.Model.ToString());
            }

            IsInHoverMode = state;
            OnSwitchHoverMode?.Invoke(state);
        }

        public void SwitchMode()
        {            
            SetMode(!IsInHoverMode);
        }

        public static implicit operator Vehicle(HoverVehicle hoverVehicle)
        {
            if (!hoverVehicle.IsFunctioning())
            {
                return null;
            }

            return hoverVehicle.Vehicle;
        }

        public static implicit operator Entity(HoverVehicle hoverVehicle)
        {
            if (!hoverVehicle.IsFunctioning())
            {
                return null;
            }

            return hoverVehicle.Vehicle;
        }

        public static implicit operator InputArgument(HoverVehicle hoverVehicle)
        {
            if (!hoverVehicle.IsFunctioning())
            {
                return null;
            }

            return hoverVehicle.Vehicle;
        }
    }
}
