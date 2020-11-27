using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;

namespace FusionLibrary
{
    public delegate void OnTimeChanged(DateTime time);
    public delegate void OnDayNightChange();

    public class TimeHandler
    {
        public static List<Vehicle> UsedVehiclesByPlayer = new List<Vehicle>();
        private static List<Vehicle> RemoveUsedVehicle = new List<Vehicle>();

        public static OnTimeChanged OnTimeChanged;
        public static OnDayNightChange OnDayNightChange;

        public static bool IsNight { get; internal set; }

        public static bool TrafficVolumeYearBased { get; set; }

        internal static void Process()
        {
            UsedVehiclesByPlayer.ForEach(x =>
            {
                if (!x.IsFunctioning() || x.IsTimeMachine2())
                    RemoveUsedVehicle.Add(x);
            });

            if (RemoveUsedVehicle.Count > 0)
            {
                RemoveUsedVehicle.ForEach(x => UsedVehiclesByPlayer.Remove(x));
                RemoveUsedVehicle.Clear();
            }

            if (Utils.PlayerVehicle.IsFunctioning() && !Utils.PlayerVehicle.IsTimeMachine2() && !Utils.PlayerVehicle.IsTrain() && !UsedVehiclesByPlayer.Contains(Utils.PlayerVehicle))
                UsedVehiclesByPlayer.Add(Utils.PlayerVehicle);

            bool isNight = Utils.CurrentTime.Hour >= 20 || (Utils.CurrentTime.Hour >= 0 && Utils.CurrentTime.Hour <= 5);

            if (isNight != IsNight)
            {
                IsNight = isNight;
                OnDayNightChange?.Invoke();
            }

            if (!TrafficVolumeYearBased)
                return;

            float vehDensity = 1;

            float year = Utils.CurrentTime.Year;

            if (year > 1900 && year < 1950)
            {
                year -= 1900;

                vehDensity = year / 50f;
            }
            else if (year <= 1900)
                vehDensity = 0;

            if (vehDensity == 1)
                return;

            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, vehDensity);
            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, vehDensity);
            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, vehDensity);
        }

        public static void TimeTravelTo(DateTime destinationTime)
        {
            MomentReplica momentReplica = MomentReplica.SearchForMoment();

            if (momentReplica == null)
                MomentReplica.MomentReplicas.Add(new MomentReplica());
            else
                momentReplica.Update();

            Utils.ClearWorld();

            Utils.CurrentTime = destinationTime;

            UsedVehiclesByPlayer.Clear();

            momentReplica = MomentReplica.SearchForMoment();

            if (momentReplica == null)
                MomentReplica.Randomize();
            else
                momentReplica.Apply();

            OnTimeChanged?.Invoke(destinationTime);
        }
    }
}
