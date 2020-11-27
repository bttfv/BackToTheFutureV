using System;
using GTA;
using GTA.Native;

namespace FusionLibrary
{
    public class Main : Script
    {
        public Main()
        {
            Tick += Main_Tick;
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading)
                return;

            TimeHandler.Process();
            AnimatePropsHandler.Process();
            CustomNativeMenu.ObjectPool.Process();
            CustomNativeMenu.ProcessAll();
            ScreenFlash.Process();
            PlayerSwitch.Process();
            NativeInput.ProcessAll();            

            if (PlayerSwitch.Disable)
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 19, true);

            if (Utils.HideGUI)
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
        }
    }
}
