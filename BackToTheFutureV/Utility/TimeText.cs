using GTA.Native;
using GTA;
using System;

namespace BackToTheFutureV
{
    internal class TimeText
    {
        private static int textAlpha1 = 0;
        private static int textAlpha2 = 0;
        private static int textAlpha3 = 0;

        private static void TextBlock(string input)
        {
            var fVar1 = Function.Call<float>(Hash.GET_RENDERED_CHARACTER_HEIGHT, 1.25f, 0);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING"); // draw crap on screen
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, input);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.62f - fVar1 / 2, 0);
            // -1f to 1f.
            // X - Horizontal
            // Y - Vertical
        }

        private static void FadeTextIn(int line)
        {
            Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f); // scale, size
            switch (line)
            {
                case 1:
                    {
                        if (textAlpha1 * 8 < 252)
                        {
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha1 * 8);
                            textAlpha1++;
                        }
                        else
                        {
                            textAlpha1 = 63;
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                        }
                        break;
                    }
                case 2:
                    {
                        if (textAlpha2 * 8 < 252)
                        {
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha2 * 8);
                            textAlpha2++;
                        }
                        else
                        {
                            textAlpha2 = 63;
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                        }
                        break;
                    }
                case 3:
                    {
                        if (textAlpha3 * 8 < 252)
                        {
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha3 * 8);
                            textAlpha3++;
                        }
                        else
                        {
                            textAlpha3 = 63;
                            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                        }
                        break;
                    }
            }
        }

        private static void FadeTextOut()
        {
            Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f); // scale, size
            if (textAlpha1 * 4 > 0)
            {
                Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha1 * 4);
                textAlpha1--;
            }
            else
            {
                Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 0);
                textAlpha1 = 0;
            }
        }

        public static void DisplayText(DateTime date)
        {
            Function.Call(Hash.SETTIMERA, 0);
            while (true)
            {
                Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
                FadeTextIn(1);
                TextBlock($"{date:dddd}");
                Script.Wait(1);
                if (Function.Call<int>(Hash.TIMERA) > 1500)
                {
                    FadeTextIn(2);
                    TextBlock($"\n{date:MMMM d, yyyy}");
                }
                if (Function.Call<int>(Hash.TIMERA) > 3000)
                {
                    FadeTextIn(3);
                    TextBlock($"\n\n{date:h:mm tt}");
                }
                if (Function.Call<int>(Hash.TIMERA) > 6000)
                {
                    break;
                }
            }
            while (true)
            {
                Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
                FadeTextOut();
                TextBlock($"{date:dddd}\n{date:MMMM d, yyyy}\n{date:h:mm tt}");
                Script.Wait(1);
                if (Function.Call<int>(Hash.TIMERA) > 8000)
                {
                    break;
                }
            }
            Function.Call(Hash.RESET_SCRIPT_GFX_ALIGN);
            Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
            if (GTA.UI.Screen.IsFadedOut && !GTA.UI.Screen.IsFadingIn)
            {
                GTA.UI.Screen.FadeIn(1000);
            }
            textAlpha1 = 0;
            textAlpha2 = 0;
            textAlpha3 = 0;
        }
    }
}
