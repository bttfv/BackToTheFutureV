using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
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

        public Vehicle Spawn()
        {
            Vehicle vehicle = Vehicle.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants | SpawnFlags.CheckExists);

            if (!IsTimeMachine || vehicle.IsTimeMachine())
                return vehicle;

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public void Apply(Vehicle vehicle, Ped ped, float timeRatio, WaybackMachineReplica nextReplica)
        {
            if (nextReplica == null)
                nextReplica = this;

            if (ped.IsEnteringVehicle() || ped.IsLeavingVehicle())
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants | SpawnFlags.ForcePosition, timeRatio, nextReplica.Vehicle);
            else
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants, timeRatio, nextReplica.Vehicle);

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
