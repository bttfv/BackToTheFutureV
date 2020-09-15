using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.TimeMachineClasses
{
    [Serializable]
    public class TimeMachineMods : DMC12Mods
    {
        protected TimeMachine TimeMachine { get; }
        public TimeMachineMods(TimeMachine timeMachine, WormholeType wormholeType) : base(timeMachine.Vehicle)
        {
            TimeMachine = timeMachine;

            if (IsDMC12)
                Vehicle.ToggleExtra(10, false);

            WormholeType = wormholeType;

            Exterior = ModState.On;
            Interior = ModState.On;
            SteeringWheelsButtons = ModState.On;
            Vents = ModState.On;
            OffCoils = ModState.On;
            
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

        public new WheelType Wheel
        {
            get => base.Wheel;
            set
            {
                base.Wheel = value;

                if (value == WheelType.RailroadInvisible)
                {
                    TimeMachine.Props?.RRWheels?.ForEach(x => x?.SpawnProp());

                    if (HoverUnderbody == ModState.On)
                        HoverUnderbody = ModState.Off;
                }
                else
                {
                    TimeMachine.Props?.RRWheels?.ForEach(x => x?.DeleteProp());

                    Utils.SetTiresBurst(Vehicle, false);
                }                
            }
        }

        public new ModState HoverUnderbody
        {
            get => base.HoverUnderbody;
            set 
            {
                base.HoverUnderbody = value;

                TimeMachine.Events?.OnHoverUnderbodyToggle?.Invoke();

                if (IsDMC12)
                {                   
                    if (value == ModState.On)
                    {
                        SuspensionsType = SuspensionsType.Stock;
                        Exhaust = ExhaustType.None;                        
                    }

                    TimeMachine.DMC12.SetStockSuspensions?.Invoke(value == ModState.Off);
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

                DamagedBumper = value;

                if (value == ModState.On)
                {
                    if (TimeMachine.Properties == null || !TimeMachine.Properties.AreTimeCircuitsBroken)
                        return;

                    TimeMachine.Events?.SetTimeCircuitsBroken?.Invoke(false);
                    WormholeType = WormholeType.BTTF3;
                }                    
            }
        }
    }
}
