using FusionLibrary;
using GTA.Math;
using System;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class PropertiesHandler
    {
        //Persistent properties
        public Guid GUID { get; set; }
        public Guid ReplicaGUID { get; set; }
        public bool AreTimeCircuitsOn { get; set; }
        public DateTime AlarmTime { get; set; }
        public bool AlarmSet { get; set; } = false;
        public bool SyncWithCurTime { get; set; } = true;
        public DateTime ClockTime { get; set; } = FusionUtils.CurrentTime;
        public DateTime DestinationTime { get; set; } = BTTFImportantDates.GetRandom();
        public DateTime PreviousTime { get; set; } = new DateTime(1985, 10, 26, 1, 20, 00);
        public Vector3 LastVelocity { get; set; }
        public TimeTravelType TimeTravelType { get; set; } = TimeTravelType.Cutscene;
        public bool AreTimeCircuitsBroken { get; set; }
        private int reactorCharge = 1;
        public bool CutsceneMode { get; set; } = true;
        public bool IsFreezed { get; set; }
        public bool IsDefrosting { get; set; }
        public float IceValue { get; set; }
        public bool IsFlying { get; set; }
        public bool CanConvert { get; set; } = true;
        public bool AreFlyingCircuitsBroken { get; set; }
        public bool AreHoodboxCircuitsReady { get; set; }
        public bool WasOnTracks { get; set; }
        public bool HasBeenStruckByLightning { get; set; }
        public Vector3 TimeTravelDestPos { get; set; } = Vector3.Zero;
        public int TimeTravelsCount { get; set; }
        public bool ThreeDigitsSpeedo { get; set; }
        public ReactorState ReactorState { get; set; } = ReactorState.Closed;
        public bool OverrideTimeTravelConstants { get; set; }
        public bool OverrideSet { get; set; }
        public int OverrideSIDSpeed { get; set; }
        public int OverrideTTSfxSpeed { get; set; }
        public int OverrideTTSpeed { get; set; }
        public int OverrideWormholeLengthTime { get; set; }


        //Temporary properties
        public bool HasScaleformPriority { get; set; }
        public int ReactorCharge
        {
            get => reactorCharge;

            set
            {
                if (value >= 0)
                {
                    reactorCharge = value;
                }
            }
        }
        public bool IsFueled => ReactorCharge > 0;

        public TimeTravelPhase TimeTravelPhase { get; set; } = TimeTravelPhase.Completed;
        public bool IsFluxDoingBlueAnim { get; set; }
        public bool IsFluxDoingOrangeAnim { get; set; }
        public bool IsEngineStalling { get; set; }
        public bool BlockEngineRecover { get; set; }
        public bool IsRemoteControlled { get; set; }
        public bool IsLanding { get; set; }
        public bool IsAltitudeHolding { get; set; }
        public bool IsHoverBoosting { get; set; }
        public bool IsHoverGoingUpDown { get; set; }
        public bool IsPhotoModeOn { get; set; }
        public bool IsOnTracks { get; set; }
        public bool PhotoWormholeActive { get; set; }
        public bool PhotoGlowingCoilsActive { get; set; }
        public bool PhotoFluxCapacitorActive { get; set; }
        public bool PhotoEngineStallActive { get; set; }
        public bool PhotoSIDMaxActive { get; set; }
        public MissionType MissionType { get; set; } = MissionType.None;
        public bool Story { get; set; }
        public bool BlockSparks { get; set; }
        public float Boost { get; set; }
        public bool IsWayback { get; set; }

        public HUDProperties HUDProperties { get; set; } = new HUDProperties();
        public bool ForceSIDMax { get; set; }
        public int[] CurrentHeight { get; set; } = new int[10];
        public int[] NewHeight { get; set; } = new int[10];
        public int[] LedDelay { get; set; } = new int[10];

        public PropertiesHandler(Guid guid)
        {
            GUID = guid;
            ReplicaGUID = Guid.NewGuid();
        }

        public PropertiesHandler Clone()
        {
            PropertiesHandler ret = new PropertiesHandler(GUID)
            {
                ReplicaGUID = ReplicaGUID,
                AreTimeCircuitsOn = AreTimeCircuitsOn,
                AlarmSet = AlarmSet,
                AlarmTime = AlarmTime,
                SyncWithCurTime = SyncWithCurTime,
                ClockTime = ClockTime,
                DestinationTime = DestinationTime,
                PreviousTime = PreviousTime,
                LastVelocity = LastVelocity,
                TimeTravelType = TimeTravelType,
                AreTimeCircuitsBroken = AreTimeCircuitsBroken,
                reactorCharge = reactorCharge,
                CutsceneMode = CutsceneMode,
                IsFreezed = IsFreezed,
                IsDefrosting = IsDefrosting,
                IceValue = IceValue,
                IsFlying = IsFlying,
                IsHoverBoosting = IsHoverBoosting,
                IsHoverGoingUpDown = IsHoverGoingUpDown,
                CanConvert = CanConvert,
                AreFlyingCircuitsBroken = AreFlyingCircuitsBroken,
                AreHoodboxCircuitsReady = AreHoodboxCircuitsReady,
                IsOnTracks = IsOnTracks,
                WasOnTracks = WasOnTracks,
                HasBeenStruckByLightning = HasBeenStruckByLightning,
                TimeTravelDestPos = TimeTravelDestPos,
                TimeTravelsCount = TimeTravelsCount,
                ThreeDigitsSpeedo = ThreeDigitsSpeedo,
                ReactorState = ReactorState,
                OverrideTimeTravelConstants = OverrideTimeTravelConstants,
                OverrideSet = OverrideSet,
                OverrideSIDSpeed = OverrideSIDSpeed,
                OverrideTTSfxSpeed = OverrideTTSfxSpeed,
                OverrideTTSpeed = OverrideTTSpeed,
                OverrideWormholeLengthTime = OverrideWormholeLengthTime
            };

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            timeMachine.Properties.GUID = GUID;
            timeMachine.Properties.ReplicaGUID = ReplicaGUID;
            timeMachine.Properties.AreTimeCircuitsOn = AreTimeCircuitsOn;
            timeMachine.Properties.AlarmTime = AlarmTime;
            timeMachine.Properties.AlarmSet = AlarmSet;
            timeMachine.Properties.SyncWithCurTime = SyncWithCurTime;
            timeMachine.Properties.ClockTime = ClockTime;
            timeMachine.Properties.DestinationTime = DestinationTime;
            timeMachine.Properties.PreviousTime = PreviousTime;
            timeMachine.Properties.LastVelocity = LastVelocity;
            timeMachine.Properties.TimeTravelType = TimeTravelType;
            timeMachine.Properties.AreTimeCircuitsBroken = AreTimeCircuitsBroken;
            timeMachine.Properties.reactorCharge = reactorCharge;
            timeMachine.Properties.CutsceneMode = CutsceneMode;
            timeMachine.Properties.IsFreezed = IsFreezed;
            timeMachine.Properties.IsDefrosting = IsDefrosting;
            timeMachine.Properties.IceValue = IceValue;
            timeMachine.Properties.IsFlying = IsFlying;
            timeMachine.Properties.CanConvert = CanConvert;
            timeMachine.Properties.AreFlyingCircuitsBroken = AreFlyingCircuitsBroken;
            timeMachine.Properties.AreHoodboxCircuitsReady = AreHoodboxCircuitsReady;
            timeMachine.Properties.WasOnTracks = WasOnTracks;
            timeMachine.Properties.HasBeenStruckByLightning = HasBeenStruckByLightning;
            timeMachine.Properties.TimeTravelDestPos = TimeTravelDestPos;
            timeMachine.Properties.TimeTravelsCount = TimeTravelsCount;
            timeMachine.Properties.ThreeDigitsSpeedo = ThreeDigitsSpeedo;
            timeMachine.Properties.OverrideTimeTravelConstants = OverrideTimeTravelConstants;
            timeMachine.Properties.OverrideSet = OverrideSet;
            timeMachine.Properties.OverrideSIDSpeed = OverrideSIDSpeed;
            timeMachine.Properties.OverrideTTSfxSpeed = OverrideTTSfxSpeed;
            timeMachine.Properties.OverrideTTSpeed = OverrideTTSpeed;
            timeMachine.Properties.OverrideWormholeLengthTime = OverrideWormholeLengthTime;

            if (IsFlying)
            {
                timeMachine.Events.SetFlyMode?.Invoke(true, true);
            }

            if (IsFreezed)
            {
                timeMachine.Events.SetFreeze?.Invoke(true, true);
            }

            if (AreHoodboxCircuitsReady)
            {
                timeMachine.Events.SetHoodboxWarmedUp?.Invoke();
            }

            timeMachine.Events.SetReactorState?.Invoke(ReactorState);
        }

        public void ApplyToWayback(TimeMachine timeMachine)
        {
            timeMachine.Properties.IsWayback = true;

            if (ReactorCharge != timeMachine.Properties.ReactorCharge)
            {
                timeMachine.Properties.ReactorCharge = ReactorCharge;
            }

            if (ThreeDigitsSpeedo != timeMachine.Properties.ThreeDigitsSpeedo)
            {
                timeMachine.Properties.ThreeDigitsSpeedo = ThreeDigitsSpeedo;
            }

            if (PreviousTime != timeMachine.Properties.PreviousTime)
            {
                timeMachine.Properties.PreviousTime = PreviousTime;
            }

            if (AreHoodboxCircuitsReady != timeMachine.Properties.AreHoodboxCircuitsReady)
            {
                timeMachine.Properties.AreHoodboxCircuitsReady = AreHoodboxCircuitsReady;
            }

            if (AreTimeCircuitsOn != timeMachine.Properties.AreTimeCircuitsOn)
            {
                timeMachine.Events.SetTimeCircuits?.Invoke(AreTimeCircuitsOn);
            }

            if (DestinationTime != timeMachine.Properties.DestinationTime)
            {
                timeMachine.Events.SimulateInputDate?.Invoke(DestinationTime);
            }

            if (IsFlying != timeMachine.Properties.IsFlying)
            {
                timeMachine.Events.SetFlyMode?.Invoke(IsFlying);
            }

            if (IsHoverBoosting != timeMachine.Properties.IsHoverBoosting)
            {
                timeMachine.Events.SimulateHoverBoost?.Invoke(IsHoverBoosting);
            }

            if (IsHoverGoingUpDown != timeMachine.Properties.IsHoverGoingUpDown)
            {
                timeMachine.Events.SimulateHoverGoingUpDown?.Invoke(IsHoverGoingUpDown);
            }
        }
    }
}
