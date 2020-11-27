using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace FusionLibrary
{
    public class SpawnLocation
    {
        public static List<SpawnLocation> SpawnLocations;

        public Vector3 Position;
        public Vector3 CameraPos = Vector3.Zero;
        public Vector3 CameraDir = Vector3.Zero;
        public bool Direction;
        public string Name;

        public SpawnLocation(Vector3 position, bool direction)
        {
            Position = position;
            Direction = direction;
            Name = World.GetZoneLocalizedName(position);
        }

        public SpawnLocation(Vector3 position, Vector3 cameraPos, Vector3 cameraDir, bool direction = true)
        {
            Position = position;
            Direction = direction;
            Name = World.GetZoneLocalizedName(position);
            CameraPos = cameraPos;
            CameraDir = cameraDir;
        }

        public override string ToString()
        {
            return SpawnLocations.IndexOf(this).ToString();
        }
    }
}
