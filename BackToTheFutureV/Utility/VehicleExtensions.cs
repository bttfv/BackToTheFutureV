using System;
using System.Linq;
using BackToTheFutureV.Delorean;
using GTA;

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
    }
}
