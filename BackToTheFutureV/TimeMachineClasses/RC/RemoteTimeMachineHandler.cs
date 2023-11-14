﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA.Chrono;
using LemonUI.TimerBars;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal class RemoteTimeMachineHandler
    {
        public static readonly float MAX_DIST = 650f;

        public static TimeMachine RemoteControlling { get; private set; }
        public static bool IsRemoteOn => RemoteControlling != null;

        private static TimerBarCollection TimerBarCollection { get; }
        private static readonly TimerBarProgress SignalBar;

        public static List<RemoteTimeMachine> RemoteTimeMachines { get; private set; } = new List<RemoteTimeMachine>();

        static RemoteTimeMachineHandler()
        {
            TimerBarCollection = new TimerBarCollection(SignalBar = new TimerBarProgress(TextHandler.Me.GetLocalizedText("SignalStrength")))
            {
                Visible = false
            };

            CustomNativeMenu.ObjectPool.Add(TimerBarCollection);
        }

        public static RemoteTimeMachine GetRemoteTimeMachineFromGUID(Guid guid)
        {
            RemoteTimeMachine timeMachine = RemoteTimeMachines.SingleOrDefault(x => x.TimeMachineClone.Properties.GUID == guid);

            if (timeMachine == default)
                return null;

            return timeMachine;
        }

        public static void StartRemoteControl(TimeMachine timeMachine)
        {
            if (timeMachine == null)
            {
                return;
            }

            if (IsRemoteOn)
            {
                RemoteControlling.Events.SetRCMode?.Invoke(false, true);
            }

            timeMachine.Events.SetRCMode?.Invoke(true);
            RemoteControlling = timeMachine;

            PlayerSwitch.Disable = true;
        }

        public static void StopRemoteControl(bool instant = false)
        {
            RemoteControlling.Events.SetRCMode?.Invoke(false, instant);
            RemoteControlling = null;

            PlayerSwitch.Disable = false;
            TimerBarCollection.Visible = false;
        }

        public static RemoteTimeMachine GetTimeMachineFromIndex(int index)
        {
            try
            {
                return RemoteTimeMachines[index];
            }
            catch
            {
                return null;
            }
        }

        public static RemoteTimeMachine AddRemote(TimeMachineClone timeMachineClone)
        {
            if (RemoteTimeMachines.Count > ModSettings.MaxRecordedMachines)
            {
                RemoteTimeMachines[0].Dispose();
                RemoteTimeMachines.RemoveAt(0);
            }

            RemoteTimeMachine timeMachine;

            RemoteTimeMachines.Add(timeMachine = new RemoteTimeMachine(timeMachineClone));

            return timeMachine;
        }

        public static void ExistenceCheck(GameClockDateTime time)
        {
            RemoteTimeMachines.ForEach(x => x.ExistenceCheck(time));
        }

        public static void Tick()
        {
            RemoteTimeMachines.ForEach(x => x.Tick());

            if (!IsRemoteOn)
            {
                return;
            }

            // If a savegame is loaded while RC is active it can cause the load process to malfunction, so force RC to exit first
            if (GTA.Game.IsControlJustPressed(GTA.Control.FrontendPause))
            {
                StopRemoteControl();
                return;
            }

            float squareDist = RemoteControlling.OriginalPed.DistanceToSquared2D(RemoteControlling.Vehicle);

            if (squareDist > MAX_DIST * MAX_DIST)
            {
                StopRemoteControl();
                return;
            }

            float percentage = ((MAX_DIST * MAX_DIST) - squareDist) / (MAX_DIST * MAX_DIST) * 100;

            if (!TimerBarCollection.Visible)
            {
                TimerBarCollection.Visible = true;
            }

            SignalBar.Progress = percentage;
        }

        public static void DeleteAll()
        {
            RemoteTimeMachines.ForEach(x => x.Dispose());
            RemoteTimeMachines.Clear();
        }

        public static void Abort()
        {
            foreach (RemoteTimeMachine x in RemoteTimeMachines)
            {
                x?.Dispose();
            }
        }
    }
}
