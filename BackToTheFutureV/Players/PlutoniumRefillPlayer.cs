using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.UI;
using BackToTheFutureV.TimeMachineClasses;
using System.Threading.Tasks;
using BTTFVUtils;

namespace BackToTheFutureV.Players
{
    public class PlutoniumRefillPlayer : Player
    {
        private float currentRotation;
        private float currentOffset;

        private AnimateProp plutoniumCap;

        private bool open;
        private int currentStep;

        public PlutoniumRefillPlayer(TimeMachine timeMachine) : base(timeMachine)
        {
            plutoniumCap = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFReactorCap), "bttf_reactorcap");
            plutoniumCap.SpawnProp();
        }

        public override void Play()
        {
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
                        float rotationToAdd = (90f * Game.LastFrameTime) / 0.75f;
                        currentRotation -= rotationToAdd;

                        if (currentRotation <= -90)
                            currentStep++;
                    }
                    else
                    {
                        float offsetToSub = (0.06f * Game.LastFrameTime) / 0.75f;
                        currentOffset -= offsetToSub;

                        if (currentOffset <= 0)
                            currentStep++;
                    }

                    break;

                case 1:
                    if(open)
                    {
                        float offsetToAdd = (0.06f * Game.LastFrameTime) / 0.75f;
                        currentOffset += offsetToAdd;

                        if(currentOffset >= 0.06f)
                            Stop();
                    }
                    else
                    {
                        float rotationToSub = (90f * Game.LastFrameTime) / 0.75f;
                        currentRotation += rotationToSub;

                        if (currentRotation >= 0)
                            Stop();
                    }

                    break;
            }

            plutoniumCap.SpawnProp(new Vector3(0, 0, currentOffset), new Vector3(0, 0, currentRotation), false);
        }

        public override void Stop()
        {
            IsPlaying = false;
            currentStep = 0;
        }

        public override void Dispose()
        {
            plutoniumCap?.Dispose();
        }
    }
}
