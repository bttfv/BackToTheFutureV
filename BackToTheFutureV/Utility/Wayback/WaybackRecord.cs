using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using System.IO;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackRecord
    {
        public DateTime Time { get; }
        public float FrameTime { get; }

        public WaybackPed Ped { get; set; }
        public WaybackVehicle Vehicle { get; set; }

        public WaybackRecord(Ped ped)
        {
            Time = FusionUtils.CurrentTime;
            FrameTime = Game.LastFrameTime;

            Ped = new WaybackPed(ped);

            Vehicle vehicle = ped.GetUsingVehicle();

            if (!vehicle.NotNullAndExists())
            {
                return;
            }

            Vehicle = new WaybackVehicle(vehicle);
        }

        public void Apply(Ped ped, WaybackRecord nextRecord)
        {
            float adjustedRatio = 1 - (FrameTime / Game.LastFrameTime);

            Vehicle vehicle = null;

            if (Vehicle != null)
            {
                vehicle = Vehicle.Apply(nextRecord.Vehicle?.Replica, adjustedRatio, ped);
            }

            Ped.Apply(ped, vehicle, nextRecord.Ped.Replica, adjustedRatio);
        }

        public Ped Spawn(WaybackRecord nextRecord)
        {
            float adjustedRatio = 1 - (FrameTime / Game.LastFrameTime);

            Ped ped = World.GetClosestPed(FusionUtils.Lerp(Ped.Replica.Position, nextRecord.Ped.Replica.Position, adjustedRatio), 3, Ped.Replica.Model);

            if (ped.NotNullAndExists() && ped != FusionUtils.PlayerPed)
            {
                return ped;
            }

            return Ped.Replica.Spawn(FusionUtils.Lerp(Ped.Replica.Position, nextRecord.Ped.Replica.Position, adjustedRatio), FusionUtils.Lerp(Ped.Replica.Heading, nextRecord.Ped.Replica.Heading, adjustedRatio));
        }

        public static WaybackRecord FromData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                try
                {
                    return (WaybackRecord)FusionUtils.BinaryFormatter.Deserialize(stream);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static implicit operator byte[](WaybackRecord command)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                FusionUtils.BinaryFormatter.Serialize(stream, command);
                return stream.ToArray();
            }
        }
    }
}
