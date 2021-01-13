using FusionLibrary;
using GTA;
using System.Drawing;

namespace BackToTheFutureV.GUI
{
    public  class SIDScaleform : ScaleformGui
    {
        private bool[][] ledState;

        private int _delay;

        public SIDScaleform() : base("sid")
        {
            DrawInPauseMenu = true;

            ledState = new bool[10][];

            for (int i = 0; i < 10; i++)
                ledState[i] = new bool[20];
        }

        public void SetLedState(int column, int row, bool on)
        {
            ledState[column][row] = on;
        }

        public bool GetLedState(int column, int row)
        {
            return ledState[column][row];
        }

        public void SetColumnHeight(int column, int height)
        {
            for (int i = 0; i <= height - 1; i++)
                ledState[column][i] = true;

            for (int i = height; i < 20; i++)
                ledState[column][i] = false;
        }

        public void Random()
        {
            if (_delay > Game.GameTime)
                return;

            for (int column = 0; column < 10; column++)
                SetColumnHeight(column, Utils.Random.Next(0, 21));

            _delay = Game.GameTime + 750;
        }

        public void Draw()
        {
            for (int column = 0; column < 10; column++)
                for (int row = 0; row < 20; row++)
                {
                    CallFunction("setLed", column, row, 0);
                    CallFunction("setLed", column, row, ledState[column][row] ? 1 : 0);
                }
                    
            Render2D(ModSettings.TCDPosition, new SizeF(ModSettings.TCDScale * (414f / 732f) / GTA.UI.Screen.AspectRatio, ModSettings.TCDScale));
        }
    }
}
