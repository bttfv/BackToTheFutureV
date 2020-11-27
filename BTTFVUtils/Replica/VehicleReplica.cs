using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FusionLibrary.Enums;

namespace FusionLibrary
{
    public class VehicleReplica
    {
        public int Model { get; set; }
        public Vector3 Velocity { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Heading { get; set; }
        public float Speed { get; }
        public float Health { get; }
        public float EngineHealth { get; }
        public bool EngineRunning { get; }
        public VehicleColor PrimaryColor { get; }
        public VehicleColor SecondaryColor { get; }
        public int Livery { get; }
        public List<PedReplica> Occupants { get; }

        public VehicleReplica(Vehicle veh)
        {
            Model = veh.Model;
            Velocity = veh.Velocity;
            Position = veh.Position;
            Rotation = veh.Rotation;
            Heading = veh.Heading;
            Speed = veh.Speed;
            Health = veh.HealthFloat;
            EngineHealth = veh.EngineHealth;
            EngineRunning = veh.IsEngineRunning;
            PrimaryColor = veh.Mods.PrimaryColor;
            SecondaryColor = veh.Mods.SecondaryColor;
            Livery = veh.Mods.Livery;

            //Occupants = new List<PedInfo>();

            //foreach (Ped x in veh.Occupants)
            //    Occupants.Add(new PedInfo(x));
        }

        public Vehicle Spawn(SpawnFlags spawnFlags, Vector3 position = default, float heading = default)
        {
            Vehicle veh;

            if (spawnFlags.HasFlag(SpawnFlags.ForcePosition))
                veh = World.CreateVehicle(Model, position, heading);
            else
                veh = World.CreateVehicle(Model, Position, Heading);

            ApplyTo(veh, spawnFlags);

            return veh;
        }

        public void ApplyTo(Vehicle veh, SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            if (!spawnFlags.HasFlag(SpawnFlags.ForcePosition))
            {
                veh.Position = Position;
                veh.Rotation = Rotation;
                veh.Heading = Heading;
            }

            if (!spawnFlags.HasFlag(SpawnFlags.NoVelocity))
            {
                veh.Velocity = Velocity;
                veh.Speed = Speed;
            }

            veh.HealthFloat = Health;
            veh.EngineHealth = EngineHealth;
            veh.IsEngineRunning = EngineRunning;
            veh.Mods.PrimaryColor = PrimaryColor;
            veh.Mods.SecondaryColor = SecondaryColor;
            veh.Mods.Livery = Livery;

            //if (!spawnFlags.HasFlag(SpawnFlags.NoOccupants))
            //    foreach (PedInfo pedInfo in Occupants)
            //        pedInfo.Spawn(veh);
        }
    }
}
