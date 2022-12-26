using FusionLibrary;
using GTA;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal static class WaybackSystem
    {
        private static readonly List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentPlayerRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording && x.IsPlayer);

        public static List<WaybackMachine> CurrentReplaying => Machines.Where(x => x.Status == WaybackStatus.Playing).ToList();

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => Stop();
        }

        public static void Tick()
        {
            if (!ModSettings.WaybackSystem || TimeParadox.ParadoxInProgress)
                return;

            if (CurrentPlayerRecording == default && FusionUtils.PlayerPed.IsAlive && !FusionUtils.PlayerPed.IsDead)
            {
                if (!TimeMachineHandler.CurrentTimeMachine.IsFunctioning() || (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase != TimeTravelPhase.InTime && TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase != TimeTravelPhase.Reentering))
                {
                    Create(FusionUtils.PlayerPed, Guid.NewGuid());
                }
            }

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

        public static void Create(Ped ped, Guid guid)
        {
            WaybackMachine wayback = new WaybackMachine(ped, guid);

            Machines.Add(wayback);
        }

        public static WaybackMachine GetFromGUID(Guid guid)
        {
            return Machines.SingleOrDefault(x => x.GUID == guid);
        }
    }
}
