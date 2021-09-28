using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class DMC12Mods : ModsPrimitive
    {
        protected Vehicle Vehicle { get; }

        public DMC12Mods(Vehicle vehicle)
        {
            Vehicle = vehicle;

            IsDMC12 = Vehicle.Model == ModelHandler.DMC12;

            Vehicle.Mods.InstallModKit();

            if (IsDMC12)
            {
                Function.Call(Hash._SET_HYDRAULIC_RAISED, Vehicle, false);

                Vehicle.ToggleExtra(10, true);

                Vehicle.Mods.PrimaryColor = VehicleColor.BrushedAluminium;
                Vehicle.Mods.TrimColor = VehicleColor.PureWhite;

                Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);

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
            get
            {
                return base.SuspensionsType;
            }

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

                switch (value)
                {
                    case SuspensionsType.Stock:
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelLeftFront, 0f);
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelRightFront, 0f);
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelLeftRear, 0f);
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelRightRear, 0f);

                        Function.Call(Hash._SET_CAMBERED_WHEELS_DISABLED, Vehicle, false);
                        break;
                    default:
                        Function.Call(Hash._SET_CAMBERED_WHEELS_DISABLED, Vehicle, true);

                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelLeftFront, 0f);
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelRightFront, 0f);
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelLeftRear, 0f);
                        Vehicle.LiftUpWheel(VehicleWheelBoneId.WheelRightRear, 0f);
                        break;
                }
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
                base.Wheel = value;

                Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, Vehicle, 12);
                Vehicle.Mods[VehicleModType.FrontWheel].Index = (int)value;
            }
        }

        public new ModState Components
        {
            get
            {
                return base.Components;
            }

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
            get
            {
                return base.OffCoils;
            }

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
            get
            {
                return base.GlowingEmitter;
            }

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
            get
            {
                return base.GlowingReactor;
            }

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
            get
            {
                return base.HoverUnderbody;
            }

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
            get
            {
                return base.Reactor;
            }

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
            get
            {
                return base.Plate;
            }

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
            get
            {
                return base.Exhaust;
            }

            set
            {
                base.Exhaust = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Windows].Index = (int)value;
                }
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

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Livery].Index = (int)value;
                    Vehicle.Mods[VehicleModType.Aerials].Index = (int)value;
                }
            }
        }

        public new HookState Hook
        {
            get
            {
                return base.Hook;
            }

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
            get
            {
                return base.Hood;
            }

            set
            {
                base.Hood = value;

                if (IsDMC12)
                {
                    Vehicle.Mods[VehicleModType.Hood].Index = (int)value;
                }
            }
        }
    }
}
