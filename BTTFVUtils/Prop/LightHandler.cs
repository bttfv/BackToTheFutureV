using GTA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionLibrary
{
    public class LightHandler
    {
        public List<Light> Lights = new List<Light>();

        private Entity Entity;

        private int ShadowMulti;

        public LightHandler(Entity entity, int shadowMulti)
        {
            this.Entity = entity;
            this.ShadowMulti = shadowMulti * 10;
        }

        public Light Add(string sourceBone, string directionBone, Color color, float distance, float brightness, float roundness, float radius, float fadeout)
        {
            Lights.Add(new Light(sourceBone, directionBone, color, distance, brightness, roundness, radius, fadeout));

            return Lights.Last();
        }

        public Light Add(float positionX, float positionY, float positionZ, float directionX, float directionY, float directionZ, Color color, float distance, float brightness, float roundness, float radius, float fadeout)
        {
            Lights.Add(new Light(positionX, positionY, positionZ, directionX, directionY, directionZ, color, distance, brightness, roundness, radius, fadeout));
            return Lights.Last();
        }

        public void Draw()
        {
            Lights.ForEach(x =>
            {
                x.Draw(Entity, (Lights.IndexOf(x) + 1) * ShadowMulti);
            });
        }
    }
}
