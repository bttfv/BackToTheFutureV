using System.Collections.Generic;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA;

using System.Windows.Forms;
using System.Linq;
using GTA.Native;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class RailRoadWheelsHandler : Handler
    {
        public bool AreRailRoadWheelsOn => _rrWheels.TrueForAll(x => x.IsSpawned);

        public RailRoadWheelsHandler(TimeCircuits circuits) : base(circuits)
        {
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lf"));
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lr"));
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rf"));
            _rrWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rr"));            
        }

        private List<AnimateProp> _rrWheels = new List<AnimateProp>();

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public override void Process()
        {
            if (DeloreanType == DeloreanType.BTTF3RR && !AreRailRoadWheelsOn)
                _rrWheels.ForEach(x => x.SpawnProp());
        }

        public override void Stop()
        {
            _rrWheels.ForEach(x => x.DeleteProp());
            _rrWheels.Clear();
        }
    }
}
