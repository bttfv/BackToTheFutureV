using BackToTheFutureV.TimeMachineClasses;
using GTA;
using System;

namespace BackToTheFutureV
{
    internal class PersistenceScript : Script
    {
        public PersistenceScript()
        {
            Interval = 100;
            Tick += PersistenceScript_Tick;
        }

        private void PersistenceScript_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading || Main.FirstTick || !ModSettings.PersistenceSystem)
                return;

            TimeMachineHandler.SaveAllTimeMachines();
            RemoteTimeMachineHandler.Save();
        }
    }
}
