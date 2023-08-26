﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class DMC12Mods : ModsPrimitive
    {
        protected Vehicle Vehicle { get; }

        protected bool WasReloaded { get; set; }

        public Dictionary<VehicleWheelBoneId, Vector3> WheelStartOffsets { get; } = new Dictionary<VehicleWheelBoneId, Vector3>();

        public CVehicle CVehicle { get; }

        public DMC12Mods(Vehicle vehicle)
        {
            Vehicle = vehicle;

            CVehicle = new CVehicle(vehicle);

            WasReloaded = WheelStartOffsets.Count == 0 && (SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index != SuspensionsType.Unknown;

            foreach (KeyValuePair<VehicleWheelBoneId, CWheel> cWheel in CVehicle.Wheels)
            {
                WheelStartOffsets.Add(cWheel.Key, cWheel.Value.RelativePosition.Get());
            }

            if (WasReloaded)
            {
                switch ((SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index)
                {
                    case SuspensionsType.LiftFrontLowerRear:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightFront] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightRear] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        break;
                    case SuspensionsType.LiftFront:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightFront] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        break;
                    case SuspensionsType.LiftRear:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightRear] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        break;
                    case SuspensionsType.LiftFrontAndRear:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.07f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightFront] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.07f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.07f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightRear] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.07f);
                        break;
                    case SuspensionsType.LowerFrontLiftRear:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightFront] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightRear] += Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.1f);
                        break;
                    case SuspensionsType.LowerFront:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightFront] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        break;
                    case SuspensionsType.LowerRear:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightRear] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        break;
                    case SuspensionsType.LowerFrontAndRear:
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftFront] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightFront] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelLeftRear] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        WheelStartOffsets[VehicleWheelBoneId.WheelRightRear] -= Vector3.Zero.GetSingleOffset(Coordinate.Z, 0.05f);
                        break;
                    default:
                        break;
                }

                WasReloaded = false;
            }

            IsDMC12 = Vehicle.Model == ModelHandler.DMC12;

            if (IsDMC12)
            {
                Vehicle.Mods.InstallModKit();
                Function.Call(Hash.SET_HYDRAULICS_CONTROL, Vehicle, false);

                Vehicle.ToggleExtra(10, true);

                Vehicle.Mods.PrimaryColor = VehicleColor.BrushedAluminium;
                Vehicle.Mods.SecondaryColor = VehicleColor.MetallicBlackSteel;
                Vehicle.Mods.TrimColor = VehicleColor.PureWhite; // Gray Interior

                Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);
                Vehicle.DirtLevel = 0f;

                //Seats
                Vehicle.Mods[VehicleModType.VanityPlates].Index = 0;

                if (!(WormholeType > WormholeType.DMC12))
                {
                    WormholeType = WormholeType.DMC12;
                }

                if ((SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index == SuspensionsType.Unknown)
                {
                    SuspensionsType = SuspensionsType.Stock;
                }

                if ((WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index <= WormholeType.DMC12)
                {
                    Hood = (HoodType)FusionUtils.Random.Next(-1, 2);
                }
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
                    {
                        base.WormholeType = wormholeType;
                    }
                }

                return base.WormholeType;
            }
            set
            {
                base.WormholeType = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.TrimDesign].Index = (int)value;
                }
            }
        }

        public new SuspensionsType SuspensionsType
        {
            get => base.SuspensionsType;

            set
            {
                if (value == SuspensionsType.Unknown)
                {
                    value = SuspensionsType.Stock;
                }

                base.SuspensionsType = value;

                if (!IsDMC12)
                {
                    return;
                }
                else
                {
                    Vehicle.Mods[VehicleModType.Hydraulics].Index = (int)value;
                }
            }
        }

        public new WheelType Wheel
        {
            get => base.Wheel;

            set
            {
                base.Wheel = value;

                //Vehicle.Mods.WheelType = VehicleWheelType.Track;
                Vehicle.Mods.WheelType = VehicleWheelType.Track;
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
                }
            }
        }

        public new ModState Bulova
        {
            get => base.Bulova;

            set
            {
                base.Bulova = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.RightFender].Index = (int)value;
                }
            }
        }

        public new ModState Speedo
        {
            get => base.Speedo;

            set
            {
                base.Speedo = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Dashboard].Index = (int)value;
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
                {
                    Vehicle.Mods[VehicleModType.FrontBumper].Index = (int)value;
                }
            }
        }

        public new ModState GlowingEmitter
        {
            get => base.GlowingEmitter;

            set
            {
                base.GlowingEmitter = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Frame].Index = (int)value;
                }
            }
        }

        public new ModState GlowingReactor
        {
            get => base.GlowingReactor;

            set
            {
                base.GlowingReactor = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Fender].Index = (int)value;
                }
            }
        }

        public new ModState HoverUnderbody
        {
            get => base.HoverUnderbody;

            set
            {
                base.HoverUnderbody = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.DialDesign].Index = (int)value;
                }
            }
        }

        public new ReactorType Reactor
        {
            get => base.Reactor;

            set
            {
                base.Reactor = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Plaques].Index = (int)value;
                }
            }
        }

        public new PlateType Plate
        {
            get => base.Plate;

            set
            {
                base.Plate = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Ornaments].Index = (int)value;
                }
            }
        }

        public new ExhaustType Exhaust
        {
            get => base.Exhaust;

            set
            {
                base.Exhaust = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Exhaust].Index = (int)value;
                }
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
                {
                    return;
                }

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
                {
                    Vehicle.Mods[VehicleModType.Hood].Index = (int)value;
                }
            }
        }

        public void ModCheck()
        {
            if (IsDMC12 && !WasReloaded)
            {
                // The following checks are to stop Los Santos Customs mods on DeLoreans
                SuspensionsType suspensionsType = (SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index;
                WormholeType wormholeType = (WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index;
                ReactorType reactorType = (ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index;
                PlateType plateType = (PlateType)Vehicle.Mods[VehicleModType.Ornaments].Index;
                ExhaustType exhaustType = (ExhaustType)Vehicle.Mods[VehicleModType.Exhaust].Index;
                HoodType hoodType = (HoodType)Vehicle.Mods[VehicleModType.Hood].Index;
                ModState components1 = (ModState)Vehicle.Mods[VehicleModType.Spoilers].Index;
                ModState components2 = (ModState)Vehicle.Mods[VehicleModType.ColumnShifterLevers].Index;
                ModState components3 = (ModState)Vehicle.Mods[VehicleModType.SideSkirt].Index;
                ModState components4 = (ModState)Vehicle.Mods[VehicleModType.SteeringWheels].Index;
                ModState hoodbox1 = (ModState)Vehicle.Mods[VehicleModType.Livery].Index;
                ModState hoodbox2 = (ModState)Vehicle.Mods[VehicleModType.Aerials].Index;
                ModState modState = (ModState)Vehicle.Mods[VehicleModType.RightFender].Index;

                if (wormholeType != WormholeType)
                {
                    WormholeType temp = WormholeType;
                    WormholeType = temp;
                }

                if (reactorType != Reactor)
                {
                    ReactorType temp = Reactor;
                    Reactor = temp;
                }

                if (plateType != Plate)
                {
                    PlateType temp = Plate;
                    Plate = temp;
                }

                if (exhaustType != Exhaust)
                {
                    ExhaustType temp = Exhaust;
                    Exhaust = temp;
                }

                if (hoodType != Hood)
                {
                    HoodType temp = Hood;
                    Hood = temp;
                }

                if (components1 != Components || components2 != Components || components3 != Components || components4 != Components)
                {
                    ModState temp = Components;
                    Components = temp;
                }

                if (modState != Bulova)
                {
                    ModState temp = Bulova;
                    Bulova = temp;
                }

                if (hoodbox1 != Hoodbox || hoodbox2 != Hoodbox)
                {
                    ModState temp = Hoodbox;
                    Hoodbox = temp;
                }

                modState = (ModState)Vehicle.Mods[VehicleModType.Dashboard].Index;

                if (modState != Speedo)
                {
                    ModState temp = Speedo;
                    Speedo = temp;
                }

                modState = (ModState)Vehicle.Mods[VehicleModType.FrontBumper].Index;

                if (modState != OffCoils)
                {
                    ModState temp = OffCoils;
                    OffCoils = temp;
                }

                modState = (ModState)Vehicle.Mods[VehicleModType.Frame].Index;

                if (modState != GlowingEmitter)
                {
                    ModState temp = GlowingEmitter;
                    GlowingEmitter = temp;
                }

                modState = (ModState)Vehicle.Mods[VehicleModType.Fender].Index;

                if (modState != GlowingReactor)
                {
                    ModState temp = GlowingReactor;
                    GlowingReactor = temp;
                }

                modState = (ModState)Vehicle.Mods[VehicleModType.DialDesign].Index;

                if (modState != HoverUnderbody)
                {
                    ModState temp = HoverUnderbody;
                    HoverUnderbody = temp;
                }

                if (Components == ModState.On)
                {
                    if ((Vehicle.Mods[VehicleModType.Roof].Index == 1 && Hook != HookState.Off) || (Vehicle.Mods[VehicleModType.ArchCover].Index == 0 && Hook != HookState.OnDoor)
                        || (Vehicle.Mods[VehicleModType.ArchCover].Index == 1 && Hook != HookState.On))
                    {
                        HookState temp = Hook;
                        Hook = temp;
                    }
                    else if (Vehicle.Mods[VehicleModType.ArchCover].Index == -1 && Vehicle.Mods[VehicleModType.Roof].Index == 0 && Hook != HookState.Removed)
                    {
                        Hook = HookState.Removed;
                    }
                }
                else
                {
                    if (Vehicle.Mods[VehicleModType.Roof].Index != -1 || Vehicle.Mods[VehicleModType.ArchCover].Index != -1 || Vehicle.Mods[VehicleModType.Grille].Index != -1)
                    {
                        Vehicle.Mods[VehicleModType.Roof].Index = -1;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;
                        Vehicle.Mods[VehicleModType.ArchCover].Index = -1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;
                        Vehicle.Mods[VehicleModType.Grille].Index = -1;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                    }
                    if (Hook != HookState.Unknown)
                    {
                        Hook = HookState.Unknown;
                    }
                }

                if (suspensionsType != SuspensionsType)
                {
                    SuspensionsType temp = SuspensionsType;
                    SuspensionsType = temp;
                }
            }
        }
    }
}
