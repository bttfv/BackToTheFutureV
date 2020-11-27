using GTA;
using GTA.Native;
using GTA.UI;
using System.Drawing;

namespace FusionLibrary
{
    public class ScreenFlash
    {
        public static int CurrentAlpha { get; private set; }

        public static bool IsFlashing { get; private set; }

        private static float time;
        private static int currentStep;

        public static void FlashScreen(float timeInSecs)
        {
            IsFlashing = true;
            time = timeInSecs;
            currentStep = 0;
        }

        internal static void Process()
        {
            if (!IsFlashing) return;

            switch(currentStep)
            {
                case 0:
                    float numToAdd = (255 * Game.LastFrameTime) / (time / 2);
                    CurrentAlpha += (int)numToAdd;

                    if (CurrentAlpha > 255)
                    {
                        CurrentAlpha = 255;
                        currentStep++;
                    }

                    break;

                case 1:
                    float numToSub = (255 * Game.LastFrameTime) / (time / 2);
                    CurrentAlpha -= (int)numToSub;

                    if (CurrentAlpha < 0)
                    {
                        IsFlashing = false;
                        currentStep = 0;
                        CurrentAlpha = 0;
                    }

                    break;
            }

            Function.Call(Hash.DRAW_RECT, 0f, 0f, 3f, 3f, 255, 255, 255, CurrentAlpha);
        }
    }
}
