using BackToTheFutureV.Utility;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV
{
    class MillisecondScript : Script
    {
        private ExternalThread externalThread;

        private bool _firstTick = true;

        public MillisecondScript()
        {
            externalThread = new ExternalThread();
            Interval = 1;

            Tick += MillisecondScript_Tick;
        }

        private void MillisecondScript_Tick(object sender, EventArgs e)
        {
            //externalThread.Interval = (int)(1000f / Game.FPS);
            externalThread.PauseAll = false;

            if (_firstTick)
            {
                externalThread.Start();
                _firstTick = false;
            }
        }
    }
}
