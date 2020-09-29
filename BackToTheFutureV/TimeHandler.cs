using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Memory;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Native;
using GTA.UI;

namespace BackToTheFutureV
{
    public class TimeHandler
    {
        private static List<Moment> momentsInTime = new List<Moment>();
        private static List<Vehicle> vehiclesEnteredByPlayer = new List<Vehicle>();

        public static void Process()
        {
            if (Main.PlayerVehicle != null && !Main.PlayerVehicle.IsTimeMachine() && !vehiclesEnteredByPlayer.Contains(Main.PlayerVehicle))
                vehiclesEnteredByPlayer.Add(Main.PlayerVehicle);

            float vehDensity = 1;

            int year = Main.CurrentTime.Year;

            if (year > 1900 && year < 1950)
            {
                year -= 1900;

                vehDensity = year / 50;
            }
            else if (year <= 1900)
                vehDensity = 0;

            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, vehDensity);
            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, vehDensity);
            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, vehDensity);
        }

        public static void SetupJump(TimeMachine timeMachine, DateTime time)
        {
            Utils.DeleteNearbyVehPed();

            // Try to find a stored moment for our time jump
            var moment = GetStoredMoment(time, 6);
            if (moment != null)
            {
                // We found a moment.
                // Apply it.
                ApplyMoment(moment);
            }
            else
            {
                // Get the current Moment object for current situation.
                var currentMoment = GetMomentForNow();

                // Clear the entered vehicles list
                vehiclesEnteredByPlayer.Clear();

                // We didn't find a moment.
                // Randomise.
                Randomize();

                // Get the moment AFTER randomizing
                var newMoment = GetMomentForNow();
                newMoment.CurrentDate = timeMachine.Properties.DestinationTime;

                // Add both moments to stored Moments list.
                momentsInTime.Add(moment);
                momentsInTime.Add(newMoment);
            }
          
            TimeMachineHandler.ExistenceCheck(time);

            RemoteTimeMachineHandler.ExistenceCheck(time);

            //RogersSierra.Manager.RogersSierra?.ForEach(x => x?.Delete());
        }

        public static void TimeTravelTo(TimeMachine timeMachine, DateTime time)
        {
            // Sets up the jump to that time period.
            SetupJump(timeMachine, time);

            // Set the new GTA time.
            Utils.SetWorldTime(time);
        }

        public static void Randomize()
        {
            // Set the weather to a random weather
            //World.Weather = Utils.GetRandomWeather();
            Function.Call(Hash.SET_RANDOM_WEATHER_TYPE);

            // Initial puddle level
            float puddleLevel = 0;

            // If the weather is raining
            if (World.Weather == Weather.Raining)
            {
                // Set the puddle to a random number between 0.4 and 0.8
                puddleLevel = (float)Utils.Random.NextDouble(0.4, 0.8);
            }
            // If the weather is clearing
            else if (World.Weather == Weather.Clearing)
            {
                // Set the puddle to 0.2
                puddleLevel = 0.2f;
            }
            // If the weather is a thunderstorm
            else if (World.Weather == Weather.ThunderStorm)
            {
                // Set the puddle to 0.9f
                puddleLevel = 0.9f;
            }

            // Apply the puddle level
            RainPuddleEditor.Level = puddleLevel;

            // Reset wanted level
            Game.Player.WantedLevel = 0;
        }

        public static Moment GetMomentForNow()
        {
            // Get current information
            var currentTime = Utils.GetWorldTime();
            var currentWeather = World.Weather;
            var currentPuddleLevel = RainPuddleEditor.Level;
            var currentWantedLevel = Game.Player.WantedLevel;
            var infos = vehiclesEnteredByPlayer
                .Where(x => !x.IsTimeMachine() && x.Driver == null) // needs to be stationary
                .Select(x => new VehicleInfo(x));

            // Return a new Moment instance with all the above information
            return new Moment(currentTime, currentWeather, currentPuddleLevel, currentWantedLevel, infos);
        }

        public static void ApplyMoment(Moment moment)
        {
            //World.GetNearbyVehicles(Main.PlayerPed.Position, 50f)
            //    .Where(x => !x.IsTimeMachine())
            //    .ToList()
            //    .ForEach(x => x.DeleteCompletely());

            foreach (var vehicleInfo in moment.Vehicles)
            {
                Utils.SpawnFromVehicleInfo(vehicleInfo);
            }

            if (World.Weather != moment.Weather)
                World.Weather = moment.Weather;

            RainPuddleEditor.Level = moment.PuddleLevel;

            //Game.Player.WantedLevel = moment.WantedLevel;

            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player, moment.WantedLevel, false);
            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, Game.Player, false);
        }

        public static Moment GetStoredMoment(DateTime currentTime, int maxHoursRange)
        {
            Moment foundMoment = null;

            foreach (var moment in momentsInTime)
            {
                if (moment == null) continue;

                var momentDate = moment.CurrentDate;
                
                if(currentTime >= momentDate && currentTime <= (momentDate + new TimeSpan(maxHoursRange, 0, 0)))
                {
                    foundMoment = moment;
                    break;
                }
            }

            return foundMoment;
        }
    }
}
