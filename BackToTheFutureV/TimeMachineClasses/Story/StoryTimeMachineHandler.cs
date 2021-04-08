using GTA.Math;
using System;
using System.Collections.Generic;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal static class StoryTimeMachineHandler
    {
        public static List<StoryTimeMachine> StoryTimeMachines { get; private set; } = new List<StoryTimeMachine>();

        static StoryTimeMachineHandler()
        {
            //Inside mine            
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-595.14f, 2085.36f, 130.78f), 13.78f, WormholeType.BTTF2, SpawnFlags.NoPosition | SpawnFlags.Broken, new DateTime(1885, 9, 1, 0, 0, 1), new DateTime(1955, 11, 13, 23, 59, 59), true, new DateTime(1885, 1, 1, 0, 0, 0), new DateTime(1955, 11, 12, 21, 43, 0)));

            //Parking lot
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-264.22f, -2092.08f, 26.76f), 287.57f, WormholeType.BTTF1, SpawnFlags.NoPosition, new DateTime(1985, 10, 26, 1, 15, 0), new DateTime(1985, 10, 26, 1, 35, 0), false, new DateTime(1985, 10, 26, 1, 21, 0)));

            //Desert
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(1746f, 3323f, 40.3f), 295.84f, WormholeType.BTTF3, SpawnFlags.NoPosition, new DateTime(1955, 11, 14, 0, 0, 0), new DateTime(1955, 11, 20, 0, 0, 0), false, new DateTime(1885, 1, 1, 0, 0, 0), new DateTime(1955, 11, 12, 21, 43, 0)));

            //Railroad
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(2611f, 1691.61f, 26.2f), 0, WormholeType.BTTF3, SpawnFlags.NoPosition, new DateTime(1885, 9, 6, 21, 0, 0), new DateTime(1885, 9, 7, 9, 0, 0), false, new DateTime(1885, 1, 2, 8, 0, 0), new DateTime(1955, 11, 14, 8, 0, 0), true));
        }

        public static void Tick()
        {
            foreach (StoryTimeMachine x in StoryTimeMachines)
                x.Tick();
        }

        public static void Abort()
        {
            foreach (StoryTimeMachine x in StoryTimeMachines)
            {
                if (x.Spawned && !x.IsUsed)
                    x.TimeMachine.Dispose();
            }
        }
    }
}
