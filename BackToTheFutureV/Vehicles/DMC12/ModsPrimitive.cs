using System;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class ModsPrimitive
    {
        public bool IsDMC12 { get; protected set; } = false;

        public WormholeType WormholeType { get; set; } = WormholeType.DMC12;
        public SuspensionsType SuspensionsType { get; set; } = SuspensionsType.Stock;
        public WheelType Wheel { get; set; } = WheelType.Stock;
        public ModState Components { get; set; } = ModState.Off;
        public ModState OffCoils { get; set; } = ModState.Off;
        public ModState GlowingEmitter { get; set; } = ModState.Off;
        public ModState GlowingReactor { get; set; } = ModState.Off;
        public ModState HoverUnderbody { get; set; } = ModState.Off;
        public ReactorType Reactor { get; set; } = ReactorType.None;
        public PlateType Plate { get; set; } = PlateType.Empty;
        public ExhaustType Exhaust { get; set; } = ExhaustType.Stock;
        public ModState Hoodbox { get; set; } = ModState.Off;
        public HookState Hook { get; set; } = HookState.Off;
        public HoodType Hood { get; set; } = HoodType.Stock;

        public ModsPrimitive Clone()
        {
            ModsPrimitive ret = new ModsPrimitive
            {
                IsDMC12 = IsDMC12,
                WormholeType = WormholeType,
                SuspensionsType = SuspensionsType,
                Wheel = Wheel,
                Components = Components,
                OffCoils = OffCoils,
                GlowingEmitter = GlowingEmitter,
                GlowingReactor = GlowingReactor,
                HoverUnderbody = HoverUnderbody,
                Reactor = Reactor,
                Plate = Plate,
                Exhaust = Exhaust,
                Hoodbox = Hoodbox,
                Hook = Hook,
                Hood = Hood
            };

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            ModsHandler ret = timeMachine.Mods;

            timeMachine.Vehicle.Mods.InstallModKit();

            ret.IsDMC12 = IsDMC12;
            ret.WormholeType = WormholeType;
            ret.SuspensionsType = SuspensionsType;
            ret.Wheel = Wheel;
            ret.Components = Components;
            ret.OffCoils = OffCoils;
            ret.GlowingEmitter = GlowingEmitter;
            ret.GlowingReactor = GlowingReactor;
            ret.HoverUnderbody = HoverUnderbody;
            ret.Reactor = Reactor;
            ret.Plate = Plate;
            ret.Exhaust = Exhaust;
            ret.Hoodbox = Hoodbox;
            ret.Hook = Hook;
            ret.Hood = Hood;
        }

        public void ApplyToWayback(TimeMachine timeMachine)
        {
            ModsHandler ret = timeMachine.Mods;

            if (ret.WormholeType != WormholeType)
                ret.WormholeType = WormholeType;

            if (ret.SuspensionsType != SuspensionsType)
                ret.SuspensionsType = SuspensionsType;

            if (ret.HoverUnderbody != HoverUnderbody)
                ret.HoverUnderbody = HoverUnderbody;

            if (ret.Wheel != Wheel)
                ret.Wheel = Wheel;

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