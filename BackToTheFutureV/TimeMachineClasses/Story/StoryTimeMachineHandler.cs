using GTA.Chrono;
using GTA.Math;
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
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-595.14f, 2085.36f, 130.78f), 13.78f, WormholeType.BTTF2, SpawnFlags.NoPosition | SpawnFlags.Broken, new GameClockDateTime(GameClockDate.FromYmd(1885, 9, 1), GameClockTime.FromHms(0, 0, 1)), new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 14), GameClockTime.FromHms(12, 0, 0)), true, new GameClockDateTime(GameClockDate.FromYmd(1885, 1, 1), GameClockTime.FromHms(0, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 12), GameClockTime.FromHms(21, 44, 0))));

            //Parking lot
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-264.22f, -2092.08f, 26.76f), 287.57f, WormholeType.BTTF1, SpawnFlags.NoPosition, new GameClockDateTime(GameClockDate.FromYmd(1985, 10, 26), GameClockTime.FromHms(1, 15, 0)), new GameClockDateTime(GameClockDate.FromYmd(1985, 10, 26), GameClockTime.FromHms(1, 35, 0)), false, new GameClockDateTime(GameClockDate.FromYmd(1985, 10, 26), GameClockTime.FromHms(1, 21, 0))));

            //Signboard
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-2504.5f, 3645.47f, 13.47f), 0, WormholeType.BTTF2, SpawnFlags.NoPosition, new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 12), GameClockTime.FromHms(6, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 12), GameClockTime.FromHms(21, 28, 0)), false, new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 12), GameClockTime.FromHms(6, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1985, 10, 27), GameClockTime.FromHms(2, 42, 0))));

            //Desert
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(1746f, 3323f, 40.3f), 295.84f, WormholeType.BTTF3, SpawnFlags.NoPosition, new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 16), GameClockTime.FromHms(8, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 16), GameClockTime.FromHms(10, 20, 0)), false, new GameClockDateTime(GameClockDate.FromYmd(1885, 1, 1), GameClockTime.FromHms(0, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 12), GameClockTime.FromHms(21, 44, 0))));

            //Railroad
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(2611f, 1691.61f, 26.2f), 180, WormholeType.BTTF3, SpawnFlags.NoPosition, new GameClockDateTime(GameClockDate.FromYmd(1885, 9, 6), GameClockTime.FromHms(21, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1885, 9, 7), GameClockTime.FromHms(9, 0, 0)), false, new GameClockDateTime(GameClockDate.FromYmd(1885, 9, 2), GameClockTime.FromHms(8, 0, 0)), new GameClockDateTime(GameClockDate.FromYmd(1955, 11, 16), GameClockTime.FromHms(10, 20, 0)), true));
        }

        public static void Tick()
        {
            foreach (StoryTimeMachine x in StoryTimeMachines)
            {
                x.Tick();
            }
        }

        public static void Abort()
        {
            foreach (StoryTimeMachine x in StoryTimeMachines)
            {
                if (x.Spawned && !x.IsUsed)
                {
                    x.TimeMachine.Dispose();
                }
            }
        }
    }
}
