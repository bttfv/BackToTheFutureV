using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

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
                    Tick();

                    return;
                }
            }

            WormholeType = wormholeType;

            Components = ModState.On;
            Speedo = ModState.Off;
            Bulova = ModState.Off;
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
                    Bulova = ModState.On;
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

        public void Tick()
        {
            SuspensionsType suspensionsType = (SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index;
            WormholeType wormholeType = (WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index;
            WheelType wheelType = (WheelType)Vehicle.Mods[VehicleModType.FrontWheel].Index;
            ReactorType reactorType = (ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index;
            PlateType plateType = (PlateType)Vehicle.Mods[VehicleModType.Ornaments].Index;
            ExhaustType exhaustType = (ExhaustType)Vehicle.Mods[VehicleModType.Exhaust].Index;
            HoodType hoodType = (HoodType)Vehicle.Mods[VehicleModType.Hood].Index;
            ModState modState = (ModState)Vehicle.Mods[VehicleModType.Spoilers].Index;
            HookState hookState = HookState.Unknown;

            if (wormholeType != WormholeType)
            {
                WormholeType = wormholeType;
            }

            if (wheelType != Wheel)
            {
                Wheel = wheelType;
            }

            if (reactorType != Reactor)
            {
                Reactor = reactorType;
            }

            if (plateType != Plate)
            {
                Plate = plateType;
            }

            if (exhaustType != Exhaust)
            {
                Exhaust = exhaustType;
            }

            if (hoodType != Hood)
            {
                Hood = hoodType;
            }

            if (modState != Components)
            {
                Components = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.RightFender].Index;

            if (modState != Bulova)
            {
                Bulova = modState;
            }

            modState = (ModState)Vehicle.Mods[VehicleModType.Dashboard].Index;

            if (modState != Speedo)
            {
                Speedo = modState;
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
                Vehicle.Velocity += Vector3.UnitY * 0.3f;
            }

            if (Wheel == WheelType.Red && VehicleControl.GetWheelSize(Vehicle) != 1.1f)
            {
                VehicleControl.SetWheelWidth(Vehicle, 0.976f);
                VehicleControl.SetWheelSize(Vehicle, 1.1f);
                Vehicle.Velocity += Vector3.UnitY * 0.3f;
            }

            switch (SuspensionsType)
            {
                case SuspensionsType.Stock:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, 0.0f));
                    break;
                case SuspensionsType.LiftFrontLowerRear:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, 0.05f));
                    break;
                case SuspensionsType.LiftFront:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, 0.0f));
                    break;
                case SuspensionsType.LiftRear:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, -0.1f));
                    break;
                case SuspensionsType.LiftFrontAndRear:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, -0.1f));
                    break;
                case SuspensionsType.LowerFrontLiftRear:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, -0.1f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, -0.1f));
                    break;
                case SuspensionsType.LowerFront:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, 0.0f));
                    break;
                case SuspensionsType.LowerRear:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, 0.0f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, 0.05f));
                    break;
                case SuspensionsType.LowerFrontAndRear:
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightFront].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightFront].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelLeftRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear].GetSingleOffset(Coordinate.Z, 0.05f));
                    CVehicle.Wheels[VehicleWheelBoneId.WheelRightRear].RelativePosition.Set(WheelStartOffsets[VehicleWheelBoneId.WheelRightRear].GetSingleOffset(Coordinate.Z, 0.05f));
                    break;
            }
        }

        public new WheelType Wheel
        {
            get => base.Wheel;

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
            get => base.SuspensionsType;

            set
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                    {
                        return;
                    }
                }

                bool newSuspension = false;

                if (TimeMachine.Mods != null && TimeMachine.Mods.SuspensionsType != value)
                {
                    newSuspension = true;
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
                        TimeMachine.Decorators.TorqueMultiplier = 1f;
                    }
                }

                if (newSuspension)
                {
                    TimeMachine.Vehicle.Velocity += GTA.Math.Vector3.UnitY * 0.3f;
                }
            }
        }

        public new ModState HoverUnderbody
        {
            get => base.HoverUnderbody;

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

                    /*if (TimeMachine.Vehicle.Model == "dproto" && Wheel != WheelType.DMC && Wheel != WheelType.Red)
                    {
                        Wheel = WheelType.DMC;
                    }*/

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
            get => base.Exhaust;

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
            get => base.WormholeType;

            set
            {
                base.WormholeType = value;

                TimeMachine.Events?.OnWormholeTypeChanged?.Invoke();
            }
        }

        public new ReactorType Reactor
        {
            get => base.Reactor;

            set
            {
                base.Reactor = value;

                TimeMachine.Events?.OnReactorTypeChanged?.Invoke();
            }
        }

        public new ModState Hoodbox
        {
            get => base.Hoodbox;

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
                }
            }
        }
    }
}
