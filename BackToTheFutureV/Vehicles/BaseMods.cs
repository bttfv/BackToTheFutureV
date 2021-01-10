using BackToTheFutureV.TimeMachineClasses;
using System;

namespace BackToTheFutureV.Vehicles
{
    public enum WormholeType
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
        BTTF2 = 1,
        NOTIME = 2,
        TIMELESS = 3,
        TIMELESS2 = 4,
        DMCFACTORY = 5,
        DMCFACTORY2 = 6
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
        Unknown = -1,
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

    public enum HoodType 
    {
        Stock = -1,
        H1983 = 0,
        H1981 = 1
    }

    [Serializable]
    public class BaseMods
    {
        public bool IsDMC12 { get; protected set; } = false;
        public WormholeType WormholeType { get; set; } = WormholeType.DMC12;
        public SuspensionsType SuspensionsType { get; set; } = SuspensionsType.Stock;
        public WheelType Wheel { get; set; } = WheelType.Stock;
        public ModState Exterior { get; set; } = ModState.Off;
        public ModState Interior { get; set; } = ModState.Off;
        public ModState OffCoils { get; set; } = ModState.Off;
        public ModState GlowingEmitter { get; set; } = ModState.Off;
        public ModState GlowingReactor { get; set; } = ModState.Off;
        public ModState DamagedBumper { get; set; } = ModState.Off;
        public ModState HoverUnderbody { get; set; } = ModState.Off;
        public ModState SteeringWheelsButtons { get; set; } = ModState.Off;
        public ModState Vents { get; set; } = ModState.Off;
        public ModState Seats { get; set; } = ModState.Off;
        public ReactorType Reactor { get; set; } = ReactorType.None;
        public PlateType Plate { get; set; } = PlateType.Empty;
        public ExhaustType Exhaust { get; set; } = ExhaustType.Stock;
        public ModState Hoodbox { get; set; } = ModState.Off;
        public HookState Hook { get; set; } = HookState.Off;
        public HoodType Hood { get; set; } = HoodType.Stock;

        public BaseMods()
        {

        }

        public BaseMods Clone()
        {
            BaseMods ret = new BaseMods();

            ret.IsDMC12 = IsDMC12;
            ret.WormholeType = WormholeType;
            ret.SuspensionsType = SuspensionsType;
            ret.Wheel = Wheel;
            ret.Exterior = Exterior;
            ret.Interior = Interior;
            ret.OffCoils = OffCoils;
            ret.GlowingEmitter = GlowingEmitter;
            ret.GlowingReactor = GlowingReactor;
            ret.DamagedBumper = DamagedBumper;
            ret.HoverUnderbody = HoverUnderbody;
            ret.SteeringWheelsButtons = SteeringWheelsButtons;
            ret.Vents = Vents;
            ret.Seats = Seats;
            ret.Reactor = Reactor;
            ret.Plate = Plate;
            ret.Exhaust = Exhaust;
            ret.Hoodbox = Hoodbox;
            ret.Hook = Hook;
            ret.Hood = Hood;

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            TimeMachineMods ret = timeMachine.Mods;

            ret.IsDMC12 = IsDMC12;
            ret.WormholeType = WormholeType;
            ret.SuspensionsType = SuspensionsType;
            ret.Wheel = Wheel;
            ret.Exterior = Exterior;
            ret.Interior = Interior;
            ret.OffCoils = OffCoils;
            ret.GlowingEmitter = GlowingEmitter;
            ret.GlowingReactor = GlowingReactor;
            ret.DamagedBumper = DamagedBumper;
            ret.HoverUnderbody = HoverUnderbody;
            ret.SteeringWheelsButtons = SteeringWheelsButtons;
            ret.Vents = Vents;
            ret.Seats = Seats;
            ret.Reactor = Reactor;
            ret.Plate = Plate;
            ret.Exhaust = Exhaust;
            ret.Hoodbox = Hoodbox;
            ret.Hook = Hook;
            ret.Hood = Hood;
        }

        public void ApplyToWayback(TimeMachine timeMachine)
        {
            TimeMachineMods ret = timeMachine.Mods;

            if (ret.IsDMC12 != IsDMC12)
                ret.IsDMC12 = IsDMC12;

            if (ret.WormholeType != WormholeType)
                ret.WormholeType = WormholeType;

            if (Wheel != WheelType.RailroadInvisible && Wheel != WheelType.RedInvisible && Wheel != WheelType.StockInvisible)
                if (ret.SuspensionsType != SuspensionsType)
                    ret.SuspensionsType = SuspensionsType;

            if (Wheel != WheelType.RailroadInvisible && Wheel != WheelType.RedInvisible && Wheel != WheelType.StockInvisible)
                if (ret.Wheel != Wheel)
                    ret.Wheel = Wheel;

            if (ret.Exterior != Exterior)
                ret.Exterior = Exterior;

            if (ret.Interior != Interior)
                ret.Interior = Interior;

            if (ret.OffCoils != OffCoils)
                ret.OffCoils = OffCoils;

            if (ret.GlowingEmitter != GlowingEmitter)
                ret.GlowingEmitter = GlowingEmitter;

            if (ret.GlowingReactor != GlowingReactor)
                ret.GlowingReactor = GlowingReactor;

            if (ret.DamagedBumper != DamagedBumper)
                ret.DamagedBumper = DamagedBumper;

            if (Wheel != WheelType.RailroadInvisible && Wheel != WheelType.RedInvisible && Wheel != WheelType.StockInvisible)
                ret.HoverUnderbody = HoverUnderbody;

            if (ret.SteeringWheelsButtons != SteeringWheelsButtons)
                ret.SteeringWheelsButtons = SteeringWheelsButtons;

            if (ret.Vents != Vents)
                ret.Vents = Vents;

            if (ret.Seats != Seats)
                ret.Seats = Seats;

            if (ret.Reactor != Reactor)
                ret.Reactor = Reactor;

            if (ret.Plate != Plate)
                ret.Plate = Plate;

            if (ret.Exhaust != Exhaust)
                ret.Exhaust = Exhaust;

            if (ret.Hoodbox != Hoodbox)
                ret.Hoodbox = Hoodbox;

            if (ret.Hook != Hook)
                ret.Hook = Hook;

            if (ret.Hood != Hood)
                ret.Hood = Hood;
        }
    }
}