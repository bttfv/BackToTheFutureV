using BackToTheFutureV.TimeMachineClasses;
using System.Collections.Generic;

namespace BackToTheFutureV.Players
{
    public class FireTrailsHandler
    {
        private static List<FireTrail> fireTrails = new List<FireTrail>();

        public static FireTrail SpawnForTimeMachine(TimeMachine timeMachine)
        {
            FireTrail fireTrail = new FireTrail(timeMachine.Vehicle, timeMachine.Constants.FireTrailsIs99, timeMachine.Constants.FireTrailsDisappearTime, timeMachine.Constants.FireTrailsAppearTime, timeMachine.Constants.FireTrailsUseBlue, timeMachine.Constants.FireTrailsLength);
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
