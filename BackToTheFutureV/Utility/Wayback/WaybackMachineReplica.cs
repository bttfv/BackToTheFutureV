using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.Enums;

namespace BackToTheFutureV
{
    internal class WaybackMachineReplica
    {
        public VehicleReplica Vehicle { get; }

        public bool IsTimeMachine { get; }

        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }

        public WaybackMachineEvent Event { get; set; } = WaybackMachineEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackMachineReplica(Vehicle vehicle)
        {
            Vehicle = new VehicleReplica(vehicle, SpawnFlags.NoOccupants);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
                return;

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();

            Event = timeMachine.Event;
            TimeTravelDelay = timeMachine.TimeTravelDelay;

            timeMachine.Event = WaybackMachineEvent.None;
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle = Vehicle.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants | SpawnFlags.CheckExists);

            if (!IsTimeMachine || vehicle.IsTimeMachine())
                return vehicle;

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public Vehicle TryFindOrSpawn(float adjustedRatio, WaybackPedReplica nextReplica)
        {
            Vector3 position = Vehicle.Position;

            if (nextReplica.WaybackMachineReplica != null)
                position = Utils.Lerp(position, nextReplica.WaybackMachineReplica.Vehicle.Position, adjustedRatio);

            Vehicle vehicle = World.GetClosestVehicle(position, 1, Vehicle.Model);

            if (vehicle == null && !IsTimeMachine)
                vehicle = Spawn();

            return vehicle;
        }

        public void Apply(Vehicle vehicle, Ped ped, float adjusteRatio, WaybackPedReplica nextReplica)
        {
            VehicleReplica nextVehicleReplica = null;

            if (nextReplica.WaybackMachineReplica != null)
                nextVehicleReplica = nextReplica.WaybackMachineReplica.Vehicle;

            if (ped.IsEnteringVehicle() || ped.IsLeavingVehicle())
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants | SpawnFlags.ForcePosition, adjusteRatio, nextVehicleReplica);
            else
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants, adjusteRatio, nextVehicleReplica);

            if (!IsTimeMachine)
                return;

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            Mods.ApplyToWayback(timeMachine);
            Properties.ApplyToWayback(timeMachine);

            if (Event == WaybackMachineEvent.None)
                return;

            switch (Event)
            {
                case WaybackMachineEvent.OnSparksEnded:
                    timeMachine.Events.OnSparksEnded?.Invoke(TimeTravelDelay);
                    break;
                case WaybackMachineEvent.OpenCloseReactor:
                    timeMachine.Events.SetOpenCloseReactor?.Invoke();
                    break;
                case WaybackMachineEvent.RefuelReactor:
                    timeMachine.Events.SetRefuel?.Invoke(ped);
                    break;
            }
        }
    }

}
