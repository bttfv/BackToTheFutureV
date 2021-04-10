using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WaybackSystem
    {
        private static List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording && !x.IsRemote);

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => Stop();
        }

        public static void Tick()
        {
            if (!ModSettings.WaybackSystem)
                return;

            if (CurrentRecording == default)
                Create(FusionUtils.PlayerPed, Guid.NewGuid());

            if (Game.WasCheatStringJustEntered("server"))
                WaybackServer.StartServer();

            if (Game.WasCheatStringJustEntered("client"))
                WaybackClient.StartClient();

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
            return Machines.SingleOrDefault(x => x.GUID == guid);
        }

        public static bool AddFromData(byte[] data)
        {
            WaybackMachine waybackMachine = WaybackMachine.FromData(data);

            if (waybackMachine == null)
                return false;

            if (Machines.SingleOrDefault(x => x.GUID == waybackMachine.GUID && x.IsRemote) != default)
                return true;

            waybackMachine.Reset(true);

            Machines.Add(waybackMachine);

            FusionUtils.HelpText = $"Added remote wayback. Total: ({Machines.Count})";

            return true;
        }

        public static bool RecordFromData(byte[] data)
        {
            WaybackPed waybackPed = WaybackPed.FromData(data);

            if (waybackPed == null)
                return false;

            WaybackMachine waybackMachine = Machines.SingleOrDefault(x => x.GUID == waybackPed.Owner && x.IsRemote);

            if (waybackMachine == default)
                return false;

            waybackMachine.Add(waybackPed);

            return true;
        }
    }
}
