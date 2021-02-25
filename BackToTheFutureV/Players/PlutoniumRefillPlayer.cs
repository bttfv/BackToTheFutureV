using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using FusionLibrary;
using KlangRageAudioLibrary;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    public class PlutoniumRefillPlayer : Player
    {
        private AnimateProp plutoniumCap;

        private AudioPlayer refuel;

        public PlutoniumRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            refuel = Sounds.AudioEngine.Create("bttf1/refuel.wav", Presets.Exterior);
            refuel.SourceBone = "bttf_reactorcap";

            plutoniumCap = new AnimateProp(Vehicle, ModelHandler.BTTFReactorCap, "bttf_reactorcap");
            plutoniumCap[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].Setup(true, false, -90, 0, 1, 120, 1);
            plutoniumCap[AnimationType.Offset][AnimationStep.Second][Coordinate.Z].Setup(true, true, 0, 0.06f, 1, 0.08f, 1);
            plutoniumCap.OnAnimCompleted += PlutoniumCap_OnAnimCompleted;
            plutoniumCap.SpawnProp();
        }

        private void PlutoniumCap_OnAnimCompleted(AnimationStep animationStep)
        {
            if (!Properties.IsRefueling)
            {
                if (animationStep == AnimationStep.First)
                    plutoniumCap.Play(AnimationStep.Second);
                else
                    OnPlayerCompleted?.Invoke();
            }
            else
            {
                if (animationStep == AnimationStep.Second)
                    plutoniumCap.Play();
                else
                    OnPlayerCompleted?.Invoke();
            }
        }

        public override void Play()
        {
            refuel.Play();

            if (!Properties.IsRefueling)
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
            refuel.Dispose();
        }
    }
}
