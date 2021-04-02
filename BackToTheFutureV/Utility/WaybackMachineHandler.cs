using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal static class WaybackMachineHandler
    {
        public static List<WaybackMachine> WaybackMachines { get; } = new List<WaybackMachine>();

        public static bool Enabled { get; set; } = false;

        public static void Abort()
        {
            WaybackMachines.ForEach(x => x.Stop());
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

            return waybackMachine;
        }

        public static WaybackMachine TryFind(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = WaybackMachines.FirstOrDefault(x => x.GUID == timeMachine.Properties.GUID && !x.IsPlaying && !x.IsRecording && Utils.CurrentTime >= x.StartTime && Utils.CurrentTime < x.EndTime);

            if (waybackMachine == default)
                return Create(timeMachine);
            else
                waybackMachine.TimeMachine = timeMachine;

            return waybackMachine;
        }
    }
}
