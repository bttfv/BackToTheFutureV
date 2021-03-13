using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Collections.Generic;
using System.Windows.Forms;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    internal class SIDHandler : HandlerPrimitive
    {
        private bool _waitTurnOn;

        private bool _randomDelay;

        public SIDHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SetSIDLedsState += SetAllState;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        private void SetColumnHeight(int column, int height)
        {
            int max = GetMaxHeight(column);

            if (height > max)
                height = max;

            if (height < 0)
                height = 0;

            Properties.NewHeight[column] = height;
            Properties.LedDelay[column] = 0;
        }

        private void Random(int min = 0, int max = 20)
        {
            if (_waitTurnOn && AreColumnProcessing().Count > 0)
                return;

            if (_waitTurnOn)
                _waitTurnOn = false;

            if (!_randomDelay)
                _randomDelay = true;

            for (int column = 0; column < 10; column++)
            {
                if (Properties.LedDelay[column] > Game.GameTime)
                    continue;

                SetColumnHeight(column, Utils.Random.Next(min, max + 1));
            }
        }

        private void SetAllState(bool on, bool instant = false)
        {
            for (int column = 0; column < 10; column++)
            {
                int max = GetMaxHeight(column);

                SetColumnHeight(column, on ? max : 0);

                if (instant)
                {
                    for (int row = 0; row < max; row++)
                        Properties.HUDProperties.LedState[column][row] = on;

                    Properties.CurrentHeight[column] = max;
                }
            }

            if (instant)
                return;

            _randomDelay = false;
            _waitTurnOn = on;
        }

        private List<int> AreColumnProcessing()
        {
            List<int> ret = new List<int>();

            for (int column = 0; column < 10; column++)
                if (Properties.NewHeight[column] != Properties.CurrentHeight[column] && Properties.LedDelay[column] < Game.GameTime)
                    ret.Add(column);

            return ret;
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

        public override void Tick()
        {
            foreach (int column in AreColumnProcessing())
            {
                if (Properties.NewHeight[column] > Properties.CurrentHeight[column])
                {
                    Properties.HUDProperties.LedState[column][Properties.CurrentHeight[column]] = true;
                    Properties.CurrentHeight[column]++;
                }
                else if (Properties.NewHeight[column] < Properties.CurrentHeight[column])
                {
                    Properties.CurrentHeight[column]--;
                    Properties.HUDProperties.LedState[column][Properties.CurrentHeight[column]] = false;
                }

                Properties.LedDelay[column] = Game.GameTime + 60 + (_randomDelay ? Utils.Random.Next(-30, 31) : 0);
            }

            if (Properties.IsGivenScaleformPriority && Vehicle.IsVisible)
                Scaleforms.SIDRT?.Draw();
            else
                return;

            if (!Properties.AreTimeCircuitsOn || Properties.HasBeenStruckByLightning || Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                return;

            if (Constants.OverSIDMaxAtSpeed || Properties.ForceSIDMax)
            {
                Random(20, 20);

                return;
            }

            float speed = Vehicle.GetMPHSpeed();

            if (speed.ToMPH() <= 1)
            {
                Random(0, 0);

                return;
            }

            if (speed > 88)
                speed = 88;

            int height = (int)((speed / 88) * 15);

            int min = height - 2;
            int max = height + 5;

            if (min < 0)
                min = 0;

            if (max > 20)
                max = 20;

            Random(min, max);
        }

        public override void Stop()
        {

        }
    }
}
