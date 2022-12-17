using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class WaybackVehicle
    {
        public VehicleReplica Replica { get; }

        public bool IsTimeMachine { get; }

        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackVehicle(TimeMachine timeMachine, WaybackVehicleEvent waybackVehicleEvent, int timeTravelDelay = 0)
        {
            Replica = new VehicleReplica(timeMachine.Vehicle, SpawnFlags.NoOccupants);

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();

            Event |= waybackVehicleEvent;
            TimeTravelDelay = timeTravelDelay;
        }

        public WaybackVehicle(Vehicle vehicle)
        {
            SpawnFlags spawnFlags = SpawnFlags.NoOccupants;

            if (vehicle.Model == ModelHandler.DMC12)
            {
                spawnFlags |= SpawnFlags.NoMods;
            }

            Replica = new VehicleReplica(vehicle, spawnFlags);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
            {
                return;
            }

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle = Replica.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants);

            vehicle.SetPlayerLights(true);

            if (!IsTimeMachine)
            {
                return vehicle;
            }

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public Vehicle TryFindOrSpawn(VehicleReplica nextReplica, float adjustedRatio)
        {
            Vehicle vehicle;

            if (nextReplica == null)
            {
                vehicle = World.GetClosestVehicle(Replica.Position, 5f, Replica.Model);
            }
            else
            {
                vehicle = World.GetClosestVehicle(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), 5f, Replica.Model);
            }

            if (!vehicle.NotNullAndExists() || FusionUtils.PlayerPed == vehicle)
            {
                vehicle = Spawn();
            }

            return vehicle;
        }

        public Vehicle Apply(VehicleReplica nextReplica, float adjustedRatio, Ped ped = null)
        {
            Vehicle vehicle = ped?.GetUsingVehicle();

            if (!vehicle.NotNullAndExists())
            {
                vehicle = TryFindOrSpawn(nextReplica, adjustedRatio);
            }

            if (!vehicle.NotNullAndExists())
            {
                return null;
            }

            SpawnFlags spawnFlags = SpawnFlags.NoOccupants;

            if (ped.NotNullAndExists() && (ped.IsEnteringVehicle() || ped.IsLeavingVehicle()))
            {
                spawnFlags |= SpawnFlags.NoPosition;
            }

            TimeMachine timeMachine;

            if (!IsTimeMachine || !Properties.IsOnTracks)
            {
                if (nextReplica == null)
                {
                    Replica.ApplyTo(vehicle, spawnFlags);
                }
                else
                {
                    Replica.ApplyTo(vehicle, spawnFlags, nextReplica, adjustedRatio);
                }
            }
            else if (Properties.IsOnTracks)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

                timeMachine.Events.SetTrainSpeed?.Invoke(Replica.Speed * (Replica.RunningDirection != RunningDirection.Backward ? 1 : -1));
            }

            if (!IsTimeMachine)
            {
                return vehicle;
            }

            if (Event.HasFlag(WaybackVehicleEvent.Transform))
            {
                timeMachine = TimeMachineHandler.Create(vehicle);
                Mods.ApplyTo(timeMachine);
                Properties.ApplyTo(timeMachine);

                return vehicle;
            }
            else
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);
            }

            if (!timeMachine.NotNullAndExists())
            {
                return vehicle;
            }

            Mods.ApplyToWayback(timeMachine);
            Properties.ApplyToWayback(timeMachine);

            if (Event == WaybackVehicleEvent.None)
            {
                return vehicle;
            }

            if (Event.HasFlag(WaybackVehicleEvent.OnSparksEnded))
            {
                timeMachine.Events.OnSparksEnded?.Invoke(TimeTravelDelay);
            }

            if (Event.HasFlag(WaybackVehicleEvent.LightningStrike))
            {
                timeMachine.Events.StartLightningStrike?.Invoke(0);
            }

            if (Event.HasFlag(WaybackVehicleEvent.LightningRun))
            {
                timeMachine.Events.StartLightningStrike?.Invoke(-1);
            }

            if (Event.HasFlag(WaybackVehicleEvent.OpenCloseReactor))
            {
                timeMachine.Events.SetReactorState?.Invoke(timeMachine.Properties.ReactorState == ReactorState.Closed ? ReactorState.Opened : ReactorState.Closed);
            }

            if (Event.HasFlag(WaybackVehicleEvent.RefuelReactor))
            {
                timeMachine.Events.SetReactorState?.Invoke(ReactorState.Refueling);
            }

            return vehicle;
        }
    }
}
