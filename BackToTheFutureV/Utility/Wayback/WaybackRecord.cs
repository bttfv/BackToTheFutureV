﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Chrono;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WaybackRecord
    {
        public GameClockDateTime Time { get; }
        public float FrameTime { get; }

        public WaybackPed Ped { get; set; }
        public WaybackVehicle Vehicle { get; set; }

        public WaybackRecord(Ped ped, Vehicle vehicle = null)
        {
            Time = GameClock.Now;
            FrameTime = Game.LastFrameTime;

            Ped = new WaybackPed(ped);

            if (!vehicle.NotNullAndExists())
                vehicle = ped.GetUsingVehicle();

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
    }
}
