using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using FusionLibrary;
using KlangRageAudioLibrary;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    internal class PlutoniumRefillPlayer : Player
    {
        private AnimateProp plutoniumCap;

        private AudioPlayer open;
        private AudioPlayer close;

        public PlutoniumRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            open = Sounds.AudioEngine.Create("bttf1/plutonium_open.wav", Presets.Exterior);
            open.SourceBone = "bttf_reactorcap";

            close = Sounds.AudioEngine.Create("bttf1/plutonium_close.wav", Presets.Exterior);
            close.SourceBone = "bttf_reactorcap";

            plutoniumCap = new AnimateProp(ModelHandler.BTTFReactorCap, Vehicle, "bttf_reactorcap");
            plutoniumCap[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].Setup(true, false, -90, 0, 1, 240, 1);
            plutoniumCap[AnimationType.Offset][AnimationStep.Second][Coordinate.Z].Setup(true, true, 0, 0.05f, 1, 0.16f, 1);
            plutoniumCap[AnimationType.Offset][AnimationStep.Third][Coordinate.Y].Setup(true, true, 0, 0.08f, 1, 0.16f, 1);
            plutoniumCap.OnAnimCompleted += PlutoniumCap_OnAnimCompleted;
            plutoniumCap.SpawnProp();
        }

        private void PlutoniumCap_OnAnimCompleted(AnimationStep animationStep)
        {
            if (!Properties.IsRefueling)
            {
                if (animationStep == AnimationStep.First)
                    plutoniumCap.Play(AnimationStep.Second);
                else if (animationStep == AnimationStep.Second)
                    plutoniumCap.Play(AnimationStep.Third);
                else
                {
                    IsPlaying = false;
                    OnPlayerCompleted?.Invoke();
                }
            }
            else
            {
                if (animationStep == AnimationStep.Third)
                    plutoniumCap.Play(AnimationStep.Second);
                else if (animationStep == AnimationStep.Second)
                    plutoniumCap.Play();
                else
                {
                    IsPlaying = false;
                    OnPlayerCompleted?.Invoke();
                }
            }
        }

        public override void Play()
        {
            IsPlaying = true;

            if (!Properties.IsRefueling)
            {
                open.Play();
                plutoniumCap.Play();
            }
            else
            {
                close.Play();
                plutoniumCap.Play(AnimationStep.Third);
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
            plutoniumCap.Dispose();
            open.Dispose();
            close.Dispose();
        }
    }
}
