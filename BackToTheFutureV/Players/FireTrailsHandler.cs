using BackToTheFutureV.TimeMachineClasses;
using System.Collections.Generic;

namespace BackToTheFutureV.Players
{
    public class FireTrailsHandler
    {
        private static List<FireTrail> fireTrails = new List<FireTrail>();

        public static FireTrail SpawnForDelorean(TimeMachine timeMachine, bool is99 = false, float disappearTime = 45, int appearTime = 15, bool useBlue = true, int maxLength = 50)
        {
            var fireTrail = new FireTrail(timeMachine.Vehicle, is99, disappearTime, appearTime, useBlue, maxLength);
            fireTrails.Add(fireTrail);

            return fireTrail;
        }

        public static void RemoveTrail(FireTrail trail)
        {
            trail.Stop();
            fireTrails.Remove(trail);
        }

        public static void Process()
        {
            fireTrails.ForEach(x => x.Process());
        }

        public static void Stop()
        {
            fireTrails.ForEach(x => x.Stop());
        }
    }
}
