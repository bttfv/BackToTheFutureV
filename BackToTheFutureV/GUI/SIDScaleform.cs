using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using System.Drawing;

namespace BackToTheFutureV.GUI
{
    public  class SIDScaleform : ScaleformGui
    {
        private bool[][] ledState;

        private bool _waitTurnOn;

        private int[] _currentHeight;
        private int[] _newHeight;
        private int[] _ledDelay;
        private int _delay;

        private const int _minDelay = 60;

        public SIDScaleform(string scaleformID) : base(scaleformID)
        {
            ledState = new bool[10][];

            _currentHeight = new int[10];
            _newHeight = new int[10];
            _ledDelay = new int[10];

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

        public int SetColumnHeight(int column, int height)
        {
            _newHeight[column] = height;
            _ledDelay[column] = 0;

            return Math.Abs(height - _currentHeight[column]) * (_minDelay + Utils.Random.Next(-30,31));
        }

        public void Random(int min = 0, int max = 20)
        {
            if (_delay > Game.GameTime || (_waitTurnOn && AreColumnProcessing()))
                return;

            if (_waitTurnOn)
            {
                _delay = Game.GameTime + _minDelay;
                _waitTurnOn = false;
                return;
            }

            int maxWait = 0;

            for (int column = 0; column < 10; column++)
            {
                int curWait = SetColumnHeight(column, Utils.Random.Next(min, 1 + max));

                if (curWait > maxWait)
                    maxWait = curWait;
            }
                
            _delay = Game.GameTime + maxWait;
        }

        public void SetAllState(bool on)
        {
            for (int column = 0; column < 10; column++)
                SetColumnHeight(column, on ? 20 : 0);

            _waitTurnOn = on;
        }

        public bool AreColumnProcessing()
        {
            for (int column = 0; column < 10; column++)
                if (_newHeight[column] != _currentHeight[column])
                    return true;

            return false;
        }

        public void Process()
        {
            for (int column = 0; column < 10; column++)
            {
                if (_ledDelay[column] < Game.GameTime && _newHeight[column] != _currentHeight[column])
                {
                    if (_newHeight[column] > _currentHeight[column])
                    {
                        _currentHeight[column]++;
                        ledState[column][_currentHeight[column] - 1] = true;
                    }
                    else if (_newHeight[column] < _currentHeight[column])
                    {
                        ledState[column][_currentHeight[column] - 1] = false;
                        _currentHeight[column]--;
                    }

                    _ledDelay[column] = Game.GameTime + _minDelay;
                }
            }
        }

        private void SetLed()
        {
            for (int column = 0; column < 10; column++)
                for (int row = 0; row < 20; row++)
                    CallFunction("setLed", column, row, Convert.ToInt32(ledState[column][row]));
        }

        public void Draw2D()
        {
            SetLed();
            Render2D(ModSettings.SIDPosition, new SizeF(ModSettings.SIDScale * (800f / 1414f) / GTA.UI.Screen.AspectRatio, ModSettings.SIDScale));
        }

        public void Draw3D()
        {
            //SetLed();
            //Render3D(Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }
    }
}
