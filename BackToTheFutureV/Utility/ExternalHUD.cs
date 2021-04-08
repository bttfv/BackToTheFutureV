using BackToTheFutureV.HUD.Core;
using FusionLibrary;
using System;
using System.Threading;

namespace BackToTheFutureV
{
    internal static class ExternalHUD
    {
        private static Thread _backgroundThread;

        private static HUDDisplay HUD;

        private const int port = 1985;

        public static bool IsActive => HUD != null;

        public static void Toggle(bool state)
        {
            if (state)
                Start();
            else
                Stop();
        }

        public static void Start()
        {
            if (IsActive)
                Stop();

            HUD = new HUDDisplay();

            HUD.Exiting += TimeCircuits_Exiting;

            _backgroundThread = new Thread(Tick)
            {
                IsBackground = true
            };

            _backgroundThread?.Start();
        }

        private static void TimeCircuits_Exiting(object sender, EventArgs e)
        {
            ModSettings.ExternalTCDToggle = false;
            ModSettings.SaveSettings();

            Stop();
        }

        public static void Stop()
        {
            SetOff();

            _backgroundThread?.Abort();

            if (IsActive)
                HUD.Exiting -= TimeCircuits_Exiting;

            HUD?.Exit();
            HUD?.Dispose();

            HUD = null;
            _backgroundThread = null;
        }

        private static void Tick()
        {
            HUD?.Run();
        }

        public static void Update(HUDProperties properties)
        {
            if (ModSettings.ExternalTCDToggle && IsActive)
                HUD.Properties = properties;

            if (!ModSettings.RemoteTCDToggle)
                return;

            FusionNetwork.SendMsg(properties, port);
        }

        public static void SetOff()
        {
            if (ModSettings.ExternalTCDToggle && IsActive)
                HUD.Properties = new HUDProperties();

            if (!ModSettings.RemoteTCDToggle)
                return;

            FusionNetwork.SendMsg(new HUDProperties(), port);
        }
    }
}
