using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class DMC12Handler
    {
        private static readonly List<DMC12> _deloreans = new List<DMC12>();
        private static readonly List<DMC12> _deloreansToAdd = new List<DMC12>();
        private static readonly Dictionary<DMC12, bool> _deloreansToRemove = new Dictionary<DMC12, bool>();

        public static int Count => _deloreans.Count;

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
            {
                FusionUtils.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
            }

            return new DMC12(vehicle);
        }

        public static DMC12 CreateDMC12(Vehicle vehicle, bool warpInPlayer = false)
        {
            if (warpInPlayer)
            {
                FusionUtils.PlayerPed.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
            }

            return new DMC12(vehicle);
        }

        public static void AddDelorean(DMC12 vehicle)
        {
            if (_deloreansToAdd.Contains(vehicle) || _deloreans.Contains(vehicle))
            {
                return;
            }

            _deloreansToAdd.Add(vehicle);
        }

        public static void RemoveDelorean(DMC12 vehicle, bool deleteVeh = true)
        {
            if (_deloreansToRemove.ContainsKey(vehicle))
            {
                return;
            }

            _deloreansToRemove.Add(vehicle, deleteVeh);
        }

        public static void RemoveInstantlyDelorean(DMC12 vehicle, bool deleteVeh = true)
        {
            vehicle?.Dispose(deleteVeh);

            _deloreans.Remove(vehicle);
        }

        public static void Tick()
        {
            foreach (Vehicle veh in World.GetAllVehicles())
            {
                if (veh.Model.Hash == ModelHandler.DMC12.Model.Hash && veh.IsFunctioning() && GetDeloreanFromVehicle(veh) == null)
                {
                    CreateDMC12(veh);
                }
            }

            if (_deloreansToRemove.Count > 0)
            {
                foreach (KeyValuePair<DMC12, bool> delo in _deloreansToRemove)
                {
                    RemoveInstantlyDelorean(delo.Key, delo.Value);
                }

                _deloreansToRemove.Clear();
            }

            if (_deloreansToAdd.Count > 0)
            {
                _deloreans.AddRange(_deloreansToAdd);
                _deloreansToAdd.Clear();
            }

            foreach (DMC12 delo in _deloreans)
            {
                if (delo.Mods.WormholeType > WormholeType.DMC12 && !delo.Vehicle.IsTimeMachine())
                {
                    TimeMachineHandler.Create(delo, SpawnFlags.NoMods, delo.Mods.WormholeType);
                }

                delo.Tick();
            }
        }

        public static DMC12 GetDeloreanFromVehicle(Vehicle vehicle)
        {
            foreach (DMC12 delo in _deloreans)
            {
                if (delo.Vehicle == vehicle)
                {
                    return delo;
                }
            }

            foreach (DMC12 delo in _deloreansToAdd)
            {
                if (delo.Vehicle == vehicle)
                {
                    return delo;
                }
            }

            return null;
        }
    }
}
