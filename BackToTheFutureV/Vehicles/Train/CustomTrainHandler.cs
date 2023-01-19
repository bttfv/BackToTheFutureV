using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal static class CustomTrainHandler
    {
        private static readonly List<CustomTrain> trainHandlers = new List<CustomTrain>();
        private static readonly List<CustomTrain> trainHandlersToRemove = new List<CustomTrain>();

        public static CustomTrain CreateFreightTrain(Vehicle vehicle, bool direction)
        {
            trainHandlers.Add(new CustomTrain(vehicle.GetOffsetPosition(new Vector3(0, 100f, 0)), direction, 3, 0));
            trainHandlers.Last().CruiseSpeedMPH = 40;
            trainHandlers.Last().SetToDestroy(vehicle, 35);

            return trainHandlers.Last();
        }

        public static CustomTrain CreateInvisibleTrain(Vehicle vehicle, bool direction)
        {
            trainHandlers.Add(new CustomTrain(vehicle.GetOffsetPosition(new Vector3(0, -10, 0)), direction, 26, 1));

            trainHandlers.Last().SetCollision(false);

            trainHandlers.Last().SetVisible(false);
            trainHandlers.Last().SetHorn(false);

            return trainHandlers.Last();
        }

        public static void Tick()
        {
            if (trainHandlersToRemove.Count > 0)
            {
                trainHandlersToRemove.ForEach(x => trainHandlers.Remove(x));
                trainHandlersToRemove.Clear();
            }

            trainHandlers.ForEach(x =>
            {
                if (!x.Exists || !x.Train.Exists())
                {
                    trainHandlersToRemove.Add(x);
                    return;
                }

                x.Tick();
            });
        }

        public static void Abort()
        {
            trainHandlers.ForEach(x => x.DeleteTrain());
        }
    }
}
