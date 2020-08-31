using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Memory;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackToTheFutureV.Delorean
{
    public enum DeloreanType
    {
        Unknown = -1,
        DMC12,
        BTTF1,
        BTTF2,
        BTTF3
    }

    public enum ModState
    {
        Off = -1,
        On = 0
    }

    public enum HookState
    {
        Off,
        OnDoor,
        On,
        Removed,
        Unknown
    }

    public enum PlateType
    {
        Empty = -1,
        Outatime = 0,
        NOTIME = 2,
        TIMELESS = 3,
        TIMELESS2 = 4,
        DMCFACTORY = 5,
        DMCFACTORY2 = 6,
        BTTF2 = 1
    }

    public enum ReactorType
    {
        None = -1,
        MrFusion = 0,
        Nuclear = 1
    }

    public enum ExhaustType
    {
        Stock = -1,
        BTTF = 0,
        None = 1
    }

    public enum WheelType
    {
        Stock = -1,
        StockInvisible = 0,
        RailroadInvisible = 1,
        RedInvisible = 2,
        Red = 3
    }

    public enum SuspensionsType
    {
        Stock = 0,
        LiftFrontLowerRear = 1,        
        LiftFront = 2,
        LiftRear = 3,
        LiftFrontAndRear = 4,
        LowerFrontLiftRear = 5,
        LowerFront = 6,
        LowerRear = 7,
        LowerFrontAndRear = 8
    }

    public class DeloreanMods
    {
        private Vehicle Vehicle;

        private DeloreanTimeMachine TimeMachine => DeloreanHandler.GetTimeMachineFromVehicle(Vehicle);
        
        private List<AnimateProp> _rrWheels = new List<AnimateProp>();

        private SuspensionsType _suspensionsType = SuspensionsType.Stock;

        public void Save(string name) => new DeloreanModsCopy(this).Save(name);

        public void Load(string name) => DeloreanModsCopy.Load(name).ApplyTo(TimeMachine);

        public DeloreanType DeloreanType { get => (DeloreanType)Vehicle.Mods[VehicleModType.TrimDesign].Index; 
            set {
                Vehicle.Mods[VehicleModType.TrimDesign].Index = (int)value;

                if (value != DeloreanType.DMC12)
                {
                    TimeMachine?.Circuits.GetHandler<SparksHandler>().LoadRes();
                    TimeMachine?.Circuits.GetHandler<ReentryHandler>().LoadRes();
                    TimeMachine?.Circuits.GetHandler<TimeTravelHandler>().LoadRes();
                }
            }
        }

        public DeloreanMods(Vehicle vehicle, bool isNew)
        {
            Vehicle = vehicle;

            Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, vehicle, 0f);

            vehicle.Mods.PrimaryColor = VehicleColor.BrushedAluminium;
            vehicle.Mods.TrimColor = VehicleColor.PureWhite;
            vehicle.DirtLevel = 0;

            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lf"));
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lr"));
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rf"));
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rr"));

            if (isNew)
            {
                Seats = ModState.On;

                ApplyMods();
            }
            else
            {
                if (Wheel == WheelType.RailroadInvisible)
                    Wheel = Wheel;
            }
        }

        public void RemoveRrWheels()
        {
            _rrWheels?.ForEach(x => x?.DeleteProp());
        }

        private void RestoreStock()
        {
            DeloreanType = DeloreanType.DMC12;

            Exterior = ModState.Off;
            Interior = ModState.Off;
            SteeringWheelsButtons = ModState.Off;
            Vents = ModState.Off;
            OffCoils = ModState.Off;
            DamagedBumper = ModState.Off;

            Exhaust = ExhaustType.Stock;
            Reactor = ReactorType.None;
            Plate = PlateType.Empty;

            HoverUnderbody = ModState.Off;
            Hoodbox = ModState.Off;

            SuspensionsType = SuspensionsType.Stock;

            Wheel = WheelType.Stock;

            Vehicle.ToggleExtra(10, true);
        }

        public void Convert(DeloreanType to)
        {
            RestoreStock();

            if (to == DeloreanType.DMC12)
                return;

            DeloreanType = to;

            ApplyMods();
        }

        private void ApplyMods()
        {
            Vehicle.ToggleExtra(10, DeloreanType == DeloreanType.DMC12);

            if (DeloreanType != DeloreanType.DMC12)
            {
                Exterior = ModState.On;
                Interior = ModState.On;
                SteeringWheelsButtons = ModState.On;
                Vents = ModState.On;
                OffCoils = ModState.On;
                Hook = HookState.Off;
                HoverUnderbody = ModState.Off;
                DamagedBumper = ModState.Off;

                Exhaust = ExhaustType.BTTF;
                Reactor = ReactorType.MrFusion;
                Plate = PlateType.BTTF2;
            }

            switch (DeloreanType)
            {
                case DeloreanType.BTTF1:
                    Plate = PlateType.Outatime;
                    Reactor = ReactorType.Nuclear;
                    break;
                case DeloreanType.BTTF2:
                    Exhaust = ExhaustType.None;
                    HoverUnderbody = ModState.On;
                    break;
                case DeloreanType.BTTF3:                    
                    Hoodbox = ModState.On;
                    Wheel = WheelType.Red;
                    SuspensionsType = SuspensionsType.LiftFrontLowerRear;
                    break;
            }
        }

        public SuspensionsType SuspensionsType
        {
            get => _suspensionsType;

            set
            {
                switch (value)
                {
                    case SuspensionsType.Stock:                        
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, 0f);

                        Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, false);
                        Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, false);

                        Function.Call(Hash.MODIFY_VEHICLE_TOP_SPEED, Vehicle, 0f);
                        break;
                    default:
                        HoverUnderbody = ModState.Off;

                        Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, true);
                        Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, true);

                        Function.Call(Hash.MODIFY_VEHICLE_TOP_SPEED, Vehicle, 40f);

                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, 0f);

                        break;
                }

                _suspensionsType = value;
            }
        }

        public WheelType Wheel
        {
            get => (WheelType) Vehicle.Mods[VehicleModType.FrontWheel].Index;

            set
            {
                if (value == WheelType.RailroadInvisible)
                {                    
                    _rrWheels.ForEach(x => x.SpawnProp());
                } 
                else
                {
                    if (_rrWheels[0].IsSpawned)
                        _rrWheels.ForEach(x => x.DeleteProp());

                    Utils.SetTiresBurst(Vehicle, false);
                }

                Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, Vehicle, 12);
                Vehicle.Mods[VehicleModType.FrontWheel].Index = (int)value;

                if (DeloreanType == DeloreanType.BTTF3)
                    TimeMachine?.Circuits.GetHandler<SparksHandler>().LoadRes();
            }
        }

        public ModState Exterior
        {
            get => (ModState)Vehicle.Mods[VehicleModType.Spoilers].Index;            
            set => Vehicle.Mods[VehicleModType.Spoilers].Index = (int)value;            
        }

        public ModState Interior
        {
            get => (ModState)Vehicle.Mods[VehicleModType.SideSkirt].Index;
            set => Vehicle.Mods[VehicleModType.SideSkirt].Index = (int)value;
        }

        public ModState OffCoils
        {
            get => (ModState)Vehicle.Mods[VehicleModType.FrontBumper].Index;
            set => Vehicle.Mods[VehicleModType.FrontBumper].Index = (int)value;
        }

        public ModState GlowingEmitter
        {
            get => (ModState)Vehicle.Mods[VehicleModType.Frame].Index;
            set => Vehicle.Mods[VehicleModType.Frame].Index = (int)value;
        }

        public ModState GlowingReactor
        {
            get => (ModState)Vehicle.Mods[VehicleModType.Fender].Index;
            set => Vehicle.Mods[VehicleModType.Fender].Index = (int)value;
        }

        public ModState DamagedBumper
        {
            get => (ModState)Vehicle.Mods[VehicleModType.Aerials].Index;
            set => Vehicle.Mods[VehicleModType.Aerials].Index = (int)value;
        }

        public ModState HoverUnderbody
        {
            get => (ModState)Vehicle.Mods[VehicleModType.DialDesign].Index;
            set
            {
                Vehicle.Mods[VehicleModType.DialDesign].Index = (int)value;

                if (value == ModState.On)
                {
                    if (Exhaust != ExhaustType.None)
                        Exhaust = ExhaustType.None;

                    if (SuspensionsType != SuspensionsType.Stock)
                        SuspensionsType = SuspensionsType.Stock;

                    TimeMachine?.Circuits.FlyingHandler.LoadWheelAnim();
                }                    
                else
                    TimeMachine?.Circuits.FlyingHandler.DeleteWheelAnim();
            }
        }

        public ModState SteeringWheelsButtons
        {
            get => (ModState)Vehicle.Mods[VehicleModType.SteeringWheels].Index;
            set => Vehicle.Mods[VehicleModType.SteeringWheels].Index = (int)value;
        }

        public ModState Vents
        {
            get => (ModState)Vehicle.Mods[VehicleModType.ColumnShifterLevers].Index;
            set => Vehicle.Mods[VehicleModType.ColumnShifterLevers].Index = (int)value;
        }

        public ModState Seats
        {
            get => (ModState)Vehicle.Mods[VehicleModType.VanityPlates].Index;
            set => Vehicle.Mods[VehicleModType.VanityPlates].Index = (int)value;
        }

        public ReactorType Reactor
        {
            get => (ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index;

            set
            {
                Vehicle.Mods[VehicleModType.Plaques].Index = (int)value;

                if (DeloreanType == DeloreanType.DMC12)
                    return;

                TimeMachine?.Circuits.GetHandler<FuelHandler>().LoadRes();
            }
        }

        public PlateType Plate
        {
            get => (PlateType)Vehicle.Mods[VehicleModType.Ornaments].Index;
            set => Vehicle.Mods[VehicleModType.Ornaments].Index = (int)value;
        }

        public ExhaustType Exhaust
        {
            get => (ExhaustType)Vehicle.Mods[VehicleModType.Windows].Index;
            set
            {
                Vehicle.Mods[VehicleModType.Windows].Index = (int)value;

                if (DeloreanType == DeloreanType.DMC12)
                    return;

                if (value != ExhaustType.None && HoverUnderbody == ModState.On)
                    HoverUnderbody = ModState.Off;
            }
        }

        public ModState Hoodbox
        {
            get => (ModState)Vehicle.Mods[VehicleModType.Livery].Index;

            set
            {
                Vehicle.Mods[VehicleModType.Livery].Index = (int)value;
                DamagedBumper = value;

                if (value == ModState.On)
                    TimeMachine?.Circuits.SetTimeCircuitsBroken(false);
            }
        }

        public HookState Hook
        {
            get
            {
                if (Vehicle.Mods[VehicleModType.Roof].Index != 0)
                    return HookState.Off;

                if (Vehicle.Mods[VehicleModType.ArchCover].Index == 1 && Vehicle.Mods[VehicleModType.Grille].Index == 1)
                    return HookState.On;

                if (Vehicle.Mods[VehicleModType.ArchCover].Index == 0 && Vehicle.Mods[VehicleModType.Grille].Index == 0)
                    return HookState.OnDoor;

                if (Vehicle.Mods[VehicleModType.ArchCover].Index == -1 && Vehicle.Mods[VehicleModType.Grille].Index == 0)
                    return HookState.Removed;

                return HookState.Unknown;
            }
            set
            {
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

    }
}
