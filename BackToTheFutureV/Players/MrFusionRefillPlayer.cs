using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using FusionLibrary;
using KlangRageAudioLibrary;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    internal class MrFusionRefillPlayer : Player
    {
        private readonly AnimateProp _mrFusion;
        private readonly AnimateProp _mrFusionHandle;

        private readonly AudioPlayer _mrfusionOpen;
        private readonly AudioPlayer _mrfusionClosed;

        public MrFusionRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            _mrFusion = new AnimateProp(ModelHandler.BTTFMrFusion, Vehicle, "mr_fusion");
            _mrFusion[AnimationType.Rotation][AnimationStep.First][Coordinate.X].Setup(true, false, -70, 0, 1, 140, 1);
            _mrFusion.OnAnimCompleted += _mrFusion_OnAnimCompleted;
            _mrFusion.SpawnProp();

            _mrFusionHandle = new AnimateProp(ModelHandler.BTTFMrFusionHandle, Vehicle, "mr_fusion_handle");
            _mrFusionHandle[AnimationType.Rotation][AnimationStep.First][Coordinate.X].Setup(true, true, 0, 30, 1, 140, 1);
            _mrFusionHandle.OnAnimCompleted += _mrFusionHandle_OnAnimCompleted;
            _mrFusionHandle.SpawnProp();

            _mrfusionOpen = Sounds.AudioEngine.Create("general/mrfusionOpen.wav", Presets.Exterior);
            _mrfusionClosed = Sounds.AudioEngine.Create("general/mrfusionClose.wav", Presets.Exterior);

            _mrfusionOpen.Volume = 0.4f;
            _mrfusionClosed.Volume = 0.4f;

            _mrfusionOpen.SourceBone = "mr_fusion";
            _mrfusionClosed.SourceBone = "mr_fusion";
        }

        private void _mrFusionHandle_OnAnimCompleted(AnimationStep animationStep)
        {
            if (!Properties.IsRefueling)
                _mrFusion.Play();
            else
            {
                Particles.MrFusionSmoke?.StopNaturally();

                IsPlaying = false;
                OnPlayerCompleted?.Invoke();
            }
        }

        private void _mrFusion_OnAnimCompleted(AnimationStep animationStep)
        {
            if (Properties.IsRefueling)
                _mrFusionHandle.Play();
            else
            {
                IsPlaying = false;
                OnPlayerCompleted?.Invoke();
            }
        }

        public override void Play()
        {
            IsPlaying = true;
            _mrfusionClosed?.Stop();
            _mrfusionOpen?.Stop();

            if (!Properties.IsRefueling)
            {
                _mrFusionHandle.Play();
                _mrfusionOpen.Play();
                Particles.MrFusionSmoke?.Play();
            }
            else
            {
                _mrFusion.Play();
                _mrfusionClosed.Play();
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
            Particles.MrFusionSmoke?.Stop();
            _mrFusion.Dispose();
            _mrFusionHandle.Dispose();
            _mrfusionOpen?.Dispose();
            _mrfusionClosed?.Dispose();
        }
    }
}
