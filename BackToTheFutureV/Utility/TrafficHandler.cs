using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal class TrafficHandler : Script
    {
        public static bool Enabled { get; private set; }

        public TrafficHandler()
        {
            Interval = 10000;
            Tick += TrafficHandler_Tick;
        }

        private void TrafficHandler_Tick(object sender, EventArgs e)
        {
            Enabled = FusionUtils.CurrentTime >= new DateTime(1981, 1, 21, 0, 0, 0) && DMC12Handler.Count < 25;

            if (Main.FirstTick || !Enabled)
                return;

            List<Vehicle> vehicles = FusionUtils.AllVehicles.Where(x => FusionUtils.PlayerVehicle != x && x.IsAlive && x.IsAutomobile && !x.Decorator().DrivenByPlayer && !x.IsTimeMachine()).ToList();

            if (vehicles.Count == 0)
                return;

            int dmcCount = vehicles.Count(x => x.Model == ModelHandler.DMC12);

            if (dmcCount >= 3)
                return;

            List<Vehicle> newVehicles = vehicles.Where(x => x.Model != ModelHandler.DMC12).SelectRandomElements(3 - dmcCount);

            foreach (Vehicle vehicle in newVehicles)
            {
                float dist = vehicle.DistanceToSquared2D(FusionUtils.PlayerPed);

                if (dist < 50 * 50)
                    break;

                VehicleReplica vehicleReplica = new VehicleReplica(vehicle);

                vehicle.DeleteCompletely();

                vehicleReplica.Model = ModelHandler.DMC12;

                Vehicle newVehicle = vehicleReplica.Spawn(FusionEnums.SpawnFlags.Default | FusionEnums.SpawnFlags.NoWheels);

                while (!newVehicle.NotNullAndExists())
                    Yield();

                newVehicle.PlaceOnGround();

                //newVehicle.AddBlip();

                if (newVehicle.Driver.NotNullAndExists())
                    newVehicle.Driver.Task.CruiseWithVehicle(newVehicle, 30);

                foreach (Ped ped in newVehicle.Occupants)
                    ped?.MarkAsNoLongerNeeded();

                newVehicle.MarkAsNoLongerNeeded();
            }
        }
    }
}
