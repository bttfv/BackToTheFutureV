using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal static class JVTHandler
    {
        public static List<JVT> TimeTrains { get; } = new List<JVT>();
        private static List<JVT> _timeTrainsToAdd = new List<JVT>();
        private static List<JVT> _timeTrainsToRemove = new List<JVT>();

        public static JVT CurrentJVT { get; private set; }
        public static JVT ClosestJVT { get; private set; }
        public static float ClosestJVTDistance { get; private set; } = -1;

        public static void Tick()
        {
            if (_timeTrainsToAdd.Count > 0)
            {
                TimeTrains.AddRange(_timeTrainsToAdd);
                _timeTrainsToAdd.Clear();
            }

            if (_timeTrainsToRemove.Count > 0)
            {
                foreach (JVT jvt in _timeTrainsToRemove)
                {
                    jvt.Dispose();
                    TimeTrains.Remove(jvt);
                }

                _timeTrainsToRemove.Clear();
            }

            UpdateClosestJVT();

            TimeTrains.ForEach(x => x.Tick());
        }

        public static void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.O && FusionUtils.PlayerVehicle == null)
                Create(new Vector3(2611, 1681, 27), false, true);

            CurrentJVT?.KeyDown(e);
        }

        public static JVT Create(Vector3 position, bool direction, bool warpInPlayer = false)
        {
            JVT jvt = new JVT(position, direction);

            if (warpInPlayer)
                FusionUtils.PlayerPed.Task.WarpIntoVehicle(jvt.Train, VehicleSeat.Driver);

            return jvt;
        }

        public static void Add(JVT jvt)
        {
            if (_timeTrainsToAdd.Contains(jvt) || TimeTrains.Contains(jvt))
                return;

            _timeTrainsToAdd.Add(jvt);
        }

        public static void Abort()
        {
            TimeTrains.ForEach(x => x.Dispose());
            _timeTrainsToAdd.ForEach(x => x.Dispose());
        }

        public static void UpdateClosestJVT()
        {
            if (TimeTrains.Count == 0)
            {
                if (ClosestJVTDistance != -1)
                {
                    CurrentJVT = null;
                    ClosestJVT = null;
                    ClosestJVTDistance = -1;
                }

                return;
            }

            if (CurrentJVT != null && !FusionUtils.PlayerVehicle.IsFunctioning())
                CurrentJVT = null;

            foreach (JVT jvt in TimeTrains)
            {
                if (!jvt.Train.IsFunctioning())
                    continue;

                if (jvt.Train == FusionUtils.PlayerVehicle)
                {
                    CurrentJVT = jvt;

                    if (ClosestJVT != CurrentJVT)
                    {
                        ClosestJVT = CurrentJVT;
                        ClosestJVTDistance = 0;
                    }

                    return;
                }

                float dist = FusionUtils.PlayerPed.DistanceToSquared2D(jvt.Train);

                if (ClosestJVT == jvt)
                    ClosestJVTDistance = dist;

                if (ClosestJVT != jvt && (ClosestJVTDistance == -1 || dist < ClosestJVTDistance))
                {
                    ClosestJVT = jvt;
                    ClosestJVTDistance = dist;
                }
            }
        }
    }
}
