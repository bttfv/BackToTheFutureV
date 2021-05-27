﻿using FusionLibrary;
using GTA;
using GTA.Native;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class DMC12Mods : ModsPrimitive
    {
        protected Vehicle Vehicle { get; }

        public DMC12Mods(Vehicle vehicle)
        {
            Vehicle = vehicle;

            if (Vehicle.Model == ModelHandler.DMC12)
                Type = TimeMachineType.DMC12;
            else if (Vehicle.Model == ModelHandler.JVTModel)
                Type = TimeMachineType.JVT;
            else
                Type = TimeMachineType.Other;

            Vehicle.Mods.InstallModKit();

            if (IsDMC12)
            {
                Vehicle.ToggleExtra(10, true);

                Vehicle.Mods.PrimaryColor = VehicleColor.BrushedAluminium;
                Vehicle.Mods.TrimColor = VehicleColor.PureWhite;

                Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);

                //Seats
                Vehicle.Mods[VehicleModType.VanityPlates].Index = 0;

                if (!(WormholeType > WormholeType.DMC12))
                    WormholeType = WormholeType.DMC12;

                if ((SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index == SuspensionsType.Unknown)
                    SuspensionsType = SuspensionsType.Stock;

                if ((WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index <= WormholeType.DMC12)
                    Hood = (HoodType)FusionUtils.Random.Next(-1, 2);
            }
        }

        public new WormholeType WormholeType
        {
            get
            {
                if (IsDMC12)
                {
                    WormholeType wormholeType = (WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index;

                    if (wormholeType != base.WormholeType)
                        base.WormholeType = wormholeType;
                }

                return base.WormholeType;
            }
            set
            {
                base.WormholeType = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.TrimDesign].Index = (int)value;
            }
        }

        public new SuspensionsType SuspensionsType
        {
            get => base.SuspensionsType;
            set
            {
                if (value == SuspensionsType.Unknown)
                    value = SuspensionsType.Stock;

                base.SuspensionsType = value;

                if (!IsDMC12)
                    return;
                else
                    Vehicle.Mods[VehicleModType.Hydraulics].Index = (int)value;

                switch (value)
                {
                    case SuspensionsType.Stock:
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, 0f);

                        Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, false);
                        Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, false);
                        break;
                    default:
                        Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, true);
                        Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, true);

                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0f);
                        FusionUtils.LiftUpWheel(Vehicle, WheelId.RearRight, 0f);
                        break;
                }
            }
        }

        public new WheelType Wheel
        {
            get => base.Wheel;
            set
            {
                base.Wheel = value;

                Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, Vehicle, 12);
                Vehicle.Mods[VehicleModType.FrontWheel].Index = (int)value;
            }
        }

        public new ModState Components
        {
            get => base.Components;
            set
            {
                base.Components = value;

                if (IsDMC12)
                {
                    //Exteriors
                    Vehicle.Mods[VehicleModType.Spoilers].Index = (int)value;
                    //Vents
                    Vehicle.Mods[VehicleModType.ColumnShifterLevers].Index = (int)value;
                    //Interiors
                    Vehicle.Mods[VehicleModType.SideSkirt].Index = (int)value;
                    //Steering wheel buttons
                    Vehicle.Mods[VehicleModType.SteeringWheels].Index = (int)value;
                    //Bulova clock
                    Vehicle.Mods[VehicleModType.RightFender].Index = (int)value;
                }
            }
        }

        public new ModState OffCoils
        {
            get => base.OffCoils;
            set
            {
                base.OffCoils = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.FrontBumper].Index = (int)value;
            }
        }

        public new ModState GlowingEmitter
        {
            get => base.GlowingEmitter;
            set
            {
                base.GlowingEmitter = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Frame].Index = (int)value;
            }
        }

        public new ModState GlowingReactor
        {
            get => base.GlowingReactor;
            set
            {
                base.GlowingReactor = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Fender].Index = (int)value;
            }
        }

        public new ModState HoverUnderbody
        {
            get => base.HoverUnderbody;
            set
            {
                base.HoverUnderbody = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.DialDesign].Index = (int)value;
            }
        }

        public new ReactorType Reactor
        {
            get => base.Reactor;
            set
            {
                base.Reactor = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Plaques].Index = (int)value;
            }
        }

        public new PlateType Plate
        {
            get => base.Plate;
            set
            {
                base.Plate = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Ornaments].Index = (int)value;
            }
        }

        public new ExhaustType Exhaust
        {
            get => base.Exhaust;
            set
            {
                base.Exhaust = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Windows].Index = (int)value;
            }
        }

        public new ModState Hoodbox
        {
            get => base.Hoodbox;
            set
            {
                base.Hoodbox = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Livery].Index = (int)value;
                    Vehicle.Mods[VehicleModType.Aerials].Index = (int)value;
                }
            }
        }

        public new HookState Hook
        {
            get => base.Hook;
            set
            {
                base.Hook = value;

                if (!IsDMC12)
                    return;

                switch (value)
                {
                    case HookState.Off:
                        Vehicle.Mods[VehicleModType.Roof].Index = 1;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = -1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = -1;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                    case HookState.OnDoor:
                        Vehicle.Mods[VehicleModType.Roof].Index = 0;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = 0;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = 0;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                    case HookState.On:
                        Vehicle.Mods[VehicleModType.Roof].Index = 0;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = 1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = 1;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                    case HookState.Removed:
                        Vehicle.Mods[VehicleModType.Roof].Index = 0;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = -1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = 0;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                }
            }
        }

        public new HoodType Hood
        {
            get => base.Hood;
            set
            {
                base.Hood = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Hood].Index = (int)value;
            }
        }
    }
}
