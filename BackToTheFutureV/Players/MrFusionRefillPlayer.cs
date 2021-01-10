using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using FusionLibrary;
using KlangRageAudioLibrary;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Players
{
    public class MrFusionRefillPlayer : Player
    {
        private readonly AnimateProp _mrFusion;
        private readonly AnimateProp _mrFusionHandle;

        private readonly AudioPlayer _mrfusionOpen;
        private readonly AudioPlayer _mrfusionClosed;

        private bool open;

        public MrFusionRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            _mrFusion = new AnimateProp(Vehicle, ModelHandler.BTTFMrFusion, "mr_fusion");
            _mrFusion[AnimationType.Rotation][AnimationStep.First][Coordinate.X].Setup(false, true, false, -70, 0, 1, 140, 1);
            _mrFusion.OnAnimCompleted += _mrFusion_OnAnimCompleted;
            _mrFusion.SpawnProp();

            _mrFusionHandle = new AnimateProp(Vehicle, ModelHandler.BTTFMrFusionHandle, "mr_fusion_handle");
            _mrFusionHandle[AnimationType.Rotation][AnimationStep.First][Coordinate.X].Setup(false, true, true, 0, 30, 1, 140, 1);
            _mrFusionHandle.OnAnimCompleted += _mrFusionHandle_OnAnimCompleted;
            _mrFusionHandle.SpawnProp();
            
            _mrfusionOpen = TimeMachine.Sounds.AudioEngine.Create("general/mrfusionOpen.wav", Presets.Exterior);
            _mrfusionClosed = TimeMachine.Sounds.AudioEngine.Create("general/mrfusionClose.wav", Presets.Exterior);

            _mrfusionOpen.Volume = 0.4f;
            _mrfusionClosed.Volume = 0.4f;
        }

        private void _mrFusionHandle_OnAnimCompleted(AnimationStep animationStep)
        {
            if (open)
                _mrFusion.Play();
        }

        private void _mrFusion_OnAnimCompleted(AnimationStep animationStep)
        {
            if (!open)
                _mrFusionHandle.Play();
        }

        public override void Play()
        {
            _mrfusionClosed?.Stop();
            _mrfusionOpen?.Stop();

            open = !open;
            
            if (open)
            {
                _mrFusionHandle.Play();
                _mrfusionOpen.Play();
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
            _mrFusion.Dispose();
            _mrFusionHandle.Dispose();
            _mrfusionOpen?.Dispose();
            _mrfusionClosed?.Dispose();
        }
    }
}
