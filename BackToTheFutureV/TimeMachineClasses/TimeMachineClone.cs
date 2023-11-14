﻿using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA.Chrono;
using GTA.Math;
using System;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class TimeMachineClone
    {
        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }
        public VehicleReplica Vehicle { get; }

        public GameClockDateTime ExistsUntil { get; set; } = GameClockDateTime.MaxValue;

        public TimeMachineClone(TimeMachine timeMachine)
        {
            Mods = timeMachine.Mods.Clone();
            Properties = timeMachine.Properties.Clone();
            Vehicle = timeMachine.Vehicle.Clone();

            if (Properties.TimeTravelDestPos != Vector3.Zero)
            {
                Vehicle.Position = Vehicle.Position.TransferHeight(Properties.TimeTravelDestPos);
                Properties.TimeTravelDestPos = Vector3.Zero;
            }
        }

        public TimeMachine Spawn(SpawnFlags spawnFlags = SpawnFlags.Default)
        {
            return TimeMachineHandler.Create(this, spawnFlags);
        }

        public void ApplyTo(TimeMachine timeMachine, SpawnFlags spawnFlags)
        {
            Properties.ApplyTo(timeMachine);
            Mods.ApplyTo(timeMachine);
            Vehicle.ApplyTo(timeMachine.Vehicle, spawnFlags);
        }
    }
}
