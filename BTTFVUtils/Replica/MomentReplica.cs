using FusionLibrary.Memory;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using static FusionLibrary.Enums;

namespace FusionLibrary
{
    public class MomentReplica
    {
        public static List<MomentReplica> MomentReplicas = new List<MomentReplica>();

        public Weather Weather { get; set; }
        public int WantedLevel { get; set; }
        public float PuddleLevel { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<VehicleReplica> VehicleReplicas { get; set; }

        public MomentReplica()
        {
            Update();
        }

        public static MomentReplica SearchForMoment()
        {
            foreach (MomentReplica momentReplica in MomentReplicas)
                if (momentReplica.IsNow())
                    return momentReplica;

            return null;
        }

        public bool IsNow()
        {
            if (CurrentDate > Utils.CurrentTime.AddHours(6) && CurrentDate < Utils.CurrentTime.AddHours(-6))
                return false;

            return true;
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
                //puddleLevel = (float)Utils.Random.NextDouble(0.4, 0.8);
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

            MomentReplicas.Add(new MomentReplica());
        }

        public void Apply()
        {
            World.Weather = Weather;
            RainPuddleEditor.Level = PuddleLevel;

            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL, Game.Player, WantedLevel, false);
            Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW, Game.Player, false);

            VehicleReplicas.ForEach(x => TimeHandler.UsedVehiclesByPlayer.Add(x.Spawn(SpawnFlags.Default)));
        }

        public void Update()
        {
            CurrentDate = Utils.GetWorldTime();
            Weather = World.Weather;
            PuddleLevel = RainPuddleEditor.Level;
            WantedLevel = Game.Player.WantedLevel;

            VehicleReplicas = new List<VehicleReplica>();

            TimeHandler.UsedVehiclesByPlayer.ForEach(x => VehicleReplicas.Add(new VehicleReplica(x)));
        }
    }
}
