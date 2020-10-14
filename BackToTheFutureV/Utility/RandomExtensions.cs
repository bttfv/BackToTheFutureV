using GTA;
using GTA.Math;
using GTA.Native;
using GTA.NaturalMotion;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

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

        public static bool NotNullAndExists(this Entity entity)
        {
            return entity != null && entity.Exists();
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

        public static void RequestCollision(this Vector3 position)
        {            
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, position.X, position.Y, position.Z);
        }

        public static void LoadScene(this Vector3 position)
        {
            Function.Call(Hash.NEW_LOAD_SCENE_START, position.X, position.Y, position.Z, 0.0f, 0.0f, 0.0f, 20.0f, 0);
        }

        public static Vector3 TransferHeight(this Vector3 src, Vector3 dst)
        {
            dst.Z += src.Z - World.GetGroundHeight(src);

            return dst;
        }

        public static bool MostlyNear(this float src, float to)
        {
            return (to - 5) <= src && src <= (to + 5);
        }

        public static float GetMostFreeDirection(this Vector3 position, Entity ignoreEntity)
        {
            float ret = 0;
            float maxDist = -1;
            Vector3 lastPos = Vector3.Zero;

            const float r = 1000f;

            position = position.GetSingleOffset(Coordinate.Z, 1);

            for (float i = 0; i <= 360; i += 15)
            {
                float angleRad = i * (float)Math.PI / 180;

                float x = r * (float)Math.Cos(angleRad);
                float y = r * (float)Math.Sin(angleRad);

                Vector3 circlePos = position;
                circlePos.X += y;
                circlePos.Y += x;
               
                // Then we check for every pos if it hits tracks material
                RaycastResult raycast = World.Raycast(position, circlePos, IntersectFlags.Everything, ignoreEntity);

                if (!raycast.DidHit)
                {
                    ret = i;
                    lastPos = circlePos;
                    break;
                }

                float curDist = raycast.HitPosition.DistanceTo2D(position);

                if (curDist > maxDist)
                {
                    maxDist = curDist;
                    ret = i;
                    lastPos = circlePos;
                }
            }

            if (lastPos != Vector3.Zero)
                Utils.DrawLine(position, lastPos, Color.Aqua);

            return ret;
        }

        public static void AttachToPhysically(this Entity entity1, Entity toEntity, Vector3 offset, Vector3 rotation)
        {
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, entity1, toEntity, 0, 0, offset.X, offset.Y, offset.Z, 0, 0, 0, rotation.X, rotation.Y, rotation.Z, 1000000.0f, true, true, false, false, 2);
        }

        public static float GetKineticEnergy(this Vehicle vehicle)
        {
            return 0.5f * HandlingData.GetByVehicleModel(vehicle.Model).Mass * (float)Math.Pow(vehicle.Speed, 2);
        }
    }
}
