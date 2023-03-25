using FusionLibrary;
using GTA;
using System;

namespace BackToTheFutureV
{
    internal static class WeatherHandler
    {
        private static int _length;
        public static void Register()
        {
            new MomentReplica(new DateTime(1885, 9, 4, 21, 0, 0)) { Weather = Weather.ExtraSunny, MomentDuration = 3660, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 5, 0, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 40, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 5, 46, 0)) { Weather = Weather.Clearing, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 6, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 6, 47, 0)) { Weather = Weather.Clouds, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 55, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 8, 19, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 29, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 13, 5, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 257, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 18, 29, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 67, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 20, 15, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 39, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 5, 21, 1, 0)) { Weather = Weather.Clearing, TransitionWeather = true, MomentDuration = 7, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 6, 10, 38, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 810, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 7, 16, 5, 0)) { Weather = Weather.ExtraSunny, TransitionWeather = true, MomentDuration = 837, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 8, 4, 54, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 52, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 8, 9, 1, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 195, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 8, 13, 33, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 77, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 8, 15, 2, 0)) { Weather = Weather.Clearing, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 12, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 8, 15, 30, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 16, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 8, 22, 46, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 420, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 9, 14, 56, 0)) { Weather = Weather.ExtraSunny, TransitionWeather = true, MomentDuration = 550, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 12, 9, 58, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 598, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 12, 20, 50, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 54, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 12, 21, 55, 0)) { Weather = Weather.ThunderStorm, RainLevel = 0, MomentDuration = 11, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 13, 4, 57, 0)) { Weather = Weather.ThunderStorm, TransitionWeather = true, MomentDuration = 411, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 13, 12, 1, 0)) { Weather = Weather.Clearing, TransitionWeather = true, PuddleLevel = 0.7f, MomentDuration = 13, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 13, 14, 5, 0)) { Weather = Weather.Clouds, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 111, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 14, 8, 1, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 965, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 15, 12, 6, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 720, ResetWanted = false };
            new MomentReplica(new DateTime(1955, 11, 16, 12, 6, 0)) { Weather = Weather.ExtraSunny, TransitionWeather = true, MomentDuration = 720, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 25, 10, 48, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 648, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 25, 22, 44, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 68, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 26, 0, 4, 0)) { Weather = Weather.Clearing, TransitionWeather = true, MomentDuration = 12, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 26, 5, 6, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 290, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 26, 22, 9, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 733, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 27, 11, 34, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 72, ResetWanted = false };
            new MomentReplica(new DateTime(1985, 10, 27, 18, 23, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 337, ResetWanted = false };
            new MomentReplica(new DateTime(2015, 10, 21, 16, 22, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 10, ResetWanted = false };
            new MomentReplica(new DateTime(2015, 10, 21, 18, 2, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 90, ResetWanted = false };
            new MomentReplica(new DateTime(2015, 10, 22, 12, 48, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 48, ResetWanted = false };
            new MomentReplica(new DateTime(2015, 10, 22, 4, 7, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 150, ResetWanted = false };
            new MomentReplica(new DateTime(2015, 10, 22, 7, 0, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 23, ResetWanted = false };
            new MomentReplica(new DateTime(2015, 10, 22, 7, 26, 0)) { Weather = Weather.Clearing, TransitionWeather = true, MomentDuration = 3, ResetWanted = false };

            _length = MomentReplica.MomentReplicas.Count;
        }

        public static void Tick()
        {
            for (int x = 0; x < _length; x++)
            {
                if (GTA.UI.Screen.IsFadingOut)
                {
                    MomentReplica.MomentReplicas[x].Applied = false;
                }
                else if (MomentReplica.MomentReplicas[x].IsNow() && !GTA.UI.Screen.IsFadedOut)
                {
                    MomentReplica.MomentReplicas[x].Apply();
                }
            }
        }
    }
}
