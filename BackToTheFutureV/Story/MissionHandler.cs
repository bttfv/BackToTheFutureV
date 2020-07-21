using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Story
{
    public static class MissionHandler
    {
        private static List<Mission> _missions = new List<Mission>();

        public static TrainMission TrainMission = new TrainMission();

        public static void Add(Mission mission)
        {
            _missions.Add(mission);
        }

        public static void Delete(Mission mission)
        {
            _missions.Remove(mission);
        }

        public static void Process()
        {
            _missions.ForEach(x => x.Process());
        }

        public static void Abort()
        {
            foreach (var mission in _missions)
                mission.End();

            _missions.Clear();
        }
    }
}
