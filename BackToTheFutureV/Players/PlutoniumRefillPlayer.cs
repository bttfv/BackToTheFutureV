using FusionLibrary;
using KlangRageAudioLibrary;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class PlutoniumRefillPlayer : Players.Player
    {
        private readonly AnimateProp plutoniumCap;

        private readonly AudioPlayer open;
        private readonly AudioPlayer close;

        public PlutoniumRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            open = Sounds.AudioEngine.Create("bttf1/plutonium_open.wav", Presets.Exterior);
            open.SourceBone = "bttf_reactorcap";

            close = Sounds.AudioEngine.Create("bttf1/plutonium_close.wav", Presets.Exterior);
            close.SourceBone = "bttf_reactorcap";

            plutoniumCap = new AnimateProp(ModelHandler.BTTFReactorCap, Vehicle, "bttf_reactorcap");
            plutoniumCap[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].Setup(true, false, -90, 0, 1, 240, 1, false);
            plutoniumCap[AnimationType.Offset][AnimationStep.Second][Coordinate.Z].Setup(true, true, 0, 0.05f, 1, 0.16f, 1, false);
            plutoniumCap[AnimationType.Offset][AnimationStep.Third][Coordinate.Y].Setup(true, true, 0, 0.08f, 1, 0.16f, 1, false);
            plutoniumCap.OnAnimCompleted += PlutoniumCap_OnAnimCompleted;
            plutoniumCap.SpawnProp();
        }

        private void PlutoniumCap_OnAnimCompleted(AnimationStep animationStep)
        {
            if (Properties.ReactorState == ReactorState.Opened)
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

            if (Properties.ReactorState == ReactorState.Opened)
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

        public override void Tick()
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
