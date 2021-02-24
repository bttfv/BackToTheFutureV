using BackToTheFutureV.Utility;
using FusionLibrary;
using GTA.Math;
using System;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    [Serializable]
    public class BaseProperties
    {
        //Persistent properties
        public Guid GUID { get; set; }
        public bool AreTimeCircuitsOn { get; set; }
        public DateTime SpawnTime { get; set; } = Utils.CurrentTime;
        public DateTime DestinationTime { get; set; } = BTTFImportantDates.GetRandom();
        public DateTime PreviousTime { get; set; } = new DateTime(1985, 10, 26, 1, 20, 00);
        public Vector3 LastVelocity { get; set; }
        public TimeTravelType TimeTravelType { get; set; } = TimeTravelType.Cutscene;
        public bool AreTimeCircuitsBroken { get; set; }
        private int _reactorCharge;
        public bool CutsceneMode { get; set; } = true;
        public bool IsFreezed { get; set; }
        public bool IsDefrosting { get; set; }
        public float IceValue { get; set; }
        public bool IsFlying { get; set; }
        public bool AreWheelsInHoverMode { get; set; }
        public bool CanConvert { get; set; } = true;
        public bool AreFlyingCircuitsBroken { get; set; }
        public bool AreHoodboxCircuitsReady { get; set; }
        public bool WasOnTracks { get; set; }
        public bool HasBeenStruckByLightning { get; set; }
        public Vector3 TimeTravelDestPos { get; set; } = Vector3.Zero;
        public int TimeTravelsCount { get; set; }

        //Temporary properties        
        public int ReactorCharge
        {
            get => _reactorCharge;
            set
            {
                if (value >= 0)
                    _reactorCharge = value;
            }
        }
        public bool IsFueled => ReactorCharge > 0;
        public bool IsGivenScaleformPriority { get; set; }
        public TimeTravelPhase TimeTravelPhase { get; set; } = TimeTravelPhase.Completed;
        public bool IsRefueling { get; set; }
        public bool IsFluxDoingBlueAnim { get; set; }
        public bool IsEngineStalling { get; set; }
        public bool IsRemoteControlled { get; set; }
        public bool IsLanding { get; set; }
        public bool IsAltitudeHolding { get; set; }
        public bool IsHoverBoosting { get; set; }
        public bool IsPhotoModeOn { get; set; }
        public bool IsOnTracks { get; set; }
        public bool IsAttachedToRogersSierra { get; set; }
        public bool PhotoWormholeActive { get; set; }
        public bool PhotoGlowingCoilsActive { get; set; }
        public bool PhotoFluxCapacitorActive { get; set; }
        public bool PhotoEngineStallActive { get; set; }
        public bool PhotoSIDMaxActive { get; set; }
        public float TorqueMultiplier { get; set; } = 1;
        public MissionType MissionType { get; set; } = MissionType.None;
        public bool Story { get; set; }
        public bool BlockSparks { get; set; }

        public BaseProperties Clone()
        {
            BaseProperties ret = new BaseProperties
            {
                GUID = GUID,
                AreTimeCircuitsOn = AreTimeCircuitsOn,
                SpawnTime = SpawnTime,
                DestinationTime = DestinationTime,
                PreviousTime = PreviousTime,
                LastVelocity = LastVelocity,
                TimeTravelType = TimeTravelType,
                AreTimeCircuitsBroken = AreTimeCircuitsBroken,
                _reactorCharge = _reactorCharge,
                CutsceneMode = CutsceneMode,
                IsFreezed = IsFreezed,
                IsDefrosting = IsDefrosting,
                IceValue = IceValue,
                IsFlying = IsFlying,
                AreWheelsInHoverMode = AreWheelsInHoverMode,
                CanConvert = CanConvert,
                AreFlyingCircuitsBroken = AreFlyingCircuitsBroken,
                AreHoodboxCircuitsReady = AreHoodboxCircuitsReady,
                WasOnTracks = WasOnTracks,
                HasBeenStruckByLightning = HasBeenStruckByLightning,
                TimeTravelDestPos = TimeTravelDestPos,
                TimeTravelsCount = TimeTravelsCount
            };

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            timeMachine.Properties.GUID = GUID;
            timeMachine.Properties.AreTimeCircuitsOn = AreTimeCircuitsOn;
            timeMachine.Properties.SpawnTime = SpawnTime;
            timeMachine.Properties.DestinationTime = DestinationTime;
            timeMachine.Properties.PreviousTime = PreviousTime;
            timeMachine.Properties.LastVelocity = LastVelocity;
            timeMachine.Properties.TimeTravelType = TimeTravelType;
            timeMachine.Properties.AreTimeCircuitsBroken = AreTimeCircuitsBroken;
            timeMachine.Properties._reactorCharge = _reactorCharge;
            timeMachine.Properties.CutsceneMode = CutsceneMode;
            timeMachine.Properties.IsFreezed = IsFreezed;
            timeMachine.Properties.IsDefrosting = IsDefrosting;
            timeMachine.Properties.IceValue = IceValue;
            timeMachine.Properties.IsFlying = IsFlying;
            timeMachine.Properties.AreWheelsInHoverMode = AreWheelsInHoverMode;
            timeMachine.Properties.CanConvert = CanConvert;
            timeMachine.Properties.AreFlyingCircuitsBroken = AreFlyingCircuitsBroken;
            timeMachine.Properties.AreHoodboxCircuitsReady = AreHoodboxCircuitsReady;
            timeMachine.Properties.WasOnTracks = WasOnTracks;
            timeMachine.Properties.HasBeenStruckByLightning = HasBeenStruckByLightning;
            timeMachine.Properties.TimeTravelDestPos = TimeTravelDestPos;
            timeMachine.Properties.TimeTravelsCount = TimeTravelsCount;

            if (IsFlying)
                timeMachine.Events.SetFlyMode?.Invoke(true, true);

            if (IsFreezed)
                timeMachine.Events.SetFreeze?.Invoke(true, true);

            if (AreHoodboxCircuitsReady)
                timeMachine.Events.SetHoodboxWarmedUp?.Invoke();
        }

        public void ApplyToWayback(TimeMachine timeMachine)
        {
            timeMachine.Properties.GUID = GUID;
            timeMachine.Properties.ReactorCharge = ReactorCharge;
            timeMachine.Properties.PreviousTime = PreviousTime;

            if (AreTimeCircuitsOn != timeMachine.Properties.AreTimeCircuitsOn)
                timeMachine.Events.SetTimeCircuits?.Invoke(AreTimeCircuitsOn);

            if (DestinationTime != timeMachine.Properties.DestinationTime)
                timeMachine.Events.SimulateInputDate?.Invoke(DestinationTime);

            if (IsFlying != timeMachine.Properties.IsFlying)
                timeMachine.Events.SetFlyMode?.Invoke(IsFlying);
        }
    }
}
