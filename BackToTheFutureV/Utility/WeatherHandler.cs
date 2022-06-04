using FusionLibrary;
using GTA;
using GTA.Native;
using System;

namespace BackToTheFutureV
{
    internal static class WeatherHandler
    {
        public static void Register()
        {
            new MomentReplica(new DateTime(1885, 9, 4, 21, 0, 0)) { Weather = Weather.ExtraSunny, MomentDuration = 3660 };
            new MomentReplica(new DateTime(1955, 11, 5, 5, 0, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 40 };
            new MomentReplica(new DateTime(1955, 11, 5, 5, 46, 0)) { Weather = Weather.Clearing, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 6 };
            new MomentReplica(new DateTime(1955, 11, 5, 6, 47, 0)) { Weather = Weather.Clouds, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 55 };
            new MomentReplica(new DateTime(1955, 11, 5, 8, 19, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 29 };
            new MomentReplica(new DateTime(1955, 11, 5, 13, 5, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 257 };
            new MomentReplica(new DateTime(1955, 11, 5, 18, 29, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 67 };
            new MomentReplica(new DateTime(1955, 11, 5, 20, 15, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 39 };
            new MomentReplica(new DateTime(1955, 11, 5, 21, 1, 0)) { Weather = Weather.Clearing, TransitionWeather = true, MomentDuration = 7 };
            new MomentReplica(new DateTime(1955, 11, 6, 10, 38, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 810 };
            new MomentReplica(new DateTime(1955, 11, 7, 16, 5, 0)) { Weather = Weather.ExtraSunny, TransitionWeather = true, MomentDuration = 837 };
            new MomentReplica(new DateTime(1955, 11, 8, 4, 54, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 52 };
            new MomentReplica(new DateTime(1955, 11, 8, 9, 1, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 195 };
            new MomentReplica(new DateTime(1955, 11, 8, 13, 33, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 77 };
            new MomentReplica(new DateTime(1955, 11, 8, 15, 2, 0)) { Weather = Weather.Clearing, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 12 };
            new MomentReplica(new DateTime(1955, 11, 8, 15, 30, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 16 };
            new MomentReplica(new DateTime(1955, 11, 8, 22, 46, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 420 };
            new MomentReplica(new DateTime(1955, 11, 9, 14, 56, 0)) { Weather = Weather.ExtraSunny, TransitionWeather = true, MomentDuration = 550 };
            new MomentReplica(new DateTime(1955, 11, 12, 9, 58, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 598 };
            new MomentReplica(new DateTime(1955, 11, 12, 20, 50, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 54 };
            new MomentReplica(new DateTime(1955, 11, 12, 21, 55, 0)) { Weather = Weather.ThunderStorm, RainLevel = 0, MomentDuration = 11 };
            new MomentReplica(new DateTime(1955, 11, 13, 4, 57, 0)) { Weather = Weather.ThunderStorm, TransitionWeather = true, MomentDuration = 411 };
            new MomentReplica(new DateTime(1955, 11, 13, 12, 1, 0)) { Weather = Weather.Clearing, TransitionWeather = true, PuddleLevel = 0.7f, MomentDuration = 13 };
            new MomentReplica(new DateTime(1955, 11, 13, 14, 5, 0)) { Weather = Weather.Clouds, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 111 };
            new MomentReplica(new DateTime(1955, 11, 14, 8, 1, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 965 };
            new MomentReplica(new DateTime(1955, 11, 15, 12, 6, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 720 };
            new MomentReplica(new DateTime(1955, 11, 16, 12, 6, 0)) { Weather = Weather.ExtraSunny, TransitionWeather = true, MomentDuration = 720 };
            new MomentReplica(new DateTime(1985, 10, 25, 10, 48, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 648 };
            new MomentReplica(new DateTime(1985, 10, 25, 22, 44, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 68 };
            new MomentReplica(new DateTime(1985, 10, 26, 0, 4, 0)) { Weather = Weather.Clearing, TransitionWeather = true, MomentDuration = 12 };
            new MomentReplica(new DateTime(1985, 10, 26, 5, 6, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 290 };
            new MomentReplica(new DateTime(1985, 10, 26, 22, 9, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 733 };
            new MomentReplica(new DateTime(1985, 10, 27, 11, 34, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 72 };
            new MomentReplica(new DateTime(1985, 10, 27, 18, 23, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 337 };
            new MomentReplica(new DateTime(2015, 10, 21, 16, 22, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 10 };
            new MomentReplica(new DateTime(2015, 10, 21, 18, 2, 0)) { Weather = Weather.Clear, TransitionWeather = true, PuddleLevel = 0.5f, MomentDuration = 90 };
            new MomentReplica(new DateTime(2015, 10, 22, 12, 48, 0)) { Weather = Weather.Clear, TransitionWeather = true, MomentDuration = 48 };
            new MomentReplica(new DateTime(2015, 10, 22, 4, 7, 0)) { Weather = Weather.Clouds, TransitionWeather = true, MomentDuration = 150 };
            new MomentReplica(new DateTime(2015, 10, 22, 7, 0, 0)) { Weather = Weather.Raining, TransitionWeather = true, MomentDuration = 23 };
            new MomentReplica(new DateTime(2015, 10, 22, 7, 26, 0)) { Weather = Weather.Clearing, TransitionWeather = true, MomentDuration = 3 };
        }

        private static void ClearPersistent()
        {
            Function.Call(Hash.CLEAR_OVERRIDE_WEATHER);
            Function.Call(Hash.CLEAR_WEATHER_TYPE_PERSIST);
        }

        private static void SetPersistent()
        {
            Function.Call(Hash.SET_WEATHER_TYPE_PERSIST, World.Weather.ToString());
        }

        public static void Tick()
        {
            if (MomentReplica.SearchForMoment() == null)
            {
                ClearPersistent();
            }
            else
            {
                for (int x = 0; x <= 39; x++)
                {
                    if (MomentReplica.MomentReplicas[x].IsNow())
                    {
                        MomentReplica.MomentReplicas[x].Apply();
                        SetPersistent();
                    }
                }
            }
        }
    }
}
