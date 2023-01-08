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
        public Guid GUID { get; private set; }
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
        public bool PlayerUsed { get; set; }
        public bool StruckByLightning { get; set; }
        public bool StruckByLightningInstant { get; set; }
        public bool IsWayback { get; set; }

        public HUDProperties HUDProperties { get; set; } = new HUDProperties();
        public bool ForceSIDMax { get; set; }
        public int[] CurrentHeight { get; set; } = new int[10];
        public int[] NewHeight { get; set; } = new int[10];
        public int[] LedDelay { get; set; } = new int[10];

        public PropertiesHandler()
        {
            GUID = Guid.NewGuid();
        }

        public void NewGUID()
        {
            GUID = Guid.NewGuid();
        }

        public PropertiesHandler Clone()
        {
            PropertiesHandler ret = new PropertiesHandler();

            ret.GUID = GUID;
            ret.AreTimeCircuitsOn = AreTimeCircuitsOn;
            ret.AlarmSet = AlarmSet;
            ret.AlarmTime = AlarmTime;
            ret.SyncWithCurTime = SyncWithCurTime;
            ret.ClockTime = ClockTime;
            ret.DestinationTime = DestinationTime;
            ret.PreviousTime = PreviousTime;
            ret.LastVelocity = LastVelocity;
            ret.TimeTravelType = TimeTravelType;
            ret.AreTimeCircuitsBroken = AreTimeCircuitsBroken;
            ret.ReactorCharge = ReactorCharge;
            ret.CutsceneMode = CutsceneMode;
            ret.IsFreezed = IsFreezed;
            ret.IsDefrosting = IsDefrosting;
            ret.IceValue = IceValue;
            ret.IsFlying = IsFlying;
            ret.IsHoverBoosting = IsHoverBoosting;
            ret.IsHoverGoingUpDown = IsHoverGoingUpDown;
            ret.CanConvert = CanConvert;
            ret.AreFlyingCircuitsBroken = AreFlyingCircuitsBroken;
            ret.AreHoodboxCircuitsReady = AreHoodboxCircuitsReady;
            ret.IsOnTracks = IsOnTracks;
            ret.WasOnTracks = WasOnTracks;
            ret.HasBeenStruckByLightning = HasBeenStruckByLightning;
            ret.TimeTravelDestPos = TimeTravelDestPos;
            ret.TimeTravelsCount = TimeTravelsCount;
            ret.ReactorState = ReactorState;
            ret.OverrideTimeTravelConstants = OverrideTimeTravelConstants;
            ret.OverrideSet = OverrideSet;
            ret.OverrideSIDSpeed = OverrideSIDSpeed;
            ret.OverrideTTSfxSpeed = OverrideTTSfxSpeed;
            ret.OverrideTTSpeed = OverrideTTSpeed;
            ret.OverrideWormholeLengthTime = OverrideWormholeLengthTime;

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            timeMachine.Properties.GUID = GUID;
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

            if (ReactorState != timeMachine.Properties.ReactorState)
            {
                timeMachine.Events.SetReactorState?.Invoke(ReactorState);
            }

            if (StruckByLightning && StruckByLightning != timeMachine.Properties.StruckByLightning)
            {
                timeMachine.Events.StartLightningStrike?.Invoke(0);
            }

            if (StruckByLightningInstant && StruckByLightningInstant != timeMachine.Properties.StruckByLightningInstant)
            {
                timeMachine.Events.StartLightningStrike?.Invoke(-1);
            }
        }
    }
}
