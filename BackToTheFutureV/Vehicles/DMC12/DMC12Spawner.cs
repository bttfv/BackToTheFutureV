using System;
using System.Collections.Generic;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV.Vehicles
{
    internal static class DMC12Spawner
    {
        private static DateTime StartTime = new DateTime(1981, 1, 21);
        private static DateTime EndTime = new DateTime(1982, 12, 24);

        private static DateTime LastTime = new DateTime(2050, 12, 31);

        private static int gameTimer;

        private static bool pause;

        private static List<DMC12> spawnedDMC12s = new List<DMC12>();

        static DMC12Spawner()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => 
            {
                spawnedDMC12s.ForEach(x => x?.Dispose());
                spawnedDMC12s.Clear();

                pause = true; 
            };
        }

        internal static void Process()
        {
            spawnedDMC12s.ForEach(x =>
            {
                if (x != null && x.Vehicle.IsFunctioning() && x.Vehicle.Position.DistanceToSquared2D(Utils.PlayerPed.Position) > 300 * 300)
                    x.Dispose();
            });

            if (pause || Game.GameTime < gameTimer)
                return;

            if (Utils.CurrentTime < StartTime || Utils.CurrentTime > LastTime)
            {
                pause = true;
                return;
            }

            DMC12 dmc12;

            spawnedDMC12s.Add(dmc12 = DMC12Handler.CreateDMC12(Utils.PlayerPed.Position.Around(100f), Utils.PlayerPed.Heading, false));
            dmc12.Vehicle.PlaceOnNextStreet();
            dmc12.Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver).Task.CruiseWithVehicle(dmc12, 20);

            float chance;

            if (Utils.CurrentTime <= EndTime)
                chance = 1;
            else
            {
                int year = LastTime.Year - Utils.CurrentTime.Year;

                chance = (year * 100) / (LastTime.Year - EndTime.Year);

                chance /= 100;

                chance -= 1;

                chance = Math.Abs(chance);
            }

            GTA.UI.Screen.ShowSubtitle($"{(int)(120000 * chance)}");

            gameTimer = Game.GameTime + (int)(120000 * chance);
        }
    }
}
