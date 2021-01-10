using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using System;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses
{
    [Serializable]
    public class PropertiesHandler : BaseProperties
    {
        private TimeMachine TimeMachine { get; }
        private Vehicle Vehicle => TimeMachine.Vehicle;
        private TimeMachineMods Mods => TimeMachine.Mods;

        public string LowerWormholeType => Mods.WormholeType.ToString().ToLower();
        public bool IsStockWheel => Mods.Wheel == WheelType.Stock || Mods.Wheel == WheelType.StockInvisible;
        public bool FullDamaged => Mods.Wheel == WheelType.Stock && Utils.IsAllTiresBurst(Vehicle) && AreFlyingCircuitsBroken && AreTimeCircuitsBroken;

        public PropertiesHandler(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;

            GUID = Guid.NewGuid();
        }

        public new MissionType MissionType 
        { 
            get
            {
                return base.MissionType;
            }
            set
            {
                base.MissionType = value;
                TimeMachine?.Events?.OnMissionChange?.Invoke();
            }
        }
    }
}
