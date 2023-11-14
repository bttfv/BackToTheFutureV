﻿using FusionLibrary;
using GTA;
using GTA.Chrono;
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

        public static WaybackMachine GetWaybackMachineFromGUID(Guid guid)
        {
            WaybackMachine waybackMachine = Machines.SingleOrDefault(x => x.WaitForTimeMachineGUID == guid);

            if (waybackMachine == default)
                return null;

            return waybackMachine;
        }

        static WaybackSystem()
        {
            TimeHandler.OnTimeChanged += (GameClockDateTime dateTime) => Stop();
        }

        public static void Tick()
        {
            if (FusionUtils.FirstTick || !ModSettings.WaybackSystem || TimeParadox.ParadoxInProgress || IntroHandler.Me.IsPlaying)
                return;

            if (CurrentPlayerRecording == default && FusionUtils.PlayerPed.IsAlive && !FusionUtils.PlayerPed.IsDead && !Game.IsMissionActive)
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
