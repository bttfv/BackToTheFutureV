using FusionLibrary;
using GTA;
using System;

namespace BackToTheFutureV
{
    internal static class WeatherHandler
    {
        private static MomentReplica Storm1955;
        private static MomentReplica Storm1955WithRain;
        private static MomentReplica Rain2015;
        private static MomentReplica Clear2015;

        public static void Register()
        {
            Storm1955 = new MomentReplica(new DateTime(1955, 11, 12, 21, 54, 0)) { Weather = Weather.ThunderStorm, RainLevel = 0, LockWeather = true };
            Storm1955WithRain = new MomentReplica(new DateTime(1955, 11, 12, 22, 16, 0)) { Weather = Weather.ThunderStorm, LockWeather = true };
            Rain2015 = new MomentReplica(new DateTime(2015, 10, 21, 16, 29, 0)) { Weather = Weather.Raining, LockWeather = true };
            Clear2015 = new MomentReplica(new DateTime(2015, 10, 21, 16, 50, 0)) { Weather = Weather.Clear, LockWeather = true };
        }

        public static void Tick()
        {
            if (Storm1955.IsNow() && (World.Weather != Weather.ThunderStorm || FusionUtils.RainLevel != 0))
            {
                World.Weather = Weather.ThunderStorm;
                FusionUtils.RainLevel = 0;
            }

            if (Storm1955WithRain.IsNow() && (World.Weather != Weather.ThunderStorm || FusionUtils.RainLevel == 0))
            {
                World.TransitionToWeather(Weather.ThunderStorm, 2);
                FusionUtils.RainLevel = -1;
            }

            if (Rain2015.IsNow() && World.Weather != Weather.Raining)
            {
                World.TransitionToWeather(Weather.Raining, 2);
            }

            if (Clear2015.IsNow() && World.Weather != Weather.Clear)
            {
                World.TransitionToWeather(Weather.Clear, 2);
            }
        }
    }
}
