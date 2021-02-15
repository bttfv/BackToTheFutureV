using BackToTheFutureV.TimeMachineClasses;
using GTA;
using System;

namespace BackToTheFutureV
{
    public class PersistenceScript : Script
    {
        public PersistenceScript()
        {
            Tick += PersistenceScript_Tick;
        }

        private void PersistenceScript_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading || Main.FirstTick)
                return;

            if (ModSettings.PersistenceSystem)
                TimeMachineHandler.SaveAllTimeMachines();
        }
    }
}
