using System;
using System.Linq;
using BackToTheFutureV.Delorean;
using GTA;
using GTA.Native;

namespace BackToTheFutureV.Utility
{
    public static class VehicleExtensions
    {
        public static void DeleteCompletely(this Vehicle vehicle)
        {
            try
            {
                vehicle?.Driver?.Delete();
                vehicle?.Occupants?.ToList().ForEach(x => x?.Delete());
            }
            catch(Exception)
            {
            }

            vehicle?.Delete();
        }

        public static bool IsTimeMachine(this Vehicle vehicle)
        {
            return DeloreanHandler.IsVehicleADelorean(vehicle);
        }

        public static float GetMPHSpeed(this Vehicle vehicle)
        {
            return vehicle.Speed / 0.27777f / 1.60934f;
        }

        public static void SetMPHSpeed(this Vehicle vehicle, float value)
        {
            vehicle.ForwardSpeed = (value * 0.27777f * 1.60934f);
        }

        public static void SetLightsMode(this Vehicle vehicle, LightsMode lightsMode)
        {
            Function.Call(Hash._SET_VEHICLE_LIGHTS_MODE, vehicle, lightsMode);
            Function.Call(Hash.SET_VEHICLE_LIGHTS, vehicle, lightsMode);
        }

        public static void GetLightsState(this Vehicle vehicle, out bool lightsOn, out bool highbeamsOn)
        {
            bool _lightsOn;
            bool _highbeamsOn;

            unsafe
            {
                Function.Call(Hash.GET_VEHICLE_LIGHTS_STATE, vehicle, &_lightsOn, &_highbeamsOn);
            }

            lightsOn = _lightsOn;
            highbeamsOn = _highbeamsOn;
        }

        public static void SetLightsBrightness(this Vehicle vehicle, float brightness)
        {
            Function.Call(Hash.SET_VEHICLE_LIGHT_MULTIPLIER, vehicle, brightness);
        }
    }
}
