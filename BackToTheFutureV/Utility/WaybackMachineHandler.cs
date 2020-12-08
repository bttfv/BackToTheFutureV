using BackToTheFutureV.TimeMachineClasses;
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

        internal static void Process()
        {
            //WaybackMachines.ForEach(x => x.Process());
        }

        public static void ResetRecordings()
        {
            //WaybackMachines.ForEach(x => x.Stop(true));
            //TimeMachineHandler.TimeMachinesNoStory.ForEach(x => Create(x));
        }

        public static void Create(TimeMachine timeMachine)
        {
            //WaybackMachines.Add(new WaybackMachine(timeMachine));
        }
    }
}
