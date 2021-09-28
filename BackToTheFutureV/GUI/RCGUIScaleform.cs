using FusionLibrary;
using System;
using System.Drawing;

namespace BackToTheFutureV
{
    internal class RCGUIScaleform : ScaleformGui
    {
        public RCGUIScaleform() : base("bttf_2d_rc_gui")
        {

        }

        public void Preview()
        {
            SetSpeed(0);
            SetStop(false);
            Draw2D();
        }

        public void SetSpeed(float mphSpeed)
        {
            if (mphSpeed > 88.0f)
            {
                mphSpeed = 88.0f;
            }

            string mphSpeedStr = mphSpeed.ToString("00.0");

            int speedDigit1 = int.Parse(mphSpeedStr.Substring(0, 1));
            int speedDigit2 = int.Parse(mphSpeedStr.Substring(1, 1));
            int speedDigit3 = int.Parse(mphSpeedStr.Substring(3, 1));

            CallFunction("SET_DIGIT_1", speedDigit1 == 0 ? 10 : speedDigit1);
            CallFunction("SET_DIGIT_2", speedDigit2);
            CallFunction("SET_DIGIT_3", speedDigit3);
        }

        public void SetStop(bool state)
        {
            CallFunction("SET_STOP", Convert.ToInt32(state));
        }

        public void Draw2D()
        {
            Render2D(ModSettings.RCGUIPosition, new SizeF(ModSettings.RCGUIScale * (1000f / 500f) / GTA.UI.Screen.AspectRatio, ModSettings.RCGUIScale));
        }
    }
}
