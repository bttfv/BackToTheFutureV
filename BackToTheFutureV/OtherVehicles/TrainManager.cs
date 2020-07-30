using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV.Utility
{
    public static class TrainManager
    {
        private static List<TrainHandler> trainHandlers = new List<TrainHandler>();

        public static TrainHandler CreateFreightTrain(Vehicle vehicle, bool direction)
        {
            trainHandlers.Add(new TrainHandler(vehicle.GetOffsetPosition(new Vector3(0, 100f, 0)), direction, 3, 0));
            trainHandlers.Last().CruiseSpeedMPH = 40;
            trainHandlers.Last().SetToDestroy(vehicle, 35);

            return trainHandlers.Last();
        }

        public static TrainHandler CreateInvisibleTrain(Vehicle vehicle, bool direction)
        {
            trainHandlers.Add(new TrainHandler(vehicle.GetOffsetPosition(new Vector3(0, -10, 0)), direction, 25, 1));

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
