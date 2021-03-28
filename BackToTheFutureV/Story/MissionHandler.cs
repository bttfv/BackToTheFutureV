using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal static class MissionHandler
    {
        private static List<Mission> _missions = new List<Mission>();

        public static TrainMission TrainMission = new TrainMission();

        public static EscapeMission EscapeMission = new EscapeMission();

        public static void Add(Mission mission)
        {
            _missions.Add(mission);
        }

        public static void Tick()
        {
            _missions.ForEach(x => x.Tick());
        }

        public static void KeyDown(KeyEventArgs key)
        {
            foreach (Mission mission in _missions)
                mission.KeyDown(key);
        }

        public static void Abort()
        {
            foreach (Mission mission in _missions)
                mission.End();

            _missions.Clear();
        }
    }
}
