using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
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
            plutoniumCap = new AnimateProp(Vehicle, ModelHandler.BTTFReactorCap, "bttf_reactorcap");
            plutoniumCap[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].Setup(false, true, false, -90, 0, 1, 120, 1);
            plutoniumCap[AnimationType.Offset][AnimationStep.Second][Coordinate.Z].Setup(false, true, true, 0, 0.06f, 1, 0.08f, 1);
            plutoniumCap.OnAnimCompleted += PlutoniumCap_OnAnimCompleted;
            plutoniumCap.SpawnProp();            
        }

        private void PlutoniumCap_OnAnimCompleted(AnimationStep animationStep)
        {
            if (open)
            {
                if (animationStep == AnimationStep.First)
                    plutoniumCap.Play(AnimationStep.Second);
            }
            else
            {
                if (animationStep == AnimationStep.Second)
                    plutoniumCap.Play();
            }
        }

        public override void Play()
        {
            open = !open;

            if (open)
                plutoniumCap.Play();
            else
                plutoniumCap.Play(AnimationStep.Second);
        }

        public override void Process()
        {

        }

        public override void Stop()
        {
            
        }

        public override void Dispose()
        {
            plutoniumCap.Dispose();
        }
    }
}
