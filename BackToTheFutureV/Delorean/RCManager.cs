using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Delorean.Handlers;
using GTA;
using GTA.UI;
using System.Drawing;
using NativeUI;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.Delorean
{
    public class RCManager
    {
        public static readonly float MAX_DIST = 650f;

        public static DeloreanTimeMachine RemoteControlling { get; private set; }

        private static RcHandler _cachedRCHandler;

        private static BarTimerBar _signalBar = new BarTimerBar(Game.GetLocalizedString("BTTFV_RC_Signal"));

        public static void RemoteControl(DeloreanTimeMachine timeMachine)
        {
            if (timeMachine == null)
                return;

            if (RemoteControlling != null)
                RemoteControlling.Circuits?.GetHandler<RcHandler>().StopRC();

            _cachedRCHandler = timeMachine?.Circuits?.GetHandler<RcHandler>();
            _cachedRCHandler?.StartRC();
            RemoteControlling = timeMachine;

            Main.DisablePlayerSwitching = true;
        }

        public static void Process()
        {
            if (RemoteControlling == null || _cachedRCHandler == null || !RemoteControlling.Circuits.IsRemoteControlled) return;

            float squareDist = RemoteControlling.Vehicle.Position.DistanceToSquared(_cachedRCHandler.OriginalPed.Position);
            float percentage = ((MAX_DIST * MAX_DIST - squareDist) / (MAX_DIST * MAX_DIST));

            _signalBar.Percentage = percentage;
            _signalBar.Draw(1);

            if (squareDist > MAX_DIST * MAX_DIST)
                StopRemoteControl();
        }

        public static void StopRemoteControl(bool instant = false)
        {
            RemoteControlling = null;
            _cachedRCHandler?.Stop(instant);
            _cachedRCHandler = null;
            
            Main.DisablePlayerSwitching = false;
        }

        public static void KeyPress(Keys key) {}
    }
}