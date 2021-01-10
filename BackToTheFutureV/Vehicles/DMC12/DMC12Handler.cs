using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Vehicles
{
    public class DMC12Handler
    {
        private static List<DMC12> _deloreans  = new List<DMC12>();
        private static List<DMC12> _deloreansToAdd = new List<DMC12>();
        private static Dictionary<DMC12, bool> _deloreansToRemove = new Dictionary<DMC12, bool>();

        public static void Abort()
        {
            _deloreans.ForEach(x =>
            {
                x.Dispose(false);
            });
        }

        public static DMC12 CreateDMC12(Vector3 position, float heading = 0, bool warpInPlayer = false)
        {
            Vehicle vehicle = World.CreateVehicle(ModelHandler.DMC12, position, heading);

            if (warpInPlayer)
                Utils.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);

            return new DMC12(vehicle);
        }

        public static DMC12 CreateDMC12(Vehicle vehicle, bool warpInPlayer = false)
        {            
            if (warpInPlayer)
                Utils.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);

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
            foreach (var veh in World.GetAllVehicles())
                if (veh.Model.Hash == ModelHandler.DMC12.Model.Hash && veh.IsFunctioning() && GetDeloreanFromVehicle(veh) == null)
                    CreateDMC12(veh);

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
                if (delo.Mods.WormholeType != WormholeType.DMC12 && !delo.Vehicle.IsTimeMachine())
                    TimeMachineHandler.Create(delo, SpawnFlags.Default, delo.Mods.WormholeType);

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
