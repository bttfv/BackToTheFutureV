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
        public ModState Bulova { get; set; } = ModState.Off;
        public ModState Speedo { get; set; } = ModState.Off;
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
                Bulova = Bulova,
                Speedo = Speedo,
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
    }
}
