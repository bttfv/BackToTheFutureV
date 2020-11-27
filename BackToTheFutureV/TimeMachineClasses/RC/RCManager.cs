using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.UI;
using System.Drawing;
using LemonUI;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers;
using LemonUI.TimerBars;
using BTTFVUtils;

namespace BackToTheFutureV.TimeMachineClasses.RC
{
    public class RCManager
    {
        public static readonly float MAX_DIST = 650f;

        public static TimeMachine RemoteControlling { get; private set; }
        public static bool IsRemoteOn => RemoteControlling != null;

        private static TimerBarCollection TimerBarCollection;
        private static TimerBarProgress SignalBar;

        static RCManager()
        {
            TimerBarCollection = new TimerBarCollection(SignalBar = new TimerBarProgress(Game.GetLocalizedString("BTTFV_RC_Signal")));
            TimerBarCollection.Visible = false;

            CustomNativeMenu.ObjectPool.Add(TimerBarCollection);
        }

        public static void RemoteControl(TimeMachine timeMachine)
        {
            if (timeMachine == null)
                return;

            if (IsRemoteOn)
                RemoteControlling.Events.SetRCMode?.Invoke(false, true);

            timeMachine.Events.SetRCMode?.Invoke(true);
            RemoteControlling = timeMachine;

            PlayerSwitch.Disable = true;
        }

        public static void Process()
        {
            if (!IsRemoteOn)
                return;

            float squareDist = RemoteControlling.Vehicle.Position.DistanceToSquared(RemoteControlling.OriginalPed.Position);

            if (squareDist > MAX_DIST * MAX_DIST)
            {
                StopRemoteControl();
                return;
            }

            float percentage = ((MAX_DIST * MAX_DIST - squareDist) / (MAX_DIST * MAX_DIST)) * 100;

            if (!TimerBarCollection.Visible)
                TimerBarCollection.Visible = true;

            SignalBar.Progress = percentage;
        }

        public static void StopRemoteControl(bool instant = false)
        {            
            RemoteControlling.Events.SetRCMode?.Invoke(false, instant);
            RemoteControlling = null;

            PlayerSwitch.Disable = false;
            TimerBarCollection.Visible = false;
        }
    }
}