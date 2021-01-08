using System;
using System.Threading;

namespace BackToTheFutureV
{
    public static class ExternalTimeCircuits
    {
        private static Thread _backgroundThread;

        public static TimeCircuits.Display TimeCircuits { get; private set; }

        public static bool IsOpen => TimeCircuits != null;

        public static void Toggle(bool state)
        {
            if (state)
                Start();
            else
                Stop();
        }

        public static void Start()
        {
            if (IsOpen)
                Stop();

            TimeCircuits = new TimeCircuits.Display();

            TimeCircuits.Exiting += TimeCircuits_Exiting;

            _backgroundThread = new Thread(Process)
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
            _backgroundThread?.Abort();

            if (IsOpen)
                TimeCircuits.Exiting -= TimeCircuits_Exiting;

            TimeCircuits?.Exit();
            TimeCircuits?.Dispose();

            TimeCircuits = null;
            _backgroundThread = null;
        }

        private static void Process()
        {
            TimeCircuits?.Run();
        }
    }
}
