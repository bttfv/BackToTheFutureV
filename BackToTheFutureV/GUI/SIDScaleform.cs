using BackToTheFutureV.HUD.Core;
using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using GTA.UI;
using System;
using System.Drawing;

namespace BackToTheFutureV.GUI
{
    internal class SIDScaleform : ScaleformGui
    {
        private HUDProperties HUDProperties => TimeMachineHandler.ClosestTimeMachine.Properties.HUDProperties;

        private static PointF SID3DLocation = new PointF() { X = 0.626f, Y = 0.626f };
        private static float SID3DScale = 1.284f;

        public SIDScaleform(string scaleformID) : base(scaleformID)
        {

        }

        private void SetLed()
        {
            for (int column = 0; column < 10; column++)
                for (int row = 0; row < 20; row++)
                {
                    if (row > 0 && !HUDProperties.LedState[column][row - 1] && HUDProperties.LedState[column][row])
                        HUDProperties.LedState[column][row] = false;

                    CallFunction("setLed", column, row, Convert.ToInt32(HUDProperties.LedState[column][row]));
                }
        }

        public void Draw2D()
        {
            CallFunction("setBackground", 2);

            SetLed();
            Render2D(ModSettings.SIDPosition, new SizeF(ModSettings.SIDScale * (800f / 1414f) / Screen.AspectRatio, ModSettings.SIDScale));
        }

        public void Draw3D()
        {
            CallFunction("setBackground", 1);

            SetLed();
            Render2D(SID3DLocation, new SizeF(SID3DScale * (800f / 1414f) / Screen.AspectRatio, SID3DScale));
        }
    }
}
