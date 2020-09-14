using System;
using System.Windows.Forms;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class TFCHandler : Handler
    {
        private float rotateTfcTo;
        private float currentTfcRotation;
        private bool rotate;

        public TFCHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
        }   

        private void OnTimeCircuitsToggle()
        {
            if(Properties.AreTimeCircuitsOn)
            {
                Props.TFCOn.SpawnProp(false);
                Props.TFCOff.DeleteProp();

                rotate = true;
                rotateTfcTo = -45f;
            }
            else
            {
                Props.TFCOff.SpawnProp(false);
                Props.TFCOn.DeleteProp();

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
                Props.TFCHandle.SpawnProp(Vector3.Zero, new Vector3(0, currentTfcRotation, 0), false);

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

        }
    }
}
