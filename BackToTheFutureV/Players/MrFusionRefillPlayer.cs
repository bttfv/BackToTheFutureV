using GTA;
using GTA.Math;
using BackToTheFutureV.Utility;
using KlangRageAudioLibrary;
using BackToTheFutureV.TimeMachineClasses;
using BTTFVUtils;

namespace BackToTheFutureV.Players
{
    public class MrFusionRefillPlayer : Player
    {
        private float _currentRotation;
        private float _currentHandleRotation;

        private readonly AnimateProp _mrFusion;
        private readonly AnimateProp _mrFusionHandle;

        private readonly AudioPlayer _mrfusionOpen;
        private readonly AudioPlayer _mrfusionClosed;

        private bool open;
        private int currentStep;

        public MrFusionRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            _mrFusion = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFMrFusion), "mr_fusion");
            _mrFusion.SpawnProp();

            _mrFusionHandle = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFMrFusionHandle), "mr_fusion_handle");
            _mrFusionHandle.SpawnProp();

            _mrfusionOpen = TimeMachine.Sounds.AudioEngine.Create("general/mrfusionOpen.wav", Presets.Exterior);
            _mrfusionClosed = TimeMachine.Sounds.AudioEngine.Create("general/mrfusionClose.wav", Presets.Exterior);

            _mrfusionOpen.Volume = 0.4f;
            _mrfusionClosed.Volume = 0.4f;
        }

        public override void Play()
        {
            _mrfusionClosed?.Stop();
            _mrfusionOpen?.Stop();
            open = !open;
            IsPlaying = true;
            currentStep = 0;
        }

        public override void Process()
        {
            if (!IsPlaying) return;

            switch (currentStep)
            {
                case 0:
                    if(open)
                    {
                        //float rotToAdd = (30f * Game.LastFrameTime) / 0.1f;
                        //_currentHandleRotation += rotToAdd;
                        _currentHandleRotation = Utils.Lerp(_currentHandleRotation, 30f, Game.LastFrameTime * 8f);

                        if (_currentHandleRotation >= 29f)
                            currentStep++;
                    }
                    else
                    {
                        if (!_mrfusionClosed.IsAnyInstancePlaying)
                            _mrfusionClosed.Play();

                        //float rotationToSub = (70f * Game.LastFrameTime) / 0.35f;
                        //_currentRotation += rotationToSub;
                        _currentRotation = Utils.Lerp(_currentRotation, 0, Game.LastFrameTime * 8f);

                        if (_currentRotation >= -0.1)
                            currentStep++;
                    }

                    break;
                case 1:
                    if (open)
                    {
                        if (!_mrfusionOpen.IsAnyInstancePlaying)
                            _mrfusionOpen.Play();

                        //float rotationToAdd = (70f * Game.LastFrameTime) / 0.35f;
                        //_currentRotation -= rotationToAdd;
                        _currentRotation = Utils.Lerp(_currentRotation, -70f, Game.LastFrameTime * 8f);

                        if (_currentRotation <= -69)
                            Stop();
                    }
                    else
                    {
                        //float rotToAdd = (30f * Game.LastFrameTime) / 0.1f;
                        //_currentHandleRotation -= rotToAdd;
                        _currentHandleRotation = Utils.Lerp(_currentHandleRotation, 0, Game.LastFrameTime * 8f);

                        if (_currentHandleRotation <= 0.1f)
                            Stop();
                    }

                    break;
            }

            _mrFusion.SpawnProp(new Vector3(0, 0, 0), new Vector3(_currentRotation, 0, 0), false);
            _mrFusionHandle.SpawnProp(new Vector3(0, 0, 0), new Vector3(_currentHandleRotation, 0, 0), false);
        }

        public override void Stop()
        {
            IsPlaying = false;
            currentStep = 0;
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
