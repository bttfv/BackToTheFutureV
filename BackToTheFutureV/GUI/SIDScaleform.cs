using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.UI;
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

        private bool _randomDelay;
        private const int _minDelay = 60;

        public float x=0.626f, y=1.967f, scale=3.932f;

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
            if (column > 9)
                column = 9;

            if (row > 19)
                row = 19;

            if (row < 0)
                row = 0;

            ledState[column][row] = on;
        }

        public bool GetLedState(int column, int row)
        {
            if (column > 9)
                column = 9;

            if (row > 19)
                row = 19;

            if (row < 0)
                row = 0;

            return ledState[column][row];
        }

        public void SetColumnHeight(int column, int height)
        {
            if (column > 9)
                column = 9;

            if (height > 20)
                height = 20;

            if (height < 0)
                height = 0;

            _newHeight[column] = height;
            _ledDelay[column] = 0;
        }

        public void Random(int min = 0, int max = 20)
        {
            if (_waitTurnOn && AreColumnProcessing()) 
                return;

            if (_waitTurnOn)
                _waitTurnOn = false;

            if (!_randomDelay)
                _randomDelay = true;

            for (int column = 0; column < 10; column++)
            {
                if (_ledDelay[column] > Game.GameTime)
                    continue;

                SetColumnHeight(column, Utils.Random.Next(min, max + 1));
            }
        }

        public void SetAllState(bool on, bool force = false)
        {
            if (AreColumnProcessing() && !force)
                return;

            for (int column = 0; column < 10; column++)
                SetColumnHeight(column, on ? 20 : 0);

            _randomDelay = false;
            _waitTurnOn = on;
        }

        public bool AreColumnProcessing()
        {
            for (int column = 0; column < 10; column++)
                if (_newHeight[column] != _currentHeight[column])
                    return true;

            return false;
        }

        private int GetMaxHeight(int column)
        {
            switch (column)
            {
                case 2:
                    return 13;
                case 5:
                    return 19;
                case 7:
                    return 10;
                case 9:
                    return 17;
                default:
                    return 20;
            }
        }

        public void Process()
        {
            for (int column = 0; column < 10; column++)
            {
                if (_ledDelay[column] < Game.GameTime && _newHeight[column] != _currentHeight[column])
                {
                    if (_newHeight[column] > GetMaxHeight(column))
                        _newHeight[column] = GetMaxHeight(column);

                    if (_newHeight[column] > _currentHeight[column])
                    {                        
                        ledState[column][_currentHeight[column]] = true;
                        _currentHeight[column]++;
                    }
                    else if (_newHeight[column] < _currentHeight[column])
                    {
                        _currentHeight[column]--;
                        ledState[column][_currentHeight[column]] = false;
                    }

                    _ledDelay[column] = Game.GameTime + _minDelay + (_randomDelay ? Utils.Random.Next(-30, 31) : 0);
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
            Render2D(ModSettings.SIDPosition, new SizeF(ModSettings.SIDScale * (800f / 1414f) / Screen.AspectRatio, ModSettings.SIDScale));
        }

        public void Draw3D()
        {
            SetLed();
            Render2D(new PointF(x, y), new SizeF(scale * (800f / 1414f) / Screen.AspectRatio, scale));

            //Screen.ShowSubtitle($"{x} {y} {scale}");
        }
    }
}
