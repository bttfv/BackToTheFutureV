using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GTA;
using BackToTheFutureV.Utility;

namespace BackToTheFutureV
{
    public class TrafficInjector //: Script
    {
        /// <summary>
        /// It will check every 1 second
        /// </summary>
        public int InjectorInterval { get; set; } = 1000;

        private int nextCheck;

        private static Era currentEra;

        public static List<Vehicle> dmc12s = new List<Vehicle>();

        public TrafficInjector()
        {
            //Tick += Process;
            Era.LoadEraXmls("./scripts/BackToTheFutureV/eras/");
        }

        public static void UpdateEra()
        {
            currentEra = Era.GetEraForCurrentTime();

            ReplaceVehicles();
        }

        public static void AddDMC12()
        {
            var allVehicles = World.GetAllVehicles();

            foreach(var vehicle in allVehicles)
            {
                if (vehicle.Model == ModelHandler.DMC12)
                    continue;

                var dist = vehicle.Position.DistanceToSquared2D(Main.PlayerPed.Position);
                if (vehicle != null && dist > 100f * 100f && dist < 150f * 150f && !vehicle.IsPersistent && vehicle.Model != ModelHandler.DMC12 && vehicle.GetPedOnSeat(VehicleSeat.Driver) != Main.PlayerPed)
                {
                    var veh = Utils.ReplaceVehicle(vehicle, ModelHandler.DMC12);
                    veh.Mods.PrimaryColor = VehicleColor.BrushedAluminium;

                    if (!dmc12s.Contains(veh))
                        dmc12s.Add(veh);
                }
            }
        }

        public static void DeleteDMC12s()
        {
            foreach (var veh in dmc12s)
                veh?.DeleteCompletely();
        }

        private static void ReplaceVehicles()
        {
            if (currentEra == null) return;

            var allVehicles = World.GetAllVehicles();

            // Make sure not to replace the same vehicle twice
            var replacedHandles = new List<int>();

            foreach (var vehicle in allVehicles)
            {
                var vehicleInfo = currentEra.GetRandomVehicle();
                if(vehicleInfo == null)
                {
                    continue;
                }

                var model = new Model(vehicleInfo.Model);

                if (IsVehicleValid(vehicle) && !replacedHandles.Contains(vehicle.Handle))
                {
                    var randomNum = Utils.Random.NextDouble();

                    if (randomNum < 0.5)
                    {
                        replacedHandles.Add(vehicle.Handle);

                        Utils.ReplaceVehicle(vehicle, model);

                        //Wait(0);
                    }
                    else
                    {
                        vehicle.DeleteCompletely();
                    }
                }
            }
        }

        public void Process(object sender, EventArgs e)
        {
            if (Game.GameTime > nextCheck)
            {
                nextCheck = Game.GameTime + InjectorInterval;

                //ReplaceVehicles();
                AddDMC12();
            }
        }

        private static bool IsVehicleValid(Vehicle vehicle)
        {
            // Maybe add more checks here
            return vehicle != null && vehicle.Exists() && vehicle.Driver != Main.PlayerPed && vehicle.Model.IsCar && !vehicle.IsPersistent && !currentEra.SpawnableVehicles.Select(x => new Model(x.Model)).Contains(vehicle.Model);
        }
    }
}
