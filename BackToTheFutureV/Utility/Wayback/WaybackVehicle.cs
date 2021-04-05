using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.Enums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackVehicle
    {
        public VehicleReplica Vehicle { get; }

        public bool IsTimeMachine { get; }

        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackVehicle(TimeMachine timeMachine, WaybackVehicleEvent wvEvent = WaybackVehicleEvent.None, int timeTravelDelay = 0)
        {
            Vehicle = new VehicleReplica(timeMachine, SpawnFlags.NoOccupants);

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();

            Event = wvEvent;
            TimeTravelDelay = timeTravelDelay;
        }

        public WaybackVehicle(Vehicle vehicle)
        {
            Vehicle = new VehicleReplica(vehicle, SpawnFlags.NoOccupants);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
                return;

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle;

            if (IsTimeMachine)
            {
                vehicle = TimeMachineHandler.GetTimeMachineFromReplicaGUID(Properties.ReplicaGUID);

                if (vehicle.NotNullAndExists())
                    return vehicle;
            }

            vehicle = Vehicle.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants | SpawnFlags.CheckExists);

            if (!IsTimeMachine)
                return vehicle;

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public Vehicle TryFindOrSpawn(float adjustedRatio, WaybackPed nextReplica)
        {
            Vector3 position = Vehicle.Position;

            if (nextReplica.WaybackVehicle != null)
                position = Utils.Lerp(position, nextReplica.WaybackVehicle.Vehicle.Position, adjustedRatio);

            Vehicle vehicle = World.GetClosestVehicle(position, 1, Vehicle.Model);

            if (vehicle == null)
                vehicle = Spawn();

            return vehicle;
        }

        public void Apply(Vehicle vehicle, Ped ped, WaybackPed nextReplica, float adjusteRatio)
        {
            VehicleReplica nextVehicleReplica = null;

            if (nextReplica.WaybackVehicle != null)
                nextVehicleReplica = nextReplica.WaybackVehicle.Vehicle;

            TimeMachine timeMachine = null;

            if (IsTimeMachine)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

                if (timeMachine.NotNullAndExists() && timeMachine.Properties.TimeTravelPhase == TimeTravelPhase.Reentering)
                    return;
            }

            if (ped.IsEnteringVehicle() || ped.IsLeavingVehicle())
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants | SpawnFlags.ForcePosition, nextVehicleReplica, adjusteRatio);
            else
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants, nextVehicleReplica, adjusteRatio);

            if (!timeMachine.NotNullAndExists())
                return;

            Mods.ApplyToWayback(timeMachine);
            Properties.ApplyToWayback(timeMachine);

            if (Event == WaybackVehicleEvent.None)
                return;

            switch (Event)
            {
                case WaybackVehicleEvent.OnSparksEnded:
                    timeMachine.Events.OnSparksEnded?.Invoke(TimeTravelDelay);
                    break;
                case WaybackVehicleEvent.OpenCloseReactor:
                    timeMachine.Events.SetOpenCloseReactor?.Invoke();
                    break;
                case WaybackVehicleEvent.RefuelReactor:
                    timeMachine.Events.SetRefuel?.Invoke(ped);
                    break;
            }
        }
    }

}
