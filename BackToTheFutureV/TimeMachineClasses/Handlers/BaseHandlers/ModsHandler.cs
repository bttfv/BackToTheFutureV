using FusionLibrary;
using GTA;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class ModsHandler : DMC12Mods
    {
        protected TimeMachine TimeMachine { get; }

        public CVehicleWheels Wheels { get; }

        public ModsHandler(TimeMachine timeMachine, WormholeType wormholeType) : base(timeMachine.Vehicle)
        {
            TimeMachine = timeMachine;

            Wheels = new CVehicleWheels(timeMachine);

            if (IsDMC12)
            {
                Vehicle.ToggleExtra(10, false);

                if ((ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index != ReactorType.None)
                {
                    SyncMods();

                    return;
                }
            }

            WormholeType = wormholeType;

            Components = ModState.On;
            OffCoils = ModState.On;
            Hook = HookState.Off;
            Hood = HoodType.Stock;

            switch (WormholeType)
            {
                case WormholeType.BTTF1:
                    Reactor = ReactorType.Nuclear;
                    Exhaust = ExhaustType.BTTF;
                    Plate = PlateType.Outatime;
                    break;
                case WormholeType.BTTF2:
                    Reactor = ReactorType.MrFusion;
                    Exhaust = ExhaustType.None;
                    Plate = PlateType.BTTF2;

                    HoverUnderbody = ModState.On;
                    break;
                case WormholeType.BTTF3:
                    Reactor = ReactorType.MrFusion;
                    Exhaust = ExhaustType.BTTF;
                    Plate = PlateType.BTTF2;

                    Hoodbox = ModState.On;
                    Wheel = WheelType.Red;
                    SuspensionsType = SuspensionsType.LiftFront;
                    break;
            }
        }

        public void SyncMods()
        {
            SuspensionsType suspensionsType = (SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index;

            WormholeType wormholeType = (WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index;

            if (wormholeType != WormholeType)
            {
                WormholeType = wormholeType;
            }

            WheelType wheelType = (WheelType)Vehicle.Mods[VehicleModType.FrontWheel].Index;

            if (wheelType != Wheel)
            {
                Wheel = wheelType;
            }

            if (Wheel == WheelType.RailroadInvisible && TimeMachine.Props != null && !TimeMachine.Props.RRWheels.IsSpawned)
            {
                TimeMachine.Props?.RRWheels?.SpawnProp();
            }

            ReactorType reactorType = (ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index;

            if (reactorType != Reactor)
            {
                Reactor = reactorType;
            }

            PlateType plateType = (PlateType)Vehicle.Mods[VehicleModType.Ornaments].Index;

            if (plateType != Plate)
            {
                Plate = plateType;
            }

            ExhaustType exhaustType = (ExhaustType)Vehicle.Mods[VehicleModType.Windows].Index;

            if (exhaustType != Exhaust)
            {
                Exhaust = exhaustType;
            }

            HoodType hoodType = (HoodType)Vehicle.Mods[VehicleModType.Hood].Index;

            if (hoodType != Hood)
            {
                Hood = hoodType;
            }

            ModState modState = (ModState)Vehicle.Mods[VehicleModType.Spoilers].Index;

            if (modState != Components)
            {
                Components = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.FrontBumper].Index;

            if (modState != OffCoils)
            {
                OffCoils = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.Frame].Index;

            if (modState != GlowingEmitter)
            {
                GlowingEmitter = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.Fender].Index;

            if (modState != GlowingReactor)
            {
                GlowingReactor = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.DialDesign].Index;

            if (modState != HoverUnderbody)
            {
                HoverUnderbody = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.Livery].Index;

            if (modState != Hoodbox)
            {
                Hoodbox = modState;
            }

            HookState hookState = HookState.Unknown;

            if (Vehicle.Mods[VehicleModType.Roof].Index == 1 && Vehicle.Mods[VehicleModType.ArchCover].Index == -1 && Vehicle.Mods[VehicleModType.Grille].Index == -1)
            {
                hookState = HookState.Off;
            }

            if (Vehicle.Mods[VehicleModType.Roof].Index == 0 && Vehicle.Mods[VehicleModType.ArchCover].Index == 0 && Vehicle.Mods[VehicleModType.Grille].Index == 0)
            {
                hookState = HookState.OnDoor;
            }

            if (Vehicle.Mods[VehicleModType.Roof].Index == 0 && Vehicle.Mods[VehicleModType.ArchCover].Index == 1 && Vehicle.Mods[VehicleModType.Grille].Index == 1)
            {
                hookState = HookState.On;
            }

            if (Vehicle.Mods[VehicleModType.Roof].Index == 0 && Vehicle.Mods[VehicleModType.ArchCover].Index == -1 && Vehicle.Mods[VehicleModType.Grille].Index == 0)
            {
                hookState = HookState.Removed;
            }

            if (hookState != Hook)
            {
                Hook = hookState;
            }

            if (suspensionsType != SuspensionsType)
            {
                SuspensionsType = suspensionsType;
            }
        }

        public new WheelType Wheel
        {
            get
            {
                return base.Wheel;
            }

            set
            {
                if (!IsDMC12 && HoverUnderbody == ModState.On && value != WheelType.DMC && value != WheelType.Red && value != WheelType.DMCInvisible && value != WheelType.RedInvisible)
                {
                    HoverUnderbody = ModState.Off;
                }

                base.Wheel = value;

                if (TimeMachine.Properties == null)
                {
                    return;
                }

                if (value == WheelType.RailroadInvisible)
                {
                    TimeMachine.Props?.RRWheels?.SpawnProp();

                    if (!IsDMC12)
                    {
                        return;
                    }

                    if (HoverUnderbody == ModState.On)
                    {
                        HoverUnderbody = ModState.Off;
                    }

                    if (SuspensionsType != SuspensionsType.Stock)
                    {
                        SuspensionsType = SuspensionsType.Stock;
                    }

                    if (WormholeType == WormholeType.BTTF3)
                    {
                        TimeMachine.Events?.OnWormholeTypeChanged?.Invoke();
                    }

                    Wheels.Burst = true;
                }
                else
                {
                    TimeMachine.Props?.RRWheels?.Delete();

                    Wheels.Burst = false;
                }
            }
        }

        public new SuspensionsType SuspensionsType
        {
            get
            {
                return base.SuspensionsType;
            }

            set
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                    {
                        return;
                    }
                }

                base.SuspensionsType = value;

                if (!IsDMC12)
                {
                    return;
                }

                if (value != SuspensionsType.Stock)
                {
                    if (HoverUnderbody == ModState.On)
                    {
                        HoverUnderbody = ModState.Off;
                    }

                    if (Wheel == WheelType.RailroadInvisible)
                    {
                        Wheel = WheelType.Stock;
                    }
                }
                else
                {
                    if (TimeMachine.Decorators != null)
                    {
                        TimeMachine.Decorators.TorqueMultiplier = 1;
                    }
                }
            }
        }

        public new ModState HoverUnderbody
        {
            get
            {
                return base.HoverUnderbody;
            }

            set
            {
                if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                {
                    return;
                }

                base.HoverUnderbody = value;

                bool reload = false;

                if (value == ModState.On)
                {
                    if (Wheel == WheelType.RailroadInvisible)
                    {
                        Wheel = WheelType.Stock;
                    }

                    if (!IsDMC12 && Wheel != WheelType.DMC && Wheel != WheelType.Red)
                    {
                        Wheel = WheelType.DMC;
                    }

                    reload = SuspensionsType != SuspensionsType.Stock;

                    if (SuspensionsType != SuspensionsType.Stock)
                    {
                        SuspensionsType = SuspensionsType.Stock;
                    }

                    Exhaust = ExhaustType.None;
                }

                TimeMachine.DMC12?.SetStockSuspensions?.Invoke(value == ModState.Off);

                TimeMachine.Events?.OnHoverUnderbodyToggle?.Invoke(reload);
            }
        }

        public new ExhaustType Exhaust
        {
            get
            {
                return base.Exhaust;
            }

            set
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                    {
                        return;
                    }
                }

                base.Exhaust = value;

                if (IsDMC12 && value != ExhaustType.None && HoverUnderbody == ModState.On)
                {
                    HoverUnderbody = ModState.Off;
                }
            }
        }

        public new WormholeType WormholeType
        {
            get
            {
                return base.WormholeType;
            }

            set
            {
                base.WormholeType = value;

                TimeMachine.Events?.OnWormholeTypeChanged?.Invoke();
            }
        }

        public new ReactorType Reactor
        {
            get
            {
                return base.Reactor;
            }

            set
            {
                base.Reactor = value;

                TimeMachine.Events?.OnReactorTypeChanged?.Invoke();
            }
        }

        public new ModState Hoodbox
        {
            get
            {
                return base.Hoodbox;
            }

            set
            {
                base.Hoodbox = value;

                if (value == ModState.On)
                {
                    if (TimeMachine.Properties == null)
                    {
                        return;
                    }

                    if (TimeMachine.Properties.AreTimeCircuitsOn)
                    {
                        TimeMachine.Events?.SetTimeCircuits?.Invoke(false);
                    }

                    if (!TimeMachine.Properties.AreTimeCircuitsBroken)
                    {
                        return;
                    }

                    WormholeType = WormholeType.BTTF3;
                }
                else
                {
                    if (TimeMachine.Properties.AreTimeCircuitsOn)
                    {
                        TimeMachine.Events.SetTimeCircuits?.Invoke(false);
                    }

                    TimeMachine.Properties.AreHoodboxCircuitsReady = false;
                }
            }
        }
    }
}
