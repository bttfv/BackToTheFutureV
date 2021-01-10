using System;
using System.Threading;
using TimeCircuits;

namespace BackToTheFutureV
{
    public static class ExternalHUD
    {
        private static Thread _backgroundThread;

        private static Display TimeCircuits;

        public static bool IsActive => TimeCircuits != null;

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

            TimeCircuits = new Display();

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

            if (IsActive)
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

        public static bool IsHUDVisible
        {
            get
            {
                if (IsActive)
                    return TimeCircuits.IsHUDVisible;

                return false;
            }
            set
            {
                if (IsActive)
                    TimeCircuits.IsHUDVisible = value;
            }
        }

        public static bool IsTickVisible
        {
            get 
            {
                if (IsActive)
                    return TimeCircuits.IsTickVisible;

                return false;
            }
            set
            {
                if (IsActive)
                    TimeCircuits.IsTickVisible = value;
            }
        }

        public static EmptyType Empty
        {
            get 
            {
                if (IsActive)
                    return TimeCircuits.Empty;

                return EmptyType.Hide;
            }
            set
            {
                if (IsActive)
                    TimeCircuits.Empty = value;
            }
        }

        public static int Speed
        {
            get 
            {
                if (IsActive)
                    return TimeCircuits.Speed;

                return 0;
            }
            set
            {
                if (IsActive)
                    TimeCircuits.Speed = value;
            }
        }

        public static void SetOff()
        {
            TimeCircuits?.SetOff();
        }

        public static void SetDate(string type, DateTime date)
        {
            TimeCircuits?.SetDate(type, date);
        }

        public static void SetVisible(string type, bool toggle, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            TimeCircuits?.SetVisible(type, toggle, month, day, year, hour, minute, amPm);
        }
    }
}
