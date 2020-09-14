using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV.OtherVehicles
{
    public static class GMCVanManager
    {
        private static List<GMCVanHandler> gmcVanHandlers = new List<GMCVanHandler>();

        public static void CreateGMCVan(Vector3 position, float heading)
        {
            gmcVanHandlers.Add(new GMCVanHandler(World.CreateVehicle(ModelHandler.GMCVan, position, heading)));

            Main.PlayerPed.Task.WarpIntoVehicle(gmcVanHandlers.Last().Vehicle, VehicleSeat.Driver);
        }

        public static void KeyDown(KeyEventArgs key)
        {
            gmcVanHandlers.ForEach(x => x.KeyDown(key));
        }

        public static void Process()
        {
            gmcVanHandlers.ForEach(x => x.Process());
        }

        public static void Abort()
        {
            gmcVanHandlers.ForEach(x => x.Delete());
        }
    }
}
