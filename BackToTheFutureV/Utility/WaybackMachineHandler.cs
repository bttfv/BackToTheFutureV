using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV.Utility
{
    public static class WaybackMachineHandler
    {
        public static List<WaybackMachine> WaybackMachines { get; } = new List<WaybackMachine>();

        public static bool Enabled { get; set; } = false;

        public static void SetState(bool enabled)
        {
            if (!enabled)
                Abort();

            Enabled = enabled;
        }

        public static void Abort()
        {
            WaybackMachines.ForEach(x => x.Abort());
            WaybackMachines.Clear();
        }

        public static void Stop()
        {
            WaybackMachines.ForEach(x => x.Stop());
        }

        public static WaybackMachine Create(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = Script.InstantiateScript<WaybackMachine>();

            waybackMachine.Create(timeMachine);

            //GTA.UI.Screen.ShowSubtitle($"New wayback machine {waybackMachine.StartTime}");

            return waybackMachine;
        }

        public static WaybackMachine TryFind(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = WaybackMachines.FirstOrDefault(x => x.GUID == timeMachine.Properties.GUID && !x.IsRecording && !x.IsPlaying && (Utils.CurrentTime - x.StartTime).Duration() < TimeSpan.FromMinutes(1) && Utils.CurrentTime <= x.EndTime);

            if (waybackMachine == default)
                return Create(timeMachine);
            else
                waybackMachine.TimeMachine = timeMachine;

            //GTA.UI.Screen.ShowSubtitle($"Wayback machine found {waybackMachine.StartTime} {waybackMachine.EndTime}");
                       
            return waybackMachine;
        }
    }
}
