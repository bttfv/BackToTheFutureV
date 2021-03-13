using BackToTheFutureV.TimeMachineClasses;
using System;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.Vehicles
{
    [Serializable]
    internal class BaseMods
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
        public ModState BulovaClock { get; set; } = ModState.Off;

        public BaseMods()
        {

        }

        public BaseMods Clone()
        {
            BaseMods ret = new BaseMods
            {
                IsDMC12 = IsDMC12,
                WormholeType = WormholeType,
                SuspensionsType = SuspensionsType,
                Wheel = Wheel,
                Exterior = Exterior,
                Interior = Interior,
                OffCoils = OffCoils,
                GlowingEmitter = GlowingEmitter,
                GlowingReactor = GlowingReactor,
                DamagedBumper = DamagedBumper,
                HoverUnderbody = HoverUnderbody,
                SteeringWheelsButtons = SteeringWheelsButtons,
                Vents = Vents,
                Seats = Seats,
                Reactor = Reactor,
                Plate = Plate,
                Exhaust = Exhaust,
                Hoodbox = Hoodbox,
                Hook = Hook,
                Hood = Hood,
                BulovaClock = BulovaClock
            };

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            TimeMachineMods ret = timeMachine.Mods;

            timeMachine.Vehicle.Mods.InstallModKit();

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
            ret.BulovaClock = BulovaClock;
        }

        public void ApplyToWayback(TimeMachine timeMachine)
        {
            TimeMachineMods ret = timeMachine.Mods;

            if (ret.WormholeType != WormholeType)
                ret.WormholeType = WormholeType;

            if (Wheel != WheelType.RailroadInvisible && Wheel != WheelType.RedInvisible && Wheel != WheelType.StockInvisible)
                if (ret.SuspensionsType != SuspensionsType)
                    ret.SuspensionsType = SuspensionsType;

            if (Wheel != WheelType.RailroadInvisible && Wheel != WheelType.RedInvisible && Wheel != WheelType.StockInvisible)
                if (ret.Wheel != Wheel)
                    ret.Wheel = Wheel;

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