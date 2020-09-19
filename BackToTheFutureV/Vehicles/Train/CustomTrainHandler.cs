using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV.Utility
{
    public static class CustomTrainHandler
    {
        private static List<CustomTrain> trainHandlers = new List<CustomTrain>();

        public static CustomTrain CreateFreightTrain(Vehicle vehicle, bool direction)
        {
            trainHandlers.Add(new CustomTrain(vehicle.GetOffsetPosition(new Vector3(0, 100f, 0)), direction, 3, 0));
            trainHandlers.Last().CruiseSpeedMPH = 40;
            trainHandlers.Last().SetToDestroy(vehicle, 35);

            return trainHandlers.Last();
        }

        public static CustomTrain CreateInvisibleTrain(Vehicle vehicle, bool direction)
        {
            trainHandlers.Add(new CustomTrain(vehicle.GetOffsetPosition(new Vector3(0, -10, 0)), direction, 25, 1));

            trainHandlers.Last().SetCollision(false);
            
            trainHandlers.Last().SetVisible(false);
            trainHandlers.Last().SetHorn(false);

            return trainHandlers.Last();
        }

        public static void Process()
        {
            trainHandlers.ForEach(x => {
                if (x.Process())
                    trainHandlers.Remove(x);
            });
        }

        public static void Abort()
        {
            trainHandlers.ForEach(x => x.DeleteTrain());
        }

    }
}
