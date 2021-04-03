using FusionLibrary;
using GTA;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal static class WaybackHandler
    {
        public static List<Wayback> WaybackMachines { get; } = new List<Wayback>();

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

        public static Wayback Create(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            Wayback waybackMachine = Script.InstantiateScript<Wayback>();

            waybackMachine.Create(timeMachine);

            return waybackMachine;
        }

        public static Wayback TryFind(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            Wayback waybackMachine = WaybackMachines.FirstOrDefault(x => x.GUID == timeMachine.Properties.GUID && !x.IsPlaying && !x.IsRecording && Utils.CurrentTime >= x.StartTime && Utils.CurrentTime < x.EndTime);

            if (waybackMachine == default)
                return Create(timeMachine);
            else
                waybackMachine.TimeMachine = timeMachine;

            return waybackMachine;
        }
    }
}
