using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class CompassHandler : Handler
    {
        private AnimateProp _compass;        

        public CompassHandler(TimeCircuits circuits) : base(circuits)
        {
            _compass = new AnimateProp(Vehicle, ModelHandler.Compass, "bttf_compass");
        }

        public override void Dispose()
        {            
            _compass?.Dispose();        
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public override void Process()
        {
            _compass?.SpawnProp(Vector3.Zero, new Vector3(0, 0, Vehicle.Heading), false);
            
            if (_compass.Prop.IsVisible != Vehicle.IsVisible)
                _compass.Prop.IsVisible = Vehicle.IsVisible;
        }

        public override void Stop()
        {
            _compass?.DeleteProp();
        }
    }
}
