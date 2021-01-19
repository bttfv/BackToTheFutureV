using FusionLibrary;
using System;
using System.Linq;
using System.Text;
using TimeCircuits;

namespace BackToTheFutureV
{
    public static class RemoteHUD
    {
        private const int port = 1985;

        private static bool isHUDVisible = false;
        public static bool IsHUDVisible 
        { 
            get => isHUDVisible;
            set 
            {
                isHUDVisible = value;

                if (ModSettings.NetworkTCDToggle)
                    Network.SendBool("IsHUDVisible", value, port);
            }
        }

        private static bool isTickVisible = false;
        public static bool IsTickVisible
        {
            get => isTickVisible;
            set
            {
                isHUDVisible = value;

                if (ModSettings.NetworkTCDToggle)
                    Network.SendBool("IsTickVisible", value, port);
            }
        }

        private static EmptyType empty = EmptyType.Hide;
        public static EmptyType Empty
        {
            get => empty;
            set
            {
                empty = value;

                if (ModSettings.NetworkTCDToggle)
                    Network.SendInt("Empty", (int)value, port);
            }
        }

        private static int speed = 0;
        public static int Speed 
        {
            get => speed;
            set
            {
                speed = value;

                if (ModSettings.NetworkTCDToggle)
                    Network.SendInt("Speed", value, port);
            }
        }

        public static void SetOff()
        {
            if (ModSettings.NetworkTCDToggle)
                Network.SendMsg("SetOff=1", port);
        }

        public static void SetDate(string type, DateTime date)
        {
            if (ModSettings.NetworkTCDToggle)
                Network.SendMsg($"SetDate={type}|{date}", port);
        }

        public static void SetVisible(string type, bool toggle, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            if (ModSettings.NetworkTCDToggle)
                Network.SendMsg($"SetVisible={type}|{toggle}|{month}|{day}|{year}|{hour}|{minute}|{amPm}", port);
        }

        public static void SetLedState(bool[][] ledState)
        {
            if (!ModSettings.NetworkTCDToggle)
                return;
            
            byte[] ret = new byte[20 * 10];

            int pos = 0;
            
            for (int column = 0; column < 10; column++)
                for (int row = 0; row < 20; row++)
                {
                    ret[pos] = Convert.ToByte(ledState[column][row]);
                    pos++;
                }

            ret = Encoding.ASCII.GetBytes("SetLedState=").Concat(ret).ToArray();

            Network.SendMsg(ret, port);
        }
    }
}
