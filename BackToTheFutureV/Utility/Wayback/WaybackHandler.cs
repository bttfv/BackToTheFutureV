using GTA;
using System.Collections.Generic;

namespace BackToTheFutureV
{
    internal static class WaybackHandler
    {
        private static List<Wayback> Waybacks = new List<Wayback>();

        public static void Tick()
        {
            Waybacks.ForEach(x => x.Tick());
        }

        public static Wayback Create(Ped ped)
        {
            Wayback wayback = new Wayback(ped);

            Waybacks.Add(wayback);

            return wayback;
        }
    }
}
