using FusionLibrary;

namespace BackToTheFutureV
{
    public static class TextHandler
    {
        private static CustomText CustomText = new CustomText("BTTFV_Text_{0}");

        public static string GetLocalizedText(string entry)
        {
            return CustomText.GetLocalizedText(entry);
        }

        public static string[] GetLocalizedText(params string[] entries)
        {
            return CustomText.GetLocalizedText(entries);
        }

        public static void ShowSubtitle(string entry, int duration = 2500)
        {
            CustomText.ShowSubtitle(entry, duration);
        }

        public static void ShowSubtitle(string entry, int duration = 2500, params string[] values)
        {
            CustomText.ShowSubtitle(entry, duration, values);
        }

        public static void ShowHelp(string entry, bool beep = true)
        {
            CustomText.ShowHelp(entry, beep);
        }

        public static void ShowHelp(string entry, bool beep, params object[] values)
        {
            CustomText.ShowHelp(entry, beep, values);
        }

        public static void ShowNotification(string entry, bool blinking = false)
        {
            CustomText.ShowNotification(entry, blinking);
        }

        public static void ShowNotification(string entry, bool blinking, params string[] values)
        {
            CustomText.ShowNotification(entry, blinking, values);
        }

        public static string GetOnOff(bool value)
        {
            return value ? GetLocalizedText("On") : GetLocalizedText("Off");
        }        
    }
}
