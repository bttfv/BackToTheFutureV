using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;

namespace BackToTheFutureV.Utility
{
    public static class RandomExtensions
    {
        public static double NextDouble(
            this Random random,
            double minValue,
            double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }

    public static class MathExtensions
    {
        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * by + secondFloat * (1 - by);
        }
    }

    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static Vector3 GetSingleOffset(this Vector3 vector3, Coordinate coordinate, float value)
        {
            switch (coordinate) {
                case Coordinate.X:
                    vector3.X += value;
                    break;
                case Coordinate.Y:
                    vector3.Y += value;
                    break;
                case Coordinate.Z:
                    vector3.Z += value;
                    break;
            }

            return vector3;
        }

        public static void AttachToPhysically(this Entity entity1, Entity toEntity, Vector3 offset, Vector3 rotation)
        {
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, entity1, toEntity, 0, 0, offset.X, offset.Y, offset.Z, 0, 0, 0, rotation.X, rotation.Y, rotation.Z, 1000000.0f, true, true, false, false, 2);
        }
    }
}
