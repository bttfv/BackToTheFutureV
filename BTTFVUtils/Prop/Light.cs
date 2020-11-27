using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionLibrary
{
    public class Light
    {
        public Light(float positionX, float positionY, float positionZ, float directionX, float directionY, float directionZ, Color color, float distance, float brightness, float roundness, float radius, float fadeout)
        {
            PositionX = positionX;
            PositionY = positionY;
            PositionZ = positionZ;

            DirectionX = directionX;
            DirectionY = directionY;
            DirectionZ = directionZ;

            Distance = distance;
            Brightness = brightness;
            Roundness = roundness;
            Radius = radius;
            Fadeout = fadeout;

            Color = color;
        }

        public Light(string sourceBone, string directionBone, Color color, float distance, float brightness, float roundness, float radius, float fadeout)
        {
            SourceBone = sourceBone;
            DirectionBone = directionBone;

            Distance = distance;
            Brightness = brightness;
            Roundness = roundness;
            Radius = radius;
            Fadeout = fadeout;

            Color = color;

            useBones = true;
        }

        private bool useBones;

        public string SourceBone { get; set; }
        public string DirectionBone { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        public float DirectionZ { get; set; }
        public float Distance { get; set; }
        public float Brightness { get; set; }
        public float Roundness { get; set; }
        public float Radius { get; set; }
        public float Fadeout { get; set; }
        public Color Color { get; set; }
        public bool IsEnabled { get; set; } = true;

        public void Draw(Entity Entity, float shadowId)
        {
            if (!IsEnabled)
                return;

            Vector3 pos;
            Vector3 dir;

            if (useBones)
            {
                pos = Entity.Bones[SourceBone].Position;

                dir = Vector3.Subtract(Entity.Bones[DirectionBone].Position, Entity.Bones[SourceBone].Position);
                dir.Normalize();
            }
            else
            {
                pos = Entity.GetOffsetPosition(new Vector3(PositionX, PositionY, PositionZ));
                dir = new Vector3(DirectionX, DirectionY, DirectionZ);
            }

            Function.Call(Hash._DRAW_SPOT_LIGHT_WITH_SHADOW, pos.X, pos.Y, pos.Z, dir.X, dir.Y, dir.Z, Color.R, Color.G, Color.B, Distance, Brightness, Roundness, Radius, Fadeout, shadowId);
        }
    }
}
