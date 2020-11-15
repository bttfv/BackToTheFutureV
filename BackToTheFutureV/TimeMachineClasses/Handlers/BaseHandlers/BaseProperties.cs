using BackToTheFutureV.Utility;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    [Serializable]
    public class BaseProperties
    {
        public bool IsGivenScaleformPriority { get; set; }
        public bool AreWheelsStock { get; set; }
        public bool AreTimeCircuitsOn { get; set; }
        public DateTime DestinationTime { get; set; } = BTTFImportantDates.GetRandom();
        public DateTime PreviousTime { get; set; } = new DateTime(1985, 10, 26, 1, 20, 00);
        public Vector3 LastVelocity { get; set; }
        public TimeTravelPhase TimeTravelPhase { get; set; } = TimeTravelPhase.Completed;
        public TimeTravelType TimeTravelType { get; set; } = TimeTravelType.Cutscene;
        public bool AreTimeCircuitsBroken { get; set; }
        public bool IsFueled { get; set; } = true;
        public bool IsRefueling { get; set; }
        public bool CutsceneMode { get; set; } = true;
        public bool IsFluxDoingBlueAnim { get; set; }
        public bool IsEngineStalling { get; set; }
        public bool IsFreezed { get; set; }
        public bool IsDefrosting { get; set; }
        public float IceValue { get; set; }
        public bool IsRemoteControlled { get; set; }
        public bool IsFlying { get; set; }
        public bool AreWheelsInHoverMode { get; set; }
        public bool IsLanding { get; set; }
        public bool CanConvert { get; set; } = true;
        public bool IsAltitudeHolding { get; set; }
        public bool AreFlyingCircuitsBroken { get; set; }
        public bool IsHoverBoosting { get; set; }
        public bool AreHoodboxCircuitsReady { get; set; }
        public bool IsPhotoModeOn { get; set; }
        public bool IsOnTracks { get; set; }
        public bool WasOnTracks { get; set; }
        public bool IsAttachedToRogersSierra { get; set; }
        public bool HasBeenStruckByLightning { get; set; }
        public bool PhotoWormholeActive { get; set; }
        public bool PhotoGlowingCoilsActive { get; set; }
        public bool PhotoFluxCapacitorActive { get; set; }
        public bool PhotoEngineStallActive { get; set; }
        public float TorqueMultiplier { get; set; } = 1;
        public Vector3 TimeTravelDestPos { get; set; } = Vector3.Zero;
        public MissionType MissionType { get; set; } = MissionType.None;
        public bool Story { get; set; }
        public int TimeTravelsCount { get; set; } = 0;
        public bool BlockSparks { get; set; } = false;

        public BaseProperties Clone()
        {
            BaseProperties ret = new BaseProperties();

            PropertyInfo[] properties = ret.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
                property.SetValue(ret, property.GetValue(this));

            ret.IsGivenScaleformPriority = false;

            ret.IsEngineStalling = false;

            ret.IsOnTracks = false;

            ret.IsAttachedToRogersSierra = false;

            ret.TimeTravelPhase = TimeTravelPhase.Completed;

            ret.IsLanding = false;

            ret.IsHoverBoosting = false;

            ret.IsAltitudeHolding = false;

            ret.IsRemoteControlled = false;

            ret.MissionType = MissionType.None;

            ret.Story = false;

            ret.PhotoEngineStallActive = false;

            ret.PhotoFluxCapacitorActive = false;

            ret.PhotoGlowingCoilsActive = false;

            ret.PhotoWormholeActive = false;

            ret.BlockSparks = false;

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
                property.SetValue(timeMachine.Properties, property.GetValue(this));

            if (IsFlying)
                timeMachine.Events.SetFlyMode?.Invoke(true, true);

            if (IsFreezed)
                timeMachine.Events.SetFreeze?.Invoke(true, true);

            if (AreHoodboxCircuitsReady)
                timeMachine.Events.OnHoodboxReady?.Invoke();
        }
    }
}
