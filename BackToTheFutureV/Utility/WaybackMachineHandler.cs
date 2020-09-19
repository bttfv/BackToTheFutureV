using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Utility
{
    public class WaybackMachineHandler : Script
    {
        public static List<WaybackMachine> WaybackMachines = new List<WaybackMachine>();

        public int MainDelay = 0;

        public WaybackMachineHandler()
        {
            Tick += WaybackMachineHandler_Tick;
            Aborted += WaybackMachineHandler_Aborted;
            KeyDown += WaybackMachineHandler_KeyDown;
        }

        private void WaybackMachineHandler_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.K)
            {
                WaybackMachines.Add(new WaybackMachine(Main.PlayerVehicle));
            }
        }

        private void WaybackMachineHandler_Aborted(object sender, EventArgs e)
        {
            
        }

        public static void Add(WaybackMachine waybackMachine)
        {
            WaybackMachines.Add(waybackMachine);
        }

        private void WaybackMachineHandler_Tick(object sender, EventArgs e)
        {
            if (MainDelay < Game.GameTime)
            {
                MainDelay = Game.GameTime + 20;

                foreach (WaybackMachine waybackMachine in WaybackMachines)
                    waybackMachine.Process();
            }
        }
    }
}
