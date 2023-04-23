using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class SpeedoHandler : HandlerPrimitive
    {
        private int nextCheck;
        private float currentSpeed = -1;

        public SpeedoHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {
        }

        public override void Tick()
        {
            if (!Constants.HasScaleformPriority)
            {
                return;
            }

            if (Mods.IsDMC12)
            {
                if (Mods.Speedo == ModState.Off && !Props.SpeedoCover.IsSpawned)
                {
                    Props.SpeedoCover.SpawnProp();
                    currentSpeed = -1;
                }

                if (Mods.Speedo == ModState.On && Props.SpeedoCover.IsSpawned)
                {
                    Props.SpeedoCover.Delete();
                    currentSpeed = -1;
                }
            }

            Scaleforms.SpeedoRT?.Draw();

            if (Game.GameTime < nextCheck)
            {
                return;
            }

            if (Properties.TimeTravelPhase < TimeTravelPhase.InTime)
            {
                float mphSpeed = Vehicle.GetMPHSpeed();

                if (mphSpeed > 88)
                {
                    mphSpeed = 88;
                }

                string mphSpeedStr = mphSpeed.ToString("00.0");

                int speedDigit1 = int.Parse(mphSpeedStr.Substring(0, 1));
                int speedDigit2 = int.Parse(mphSpeedStr.Substring(1, 1));
                int speedDigit3 = int.Parse(mphSpeedStr.Substring(3, 1));

                if (mphSpeed > currentSpeed || mphSpeed < currentSpeed)
                {
                    if ((int)mphSpeed != (int)currentSpeed && ModSettings.PlaySpeedoBeep && Vehicle.IsVisible)
                    {
                        Sounds.Speedo?.Play();
                    }

                    currentSpeed = mphSpeed;

                    if (Mods.IsDMC12)
                    {
                        UpdateGUI(ScaleformsHandler.Speedo, speedDigit1, speedDigit2, speedDigit3);
                    }

                    Properties.HUDProperties.Speed = (int)mphSpeed;

                    if (!TcdEditer.IsEditing && !RCGUIEditer.IsEditing)
                    {
                        UpdateGUI(ScaleformsHandler.GUI, speedDigit1, speedDigit2, speedDigit3);
                    }
                }
            }

            nextCheck = Game.GameTime + 20;
        }

        private void UpdateGUI(ScaleformGui scaleform, int digit1, int digit2, int digit3)
        {
            scaleform.CallFunction("SET_DIGIT_1", digit1 == 0 ? 10 : digit1);
            scaleform.CallFunction("SET_DIGIT_2", digit2);

            if (Mods.Speedo == ModState.On || Properties.ThreeDigit2D)
            {
                scaleform.CallFunction("SET_DIGIT_3", digit3);
            }
            else
            {
                scaleform.CallFunction("SET_DIGIT_3", 10);
            }
        }

        public override void Stop()
        {

        }
    }
}
