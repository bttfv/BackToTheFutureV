using System.Windows.Forms;
using System.Drawing;
using BackToTheFutureV.GUI;
using GTA;
using GTA.Math;

using BackToTheFutureV.Utility;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;
using FusionLibrary;
using static FusionLibrary.Enums;
using FusionLibrary.Extensions;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class SpeedoHandler : Handler
    {
        private int nextCheck;
        private int currentSpeed;

        public SpeedoHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            UpdateGUI(Scaleforms.GUI, "8", "8");
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

            Scaleforms.SpeedoRT.Draw();

            if (Game.GameTime < nextCheck) return;

            if (Properties.TimeTravelPhase < TimeTravelPhase.InTime)
            {
                int mphSpeed = ((int) Vehicle.GetMPHSpeed());

                if (mphSpeed > 88)
                    mphSpeed = 88;

                string mphSpeedStr = mphSpeed.ToString("00");

                string speedDigit1 = mphSpeedStr.Substring(0, 1);
                string speedDigit2 = mphSpeedStr.Substring(1, 1);

                if (mphSpeed > currentSpeed || mphSpeed < currentSpeed)
                {
                    currentSpeed = mphSpeed;

                    if (ModSettings.PlaySpeedoBeep && Vehicle.IsVisible)
                        Sounds.Speedo.Play();

                    UpdateGUI(ScaleformsHandler.Speedo, speedDigit1, speedDigit2);

                    if (!TcdEditer.IsEditing)
                        UpdateGUI(Scaleforms.GUI, speedDigit1, speedDigit2);
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