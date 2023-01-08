using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.Collections.Generic;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal static class BTTFImportantDates
    {
        public static readonly List<DateTime> Dates = new List<DateTime>() { new DateTime(1985, 10, 26, 1, 21, 0), new DateTime(1885, 1, 1, 0, 0, 0), new DateTime(1955, 11, 5, 6, 15, 0), new DateTime(1985, 10, 26, 1, 24, 0), new DateTime(1985, 10, 26, 1, 24, 0), new DateTime(2015, 10, 21, 16, 29, 0), new DateTime(1955, 11, 12, 13, 40, 0), new DateTime(1985, 10, 26, 21, 0, 0), new DateTime(1955, 11, 12, 6, 0, 0), new DateTime(1885, 9, 2, 8, 0, 0), new DateTime(1985, 10, 27, 11, 0, 0) };

        public static DateTime GetRandom()
        {
            return Dates[FusionUtils.Random.Next(0, Dates.Count - 1)];
        }
    }

    internal static class InternalExtensions
    {
        public static bool NotNullAndExists(this TimeMachine timeMachine)
        {
            return timeMachine != null && timeMachine.Vehicle.NotNullAndExists();
        }

        public static bool IsFunctioning(this HoverVehicle hoverVehicle)
        {
            return hoverVehicle != null && hoverVehicle.Vehicle.IsFunctioning();
        }

        public static bool IsFunctioning(this TimeMachine timeMachine)
        {
            return timeMachine != null && timeMachine.Vehicle.IsFunctioning();
        }

        public static bool IsFunctioning(this DMC12 dmc12)
        {
            return dmc12 != null && dmc12.Vehicle.IsFunctioning();
        }

        public static bool IsTimeMachine(this Vehicle vehicle)
        {
            return TimeMachineHandler.IsVehicleATimeMachine(vehicle);
        }

        public static TimeMachineClone Clone(this TimeMachine timeMachine)
        {
            return new TimeMachineClone(timeMachine);
        }

        public static TimeMachine TransformIntoTimeMachine(this Vehicle vehicle, WormholeType wormholeType = WormholeType.BTTF1)
        {
            return TimeMachineHandler.Create(vehicle, SpawnFlags.Default, wormholeType);
        }

        public static ModState ConvertFromBool(bool value)
        {
            if (value)
            {
                return ModState.On;
            }
            else
            {
                return ModState.Off;
            }
        }

        public static bool ConvertFromModState(ModState value)
        {
            if (value == ModState.On)
            {
                return true;
            }

            return false;
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
                case WheelType.DMC:
                    return WheelType.DMCInvisible;
                case WheelType.DMCInvisible:
                    return WheelType.DMC;
                default:
                    return WheelType.Stock;
            }
        }
    }
}
