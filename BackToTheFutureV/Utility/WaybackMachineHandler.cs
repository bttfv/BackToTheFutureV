using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static WaybackMachine Start(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = Script.InstantiateScript<WaybackMachine>();

            waybackMachine.Create(timeMachine);

            //GTA.UI.Screen.ShowSubtitle("New wayback machine");

            return waybackMachine;
        }

        public static WaybackMachine Exists(TimeMachine timeMachine)
        {
            return WaybackMachines.FirstOrDefault(x => x.GUID == timeMachine.Properties.GUID && !x.IsRecording && Utils.CurrentTime >= x.StartTime && Utils.CurrentTime <= x.EndTime);
        }

        public static WaybackMachine Assign(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = Exists(timeMachine);

            if (waybackMachine == default)
                return Start(timeMachine);
            else
                waybackMachine.TimeMachine = timeMachine;

            //GTA.UI.Screen.ShowSubtitle("Wayback machine found");
            
            return waybackMachine;
        }
    }
}
