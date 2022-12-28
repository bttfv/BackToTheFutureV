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
        private static readonly List<WaybackMachine> Machines = new List<WaybackMachine>();

        public static WaybackMachine CurrentPlayerRecording => Machines.SingleOrDefault(x => x.Status == WaybackStatus.Recording && x.IsPlayer);

        public static List<WaybackMachine> CurrentReplaying => Machines.Where(x => x.Status == WaybackStatus.Playing).ToList();

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (DateTime dateTime) => Stop();
        }

        public static void Tick()
        {
            if (FusionUtils.FirstTick || !ModSettings.WaybackSystem || TimeParadox.ParadoxInProgress || IntroHandler.Me.IsPlaying)
                return;

            if (CurrentPlayerRecording == default && FusionUtils.PlayerPed.IsAlive && !FusionUtils.PlayerPed.IsDead)
            {
                if (!TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() || (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase <= TimeTravelPhase.OpeningWormhole))
                {
                    Create(FusionUtils.PlayerPed);
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

        public static void Create(Ped ped)
        {
            Machines.Add(new WaybackMachine(ped));
        }
    }
}
