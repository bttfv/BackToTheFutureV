using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WaybackSystem
    {
        private static List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording);

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => Stop();
        }

        public static void Tick()
        {
            if (!ModSettings.WaybackSystem)
                return;

            if (CurrentRecording == default)
                Create(Utils.PlayerPed, Guid.NewGuid());

            Machines.ForEach(x => x.Tick());
        }

        public static void Stop()
        {
            Machines.ForEach(x => x.Stop());
        }

        public static void Abort()
        {
            Stop();
            Machines.Clear();
        }

        public static WaybackMachine Create(Ped ped, Guid guid)
        {
            CurrentRecording?.Stop();

            WaybackMachine wayback = new WaybackMachine(ped, guid);

            Machines.Add(wayback);

            return wayback;
        }

        public static WaybackMachine GetFromGUID(Guid guid)
        {
            return Machines.FirstOrDefault(x => x.GUID == guid);
        }

        public static bool AddFromData(byte[] data)
        {
            WaybackMachine waybackMachine = WaybackMachine.FromData(data);

            if (waybackMachine == null)
                return false;

            if (!Machines.Contains(waybackMachine))
                Machines.Add(waybackMachine);

            return true;
        }

        public static bool RecordFromData(byte[] data)
        {
            WaybackPed waybackPed;

            using (MemoryStream stream = new MemoryStream(data))
            {
                try
                {
                    waybackPed = (WaybackPed)Utils.BinaryFormatter.Deserialize(stream);
                }
                catch
                {
                    return false;
                }
            }

            WaybackMachine waybackMachine = Machines.SingleOrDefault(x => x.GUID == waybackPed.Owner);

            if (waybackMachine == default)
                return false;

            waybackMachine.Replicas.Add(waybackPed);

            return true;
        }
    }
}
