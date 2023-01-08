using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using MinHook;
using System;
using System.Runtime.InteropServices;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec3V
    {
        [FieldOffset(0)] public float X;
        [FieldOffset(4)] public float Y;
        [FieldOffset(8)] public float Z;
        [FieldOffset(12)] public float W;

        public Vec3V(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;

            W = x;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CSpecialFlightHandlingData
    {
        [FieldOffset(16)] public Vec3V vecAngularDamping;
        [FieldOffset(32)] public Vec3V vecAngularDampingMin;
        [FieldOffset(48)] public Vec3V vecLinearDamping;
        [FieldOffset(64)] public Vec3V vecLinearDampingMin;
        [FieldOffset(80)] public float fLiftCoefficient;
        [FieldOffset(84)] public float fCriticalLiftAngle;
        [FieldOffset(88)] public float fInitialLiftAngle;
        [FieldOffset(92)] public float fMaxLiftAngle;
        [FieldOffset(96)] public float fDragCoefficient;
        [FieldOffset(100)] public float fBrakingDrag;
        [FieldOffset(104)] public float fMaxLiftVelocity;
        [FieldOffset(108)] public float fMinLiftVelocity;
        [FieldOffset(112)] public float fRollTorqueScale;
        [FieldOffset(116)] public float fMaxTorqueVelocity;
        [FieldOffset(120)] public float fMinTorqueVelocity;
        [FieldOffset(124)] public float fYawTorqueScale;
        [FieldOffset(128)] public float fSelfLevelingPitchTorqueScale;
        [FieldOffset(132)] public float fInitalOverheadAssist;
        [FieldOffset(136)] public float fMaxPitchTorque;
        [FieldOffset(140)] public float fMaxSteeringRollTorque;
        [FieldOffset(144)] public float fPitchTorqueScale;
        [FieldOffset(148)] public float fSteeringTorqueScale;
        [FieldOffset(152)] public float fMaxThrust;
        [FieldOffset(156)] public float fTransitionDuration;
        [FieldOffset(160)] public float fHoverVelocityScale;
        [FieldOffset(164)] public float fStabilityAssist;
        [FieldOffset(168)] public float fMinSpeedForThrustFalloff;
        [FieldOffset(172)] public float fBrakingThrustScale;
        [FieldOffset(176)] public int mode;
        [FieldOffset(180)] public long strFlags1;
        [FieldOffset(188)] public long strFlags2;
    }

    internal delegate void OnSwitchHoverMode(Vehicle vehicle, bool state);
    internal delegate void OnHoverBoost(Vehicle vehicle, bool state);
    internal delegate void OnVerticalBoost(Vehicle vehicle, bool state);
    internal delegate void OnHoverLanding(Vehicle vehicle);

    internal class HoverMode2 : Script
    {
        public static event OnSwitchHoverMode OnSwitchHoverMode;
        public static event OnHoverBoost OnHoverBoost;
        public static event OnVerticalBoost OnVerticalBoost;
        public static event OnHoverLanding OnHoverLanding;

        private readonly HookEngine _hook = new HookEngine();
        private CSpecialFlightHandlingData _customFlightData = new CSpecialFlightHandlingData();
        private GCHandle _customFlightData_Handle;
        private IntPtr pSubHandling;
        private bool _firstTick = true;
        private NativeInput _flyModeInput;
        private static int _nextModeChangeAllowed;

        IntPtr IntPtrCHandling_GetSubHandlingByType_Internal(IntPtr inst, int type)
        {
            IntPtr result = CHandling_GetSubHandlingByType_Original(inst, type);
            if (result == IntPtr.Zero)
            {
                // No default subhandling. Return custom one.
                if (type == 10) // rage::par::SUB_HANDLING_SPECIAL_FLIGHT
                    return _customFlightData_Handle.AddrOfPinnedObject();
            }
            return result;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate IntPtr CHandling_GetSubHandlingByType_Delegate(IntPtr inst, int type);
        CHandling_GetSubHandlingByType_Delegate CHandling_GetSubHandlingByType_Original;
        unsafe IntPtr CHandling_GetSubHandlingByType_Detour(IntPtr inst, int type)
        {
            // Use another method because otherwise .NET corrupts the stack
            return IntPtrCHandling_GetSubHandlingByType_Internal(inst, type);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        unsafe delegate void DeluxoSubHandling_UpdateAnimationBones1_Delegate(long pVehicle, uint a2, float a3, float* a4, float* a5, float a6);
        static unsafe void DeluxoSubHandling_UpdateAnimationBones1_Detour(long pVehicle, uint a2, float a3, float* a4, float* a5, float a6)
        {
            // Void
        }

        public HoverMode2()
        {
            Tick += HoverMode_Tick;
            Aborted += HoverMode_Aborted;
            KeyDown += HoverMode_KeyDown;
        }

        private void HoverMode_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Q && FusionUtils.PlayerVehicle.NotNullAndExists())
            {
                HoverVehicle hoverVehicle = new HoverVehicle(FusionUtils.PlayerVehicle);

                hoverVehicle.IsHoverModeAllowed = !hoverVehicle.IsHoverModeAllowed;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.L)
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (vehicle.IsBoat)
                        continue;

                    HoverVehicle hoverVehicle = new HoverVehicle(vehicle);

                    hoverVehicle.IsHoverModeAllowed = true;

                    SwitchMode(hoverVehicle.Vehicle);
                }
            }
        }

        private void CreateSubHandling()
        {
            // From handling.meta
            _customFlightData.mode = 1;
            _customFlightData.fLiftCoefficient = 50.0f;
            _customFlightData.fMinLiftVelocity = 0.0f;
            _customFlightData.fDragCoefficient = 0.0f;
            _customFlightData.fMaxPitchTorque = 500.0f;
            _customFlightData.fMaxSteeringRollTorque = 50.0f;
            _customFlightData.fMaxThrust = 20.0f;
            _customFlightData.fYawTorqueScale = -4.0f;
            _customFlightData.fRollTorqueScale = 7.5f;
            _customFlightData.fTransitionDuration = 1.0f;
            _customFlightData.fPitchTorqueScale = 8.0f;
            _customFlightData.vecAngularDamping = new Vec3V(3.0f, 2.0f, 1.2f);
            _customFlightData.vecLinearDamping = new Vec3V(0.9f, 0.1f, 0.7f);
            _customFlightData.vecAngularDampingMin = new Vec3V(0, 0, 0);
            _customFlightData.vecLinearDampingMin = new Vec3V(0, 0, 0);

            // From in game (defaults)
            _customFlightData.fCriticalLiftAngle = 45.0f;
            _customFlightData.fInitialLiftAngle = 1.5f;
            _customFlightData.fMaxLiftAngle = 25.0f;
            _customFlightData.fBrakingDrag = 10.0f;
            _customFlightData.fMaxLiftVelocity = 2000.0f;
            _customFlightData.fRollTorqueScale = 7.5f;
            _customFlightData.fMaxTorqueVelocity = 100.0f;
            _customFlightData.fMinTorqueVelocity = 40000.0f;
            _customFlightData.fYawTorqueScale = -4.0f;
            _customFlightData.fSelfLevelingPitchTorqueScale = -5.0f;
            _customFlightData.fInitalOverheadAssist = -5.0f;
            _customFlightData.fSteeringTorqueScale = 1000.0f;
            _customFlightData.fMaxThrust = 20.0f;
            _customFlightData.fHoverVelocityScale = 1.0f;
            _customFlightData.fStabilityAssist = 10.0f;
            _customFlightData.fMinSpeedForThrustFalloff = 0.0f;
            _customFlightData.fBrakingThrustScale = 0.0f;
        }

        private void HoverMode_Tick(object sender, EventArgs e)
        {
            if (!_firstTick)
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if (!vehicle.NotNullAndExists())
                        continue;

                    HoverVehicle hoverVehicle = new HoverVehicle(vehicle);

                    if (hoverVehicle.IsInHoverMode)
                    {                        
                        if (hoverVehicle.IsHoverLanding)
                            HandleLanding(hoverVehicle);
                        else
                            VehicleControl.SetDeluxoFlyMode(vehicle, 1f);
                    }                                            
                }

                if (FusionUtils.PlayerVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.HoverVehicle().IsInHoverMode)
                    HandlePlayerVehicle();

                if (FusionUtils.PlayerPed.LastVehicle.NotNullAndExists() && FusionUtils.PlayerPed.LastVehicle != FusionUtils.PlayerVehicle)
                    HandleLastVehicle();
            }

            if (Game.IsLoading || !_firstTick)
                return;

            Decorator.Register(BTTFVDecors.AllowHoverMode, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsHoverBoosting, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsHoverLanding, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsVerticalBoosting, DecorType.Bool);
            Decorator.Lock();

            CreateSubHandling();

            _customFlightData_Handle = GCHandle.Alloc(_customFlightData, GCHandleType.Pinned);
            pSubHandling = _customFlightData_Handle.AddrOfPinnedObject();

            unsafe
            {
                // DeluxoSubHandling::UpdateBoneAnimations1
                IntPtr addr = Game.FindPattern("E8 ? ? ? ? EB 30 49 63 0F");
                addr += 1;
                addr = addr + 4 + *(int*)addr;

                _hook.CreateHook(addr, new DeluxoSubHandling_UpdateAnimationBones1_Delegate(DeluxoSubHandling_UpdateAnimationBones1_Detour));

                // CHandling::GetSubHandlingByType
                addr = Game.FindPattern("E8 ?? ?? ?? ?? 66 3B 70 48");
                addr += 1;
                addr = addr + 4 + *(int*)addr;

                CHandling_GetSubHandlingByType_Original = _hook.CreateHook(addr, new CHandling_GetSubHandlingByType_Delegate(CHandling_GetSubHandlingByType_Detour));
            }

            _hook.EnableHooks();
            _firstTick = false;

            _flyModeInput = new NativeInput(ModControls.Hover);
            _flyModeInput.OnControlLongPressed += OnFlyModeControlJustLongPressed;
            _flyModeInput.OnControlPressed += OnFlyModeControlJustPressed;
        }

        private void HoverMode_Aborted(object sender, EventArgs e)
        {
            _hook.DisableHooks();
            _customFlightData_Handle.Free();
        }

        private void OnFlyModeControlJustLongPressed()
        {
            if (ModControls.LongPressForHover)
                SwitchMode(FusionUtils.PlayerVehicle);
        }

        private void OnFlyModeControlJustPressed()
        {
            if (!ModControls.LongPressForHover)
                SwitchMode(FusionUtils.PlayerVehicle);
        }

        public static void SwitchMode(Vehicle vehicle)
        {
            if (_nextModeChangeAllowed > Game.GameTime || !vehicle.NotNullAndExists())
                return;

            HoverVehicle hoverVehicle = new HoverVehicle(vehicle);

            if (!hoverVehicle.IsHoverModeAllowed)
                return;

            bool oldValue = hoverVehicle.IsInHoverMode;

            if (oldValue && ModSettings.LandingSystem)
            {
                if (hoverVehicle.IsHoverLanding)
                {
                    hoverVehicle.IsHoverLanding = false;
                    hoverVehicle.IsWaitForLanding = false;
                    OnSwitchHoverMode?.Invoke(vehicle, true);

                    if (FusionUtils.PlayerVehicle == vehicle)
                        _nextModeChangeAllowed = Game.GameTime + 2000;

                    return;
                }

                hoverVehicle.IsHoverLanding = true;
                OnHoverLanding?.Invoke(vehicle);

                if (FusionUtils.PlayerVehicle == vehicle)
                {
                    TextHandler.Me.ShowHelp("VTOLTip", true, new ControlInfo(ModControls.HoverVTOL).Button);
                    _nextModeChangeAllowed = Game.GameTime + 2000;
                }

                return;
            }

            hoverVehicle.IsInHoverMode = !oldValue;

            bool newValue = hoverVehicle.IsInHoverMode;

            if (oldValue != newValue)
                OnSwitchHoverMode?.Invoke(vehicle, newValue);

            if (FusionUtils.PlayerVehicle == vehicle)
                _nextModeChangeAllowed = Game.GameTime + 2000;
        }

        private void HandlePlayerVehicle()
        {
            Vector3 _forceToBeApplied = Vector3.Zero;
            Vehicle vehicle = FusionUtils.PlayerVehicle;
            HoverVehicle hoverVehicle = new HoverVehicle(vehicle);

            // If the Handbrake control is pressed
            // Using this so that controllers are also supported
            if (!hoverVehicle.IsHoverLanding && Game.IsControlPressed(ModControls.HoverBoost) && Game.IsControlPressed(Control.VehicleAccelerate) && vehicle.IsEngineRunning)
            {
                if (Game.IsControlJustPressed(ModControls.HoverBoost))
                {
                    FusionUtils.SetPadShake(100, 200);
                }

                if (vehicle.GetMPHSpeed() <= 120)
                {
                    // Apply force
                    _forceToBeApplied += vehicle.ForwardVector * 12f * Game.LastFrameTime;

                    if (!hoverVehicle.IsHoverBoosting)
                    {
                        hoverVehicle.IsHoverBoosting = true;
                        OnHoverBoost?.Invoke(vehicle, true);
                    }
                }
            }
            else if (hoverVehicle.IsHoverBoosting)
            {
                hoverVehicle.IsHoverBoosting = false;
                OnHoverBoost?.Invoke(vehicle, false);
            }

            if (Game.IsControlPressed(Control.VehicleHandbrake) && !Game.IsControlPressed(Control.VehicleAccelerate) && !Game.IsControlPressed(Control.VehicleBrake) && vehicle.GetMPHSpeed() > 1)
            {
                _forceToBeApplied += vehicle.ForwardVector * (vehicle.RunningDirection() == RunningDirection.Forward ? -0.4f : 0.4f);
            }

            // Get how much value is moved up/down
            int upNormal = 0;

            if (Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleUp))
            {
                if (vehicle.DecreaseSpeedAndWait(vehicle.RunningDirection() == RunningDirection.Forward ? 20 : 10))
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
            else if (Game.IsControlPressed(ModControls.HoverVTOL) && Game.IsControlPressed(Control.VehicleFlyThrottleDown))
            {
                if (vehicle.DecreaseSpeedAndWait(vehicle.RunningDirection() == RunningDirection.Forward ? 10 : 20))
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
                if (!hoverVehicle.IsVerticalBoosting)
                {
                    hoverVehicle.IsVerticalBoosting = true;
                    OnVerticalBoost?.Invoke(vehicle, true);
                }

                _forceToBeApplied += vehicle.UpVector * 12f * upNormal * Game.LastFrameTime;

                _forceToBeApplied.Y = -vehicle.Velocity.Y;
                _forceToBeApplied.X = -vehicle.Velocity.X;
            }
            else if (hoverVehicle.IsVerticalBoosting)
            {
                hoverVehicle.IsVerticalBoosting = false;
                OnVerticalBoost?.Invoke(vehicle, false);
            }

            vehicle.ApplyForce(_forceToBeApplied, Vector3.Zero);
        }

        private void HandleLastVehicle()
        {
            Vehicle vehicle = FusionUtils.PlayerPed.LastVehicle;
            HoverVehicle hoverVehicle = new HoverVehicle(vehicle);

            if (hoverVehicle.IsHoverBoosting)
            {
                hoverVehicle.IsHoverBoosting = false;
                OnHoverBoost?.Invoke(vehicle, false);
            }

            if (hoverVehicle.IsVerticalBoosting)
            {
                hoverVehicle.IsVerticalBoosting = false;
                OnVerticalBoost?.Invoke(vehicle, false);
            }
        }

        private void HandleLanding(HoverVehicle hoverVehicle)
        {
            if (hoverVehicle.Vehicle.HeightAboveGround < 2 && !hoverVehicle.IsWaitForLanding)
            {
                hoverVehicle.StartLandingSmoke();
                hoverVehicle.IsWaitForLanding = true;
            }

            if (hoverVehicle.Vehicle.HeightAboveGround >= 2 && hoverVehicle.IsWaitForLanding)
                hoverVehicle.IsWaitForLanding = false;

            if (!hoverVehicle.Vehicle.IsUpsideDown && hoverVehicle.Vehicle.HeightAboveGround > 0.5f && hoverVehicle.Vehicle.HeightAboveGround < 20 && hoverVehicle.Vehicle.GetMPHSpeed() <= 30f)
                return;

            hoverVehicle.IsHoverLanding = false;
            hoverVehicle.IsWaitForLanding = false;
            hoverVehicle.IsInHoverMode = false;
            OnSwitchHoverMode?.Invoke(hoverVehicle.Vehicle, false);
        }
    }
}
