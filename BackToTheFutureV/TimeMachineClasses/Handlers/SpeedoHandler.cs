using BackToTheFutureV.Settings;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class SpeedoHandler : Handler
    {
        private int nextCheck;
        private int currentSpeed = -1;

        public SpeedoHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(Keys key)
        {
        }

        public override void Process()
        {
            if (!Properties.IsGivenScaleformPriority || Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Position) > 6f * 6f)
                return;

            Scaleforms.SpeedoRT?.Draw();

            if (Game.GameTime < nextCheck) return;

            if (Properties.TimeTravelPhase < TimeTravelPhase.InTime)
            {
                int mphSpeed = ((int)Vehicle.GetMPHSpeed());

                if (mphSpeed > 88)
                    mphSpeed = 88;

                string mphSpeedStr = mphSpeed.ToString("00");

                string speedDigit1 = mphSpeedStr.Substring(0, 1);
                string speedDigit2 = mphSpeedStr.Substring(1, 1);

                if (mphSpeed > currentSpeed || mphSpeed < currentSpeed)
                {
                    currentSpeed = mphSpeed;

                    if (ModSettings.PlaySpeedoBeep && Vehicle.IsVisible)
                        Sounds.Speedo?.Play();

                    UpdateGUI(ScaleformsHandler.Speedo, speedDigit1, speedDigit2);

                    Constants.HUDProperties.Speed = mphSpeed;

                    if (!TcdEditer.IsEditing && !RCGUIEditer.IsEditing)
                        UpdateGUI(ScaleformsHandler.GUI, speedDigit1, speedDigit2);
                }
            }

            nextCheck = Game.GameTime + 20;
        }

        private void UpdateGUI(ScaleformGui scaleform, string digit1, string digit2)
        {
            if (digit1 != "0")
                scaleform.CallFunction("SET_DIGIT_1", int.Parse(digit1));
            else
                scaleform.CallFunction("SET_DIGIT_1", 15);

            scaleform.CallFunction("SET_DIGIT_2", int.Parse(digit2));
        }

        public override void Stop()
        {

        }
    }
}