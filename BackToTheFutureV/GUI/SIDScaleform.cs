using FusionLibrary;
using GTA.UI;
using System;
using System.Drawing;

namespace BackToTheFutureV
{
    internal class SIDScaleform : ScaleformGui
    {
        private PropertiesHandler Properties => TimeMachineHandler.ClosestTimeMachine.Properties;

        private static PointF SID3DLocation = new PointF() { X = 0.626f, Y = 0.626f };
        private static float SID3DScale = 1.284f;

        public SIDScaleform(bool is2D) : base(is2D ? "bttf_2d_sid" : "bttf_3d_sid")
        {

        }

        private void SetLed()
        {
            if (TcdEditer.IsEditing)
                return;

            for (int column = 0; column < 10; column++)
                CallFunction("setLed", column, Convert.ToInt32(Properties.CurrentHeight[column]));
        }

        public void Draw2D()
        {
            CallFunction("setBackground", 2);

            SetLed();
            Render2D(ModSettings.SIDPosition, new SizeF(ModSettings.SIDScale * (800f / 1414f) / Screen.AspectRatio, ModSettings.SIDScale));
        }

        public void Draw3D()
        {
            SetLed();
            Render2D(SID3DLocation, new SizeF(SID3DScale * (800f / 1414f) / Screen.AspectRatio, SID3DScale));
        }
    }
}
