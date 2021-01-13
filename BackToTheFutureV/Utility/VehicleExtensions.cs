using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Utility
{
    public class BTTFImportantDates
    {
        public readonly static List<DateTime> Dates = new List<DateTime>() { new DateTime(1985, 10, 26, 1, 21, 0), new DateTime(1885, 1, 1, 0, 0, 0), new DateTime(1955, 11, 5, 6, 15, 0), new DateTime(1985, 10, 26, 1, 24, 0), new DateTime(1985, 10, 26, 1, 24, 0), new DateTime(2015, 10, 21, 16, 29, 0), new DateTime(1955, 11, 12, 13, 40, 0), new DateTime(1985, 10, 26, 21, 0, 0), new DateTime(1955, 11, 12, 6, 0, 0), new DateTime(1885, 9, 2, 8, 0, 0), new DateTime(1985, 10, 27, 11, 0, 0) };

        public static DateTime GetRandom()
        {
            return Dates[Utils.Random.Next(0, Dates.Count - 1)];
        }
    }

    public static class VehicleExtensions
    {
        public static bool NotNullAndExists(this TimeMachine timeMachine)
        {
            return timeMachine != null && timeMachine.Vehicle.NotNullAndExists();
        }

        public static bool IsFunctioning(this TimeMachine timeMachine)
        {
            return timeMachine != null && timeMachine.Vehicle.IsFunctioning();
        }

        public static bool IsTimeMachine(this Vehicle vehicle)
        {
            return TimeMachineHandler.IsVehicleATimeMachine(vehicle);
        }

        public static TimeMachine TransformIntoTimeMachine(this Vehicle vehicle, WormholeType wormholeType = WormholeType.BTTF1)
        {
            return TimeMachineHandler.Create(vehicle, SpawnFlags.Default, wormholeType);
        }

        public static WheelType GetVariantWheelType(this WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.Stock:
                    return WheelType.StockInvisible;
                case WheelType.StockInvisible:
                    return WheelType.Stock;
                case WheelType.Red:
                    return WheelType.RedInvisible;
                case WheelType.RedInvisible:
                    return WheelType.Red;
                default:
                    return WheelType.Stock;
            }
        }
    }
}
