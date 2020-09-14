using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Vehicles
{
    public class DMC12Handler
    {
        private static List<DMC12> _deloreans  = new List<DMC12>();
        private static List<DMC12> _deloreansToAdd = new List<DMC12>();
        private static Dictionary<DMC12, bool> _deloreansToRemove = new Dictionary<DMC12, bool>();

        public static DMC12 CreateDMC12(Vector3 position, float heading = 0, bool warpInPlayer = false)
        {
            Vehicle vehicle = World.CreateVehicle(ModelHandler.DMC12, position, heading);

            if (warpInPlayer)
                Main.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);

            return new DMC12(vehicle);
        }

        public static void AddDelorean(DMC12 vehicle)
        {
            if (_deloreansToAdd.Contains(vehicle) || _deloreans.Contains(vehicle))
                return;

            _deloreansToAdd.Add(vehicle);
        }

        public static void RemoveDelorean(DMC12 vehicle, bool deleteVeh = true)
        {
            if (_deloreansToRemove.ContainsKey(vehicle))
                return;

            _deloreansToRemove.Add(vehicle, deleteVeh);
        }

        public static void RemoveInstantlyDelorean(DMC12 vehicle, bool deleteVeh = true)
        {
            vehicle?.Dispose(deleteVeh);

            _deloreans.Remove(vehicle);
        }

        public static void Process()
        {
            if (_deloreansToRemove.Count > 0)
            {
                foreach (var delo in _deloreansToRemove)
                    RemoveInstantlyDelorean(delo.Key, delo.Value);

                _deloreansToRemove.Clear();
            }

            if (_deloreansToAdd.Count > 0)
            {
                _deloreans.AddRange(_deloreansToAdd);
                _deloreansToAdd.Clear();
            }

            foreach (var delo in _deloreans)
            {
                if (delo.Disposed || !delo.Vehicle.Exists())
                {
                    RemoveDelorean(delo, true);
                    continue;
                }

                delo.Process();
            }                
        }

        public static DMC12 GetDeloreanFromVehicle(Vehicle vehicle)
        {
            foreach (var delo in _deloreans)
            {
                if (delo.Vehicle == vehicle)
                    return delo;
            }

            foreach (var delo in _deloreansToAdd)
            {
                if (delo.Vehicle == vehicle)
                    return delo;
            }

            return null;
        }
    }
}
