using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal static class WaybackHandler
    {
        private static List<Wayback> Waybacks = new List<Wayback>();

        public static void Tick()
        {
            if (Waybacks.Count == 0)
                Create(Utils.PlayerPed);

            Waybacks.ForEach(x => x.Tick());
        }

        public static Wayback Create(Ped ped)
        {
            Wayback wayback = new Wayback(ped);

            Waybacks.Add(wayback);

            return wayback;
        }

        public static Wayback Create(Ped ped, Guid guid)
        {
            Wayback wayback = new Wayback(ped, guid);

            Waybacks.Add(wayback);

            return wayback;
        }

        public static Wayback GetFromGUID(Guid guid)
        {
            return Waybacks.FirstOrDefault(x => x.GUID == guid);
        }
    }
}
