using System;
using System.Windows.Forms;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class TFCHandler : Handler
    {
        private AnimateProp tfcOn;
        private AnimateProp tfcOff;

        private AnimateProp tfcHandle;

        private float rotateTfcTo;
        private float currentTfcRotation;
        private bool rotate;

        public TFCHandler(TimeCircuits circuits) : base(circuits)
        {
            tfcOn = new AnimateProp(Vehicle, ModelHandler.TFCOn, Vector3.Zero, Vector3.Zero);
            tfcOff = new AnimateProp(Vehicle, ModelHandler.TFCOff, Vector3.Zero, Vector3.Zero);
            tfcHandle = new AnimateProp(Vehicle, ModelHandler.TFCHandle, new Vector3(-0.03805999f, -0.0819466f, 0.5508024f), Vector3.Zero);
            tfcHandle.SpawnProp(false);

            TimeCircuits.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
        }   

        private void OnTimeCircuitsToggle()
        {
            if(IsOn)
            {
                tfcOn.SpawnProp(false);
                tfcOff.DeleteProp();

                rotate = true;
                rotateTfcTo = -45f;
            }
            else
            {
                tfcOff.SpawnProp(false);
                tfcOn.DeleteProp();

                rotate = true;
                rotateTfcTo = 0;
            }
        }

        public override void KeyPress(Keys key)
        {
        }

        public override void Process()
        {
            if(rotate)
            {
                currentTfcRotation = Utils.Lerp(currentTfcRotation, rotateTfcTo, Game.LastFrameTime * 8f);
                tfcHandle.SpawnProp(Vector3.Zero, new Vector3(0, currentTfcRotation, 0), false);

                var diff = Math.Abs(currentTfcRotation - rotateTfcTo);
                if (diff <= 0.001)
                    rotate = false;
            }
        }

        public override void Stop()
        {
        }

        public override void Dispose()
        {
            tfcOn?.Dispose();
            tfcOff?.Dispose();
            tfcHandle?.Dispose();
        }
    }
}
