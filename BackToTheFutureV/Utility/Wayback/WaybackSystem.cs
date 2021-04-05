using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal static class WaybackSystem
    {
        private static List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording);

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += OnTimeChanged;
        }

        private static void OnTimeChanged(DateTime dateTime)
        {
            Stop();
        }

        public static void Stop()
        {
            Machines.ForEach(x => x.Stop());
        }

        public static void Tick()
        {
            if (CurrentRecording == default)
                Create(Utils.PlayerPed);

            Machines.ForEach(x => x.Tick());
        }

        public static WaybackMachine Create(Ped ped)
        {
            WaybackMachine wayback = new WaybackMachine(ped);

            Machines.Add(wayback);

            return wayback;
        }

        public static WaybackMachine Create(Ped ped, Guid guid)
        {
            WaybackMachine wayback = new WaybackMachine(ped, guid);

            Machines.Add(wayback);

            return wayback;
        }

        public static WaybackMachine GetFromGUID(Guid guid)
        {
            return Machines.FirstOrDefault(x => x.GUID == guid);
        }
    }
}
