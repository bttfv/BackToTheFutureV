using System.Windows.Forms;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA.Math;

namespace BackToTheFutureV.Delorean.Handlers
{
    class CoilsIndicatorHandler : Handler
    {
        private readonly AnimateProp _leftIndicatorProp;
        private readonly AnimateProp _rightIndicatorProp;

        public CoilsIndicatorHandler(TimeCircuits circuits) : base(circuits)
        {
            _leftIndicatorProp = new AnimateProp(Vehicle, ModelHandler.CoilsIndicatorLeft,
                Vector3.Zero, Vector3.Zero);

            _rightIndicatorProp = new AnimateProp(Vehicle, ModelHandler.CoilsIndicatorRight,
                Vector3.Zero, Vector3.Zero);
        }

        public override void KeyPress(Keys key)
        {

        }

        public override void Process()
        {
            if (Vehicle.AreLightsOn && Vehicle.IsEngineRunning)
            {
                if (!_leftIndicatorProp.IsSpawned && !Vehicle.IsLeftHeadLightBroken)
                    _leftIndicatorProp.SpawnProp();

                if (!_rightIndicatorProp.IsSpawned && !Vehicle.IsRightHeadLightBroken)
                    _rightIndicatorProp.SpawnProp();
            }
            else
            {
                _leftIndicatorProp.DeleteProp();
                _rightIndicatorProp.DeleteProp();

                return;
            }

            if (Vehicle.IsLeftHeadLightBroken && _leftIndicatorProp.IsSpawned)
                _leftIndicatorProp.DeleteProp();

            if (Vehicle.IsRightHeadLightBroken && _rightIndicatorProp.IsSpawned)
                _rightIndicatorProp.DeleteProp();
        }

        public override void Stop()
        {
            _leftIndicatorProp.DeleteProp();
            _rightIndicatorProp.DeleteProp();
        }

        public override void Dispose() => Stop();
    }
}