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
        private TimeMachine TimeMachine;

        public TimeMachineMods(TimeMachine timeMachine, WormholeType wormholeType) : base(timeMachine.Vehicle)
        {
            TimeMachine = timeMachine;

            WormholeType = wormholeType;

            ApplyMods();
        }

        private void ApplyMods()
        {
            Vehicle.Mods.InstallModKit();

            Vehicle.ToggleExtra(10, WormholeType == WormholeType.DMC12);

            if (WormholeType != WormholeType.DMC12)
            {
                Exterior = ModState.On;
                Interior = ModState.On;
                SteeringWheelsButtons = ModState.On;
                Vents = ModState.On;
                OffCoils = ModState.On;
                Hook = HookState.Off;
                Hoodbox = ModState.Off;
                HoverUnderbody = ModState.Off;
                DamagedBumper = ModState.Off;

                Exhaust = ExhaustType.BTTF;
                Reactor = ReactorType.MrFusion;
                Plate = PlateType.BTTF2;
            }

            switch (WormholeType)
            {
                case WormholeType.BTTF1:
                    Plate = PlateType.Outatime;
                    Reactor = ReactorType.Nuclear;
                    break;
                case WormholeType.BTTF2:
                    Exhaust = ExhaustType.None;
                    HoverUnderbody = ModState.On;
                    break;
                case WormholeType.BTTF3:
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
                if (value == WheelType.RailroadInvisible)
                {
                    TimeMachine.Props?.RRWheels?.ForEach(x => x?.SpawnProp());
                }
                else
                {
                    TimeMachine.Props?.RRWheels?.ForEach(x => x?.DeleteProp());

                    Utils.SetTiresBurst(Vehicle, false);
                }

                base.Wheel = value;
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
    }
}
