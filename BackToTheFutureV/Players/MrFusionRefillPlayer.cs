using GTA;
using GTA.Math;
using BackToTheFutureV.Utility;
using KlangRageAudioLibrary;
using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
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
            _mrFusion = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFMrFusion), "mr_fusion");
            _mrFusion.setRotationSettings(Coordinate.X, false, false, -70, 0, 1, 70, 1, true);
            _mrFusion.AnimationStopped += AnimationStopped;
            _mrFusion.SpawnProp();

            _mrFusionHandle = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFMrFusionHandle), "mr_fusion_handle");
            _mrFusionHandle.setRotationSettings(Coordinate.X, false, true, 0, 30, 1, 45, 1, true);
            _mrFusionHandle.AnimationStopped += AnimationStopped;            
            _mrFusionHandle.SpawnProp();
            
            _mrfusionOpen = TimeMachine.Sounds.AudioEngine.Create("general/mrfusionOpen.wav", Presets.Exterior);
            _mrfusionClosed = TimeMachine.Sounds.AudioEngine.Create("general/mrfusionClose.wav", Presets.Exterior);

            _mrfusionOpen.Volume = 0.4f;
            _mrfusionClosed.Volume = 0.4f;
        }

        public void AnimationStopped(AnimateProp animateProp, Coordinate coordinate, CoordinateSetting coordinateSetting, bool IsRotation)
        {
            if (animateProp == _mrFusionHandle)
            {
                if (open)
                    _mrFusion.setRotationUpdate(Coordinate.X, true);
            }

            if (animateProp == _mrFusion)
            {
                if (!open)
                    _mrFusionHandle.setRotationUpdate(Coordinate.X, true);
            }
        }

        public override void Play()
        {
            _mrfusionClosed?.Stop();
            _mrfusionOpen?.Stop();

            open = !open;
            
            if (open)
            {
                _mrFusionHandle.setRotationUpdate(Coordinate.X, true);
                _mrfusionOpen.Play();
            }                
            else
            {
                _mrFusion.setRotationUpdate(Coordinate.X, true);
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
            _mrfusionOpen?.Dispose();
            _mrfusionClosed?.Dispose();
            _mrFusion?.Dispose();
            _mrFusionHandle?.Dispose();
        }
    }
}
