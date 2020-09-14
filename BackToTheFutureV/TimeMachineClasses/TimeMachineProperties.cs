using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public PropertiesHandler(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;            
        }
    }
}
