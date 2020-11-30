using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.UI;
using BackToTheFutureV.TimeMachineClasses;
using System.Threading.Tasks;
using FusionLibrary;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    public class PlutoniumRefillPlayer : Player
    {
        private AnimateProp plutoniumCap;

        private bool open;

        public PlutoniumRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            plutoniumCap = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFReactorCap), "bttf_reactorcap");            
            plutoniumCap.setRotationSettings(Coordinate.Z, false, false, -90, 0, 1, 120, 1, true);
            plutoniumCap.setOffsetSettings(Coordinate.Z, false, true, 0, 0.06f, 1, 0.08f, 1, true);
            plutoniumCap.AnimationStopped += AnimationStopped;
            plutoniumCap.SpawnProp();            
        }

        public override void Play()
        {
            open = !open;

            if (open)
                plutoniumCap.setRotationUpdate(Coordinate.Z, true);
            else
                plutoniumCap.setOffsetUpdate(Coordinate.Z, true);
        }

        public void AnimationStopped(AnimateProp animateProp, Coordinate coordinate, CoordinateSetting coordinateSetting, bool IsRotation)
        {
            if (animateProp == plutoniumCap)
            {
                if (open)
                {
                    if (IsRotation && coordinate == Coordinate.Z)
                        plutoniumCap.setOffsetUpdate(Coordinate.Z, true);
                }
                else
                {
                    if (!IsRotation && coordinate == Coordinate.Z)
                        plutoniumCap.setRotationUpdate(Coordinate.Z, true);
                }
            }
        }

        public override void Process()
        {

        }

        public override void Stop()
        {
            
        }

        public override void Dispose()
        {
            plutoniumCap?.Dispose();
        }
    }
}
