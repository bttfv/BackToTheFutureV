using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.UI;
using System;
using System.Collections.Generic;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses
{
    public class StoryTimeMachine
    {
        public Vector3 Position { get; }
        public float Heading { get; }
        public WormholeType WormholeType { get; }
        public SpawnFlags SpawnFlags { get; }
        public DateTime SpawnDate { get; }
        public DateTime DeleteDate { get; }
        public bool IsInvincible { get; }
        public DateTime DestinationTime { get; } = default;
        public DateTime PreviousTime { get; } = default;

        public TimeMachine TimeMachine { get; private set; }
        public bool Spawned => TimeMachineHandler.Exists(TimeMachine);
        public bool IsUsed { get; private set; }
        public bool WarningMessageShowed { get; private set; }

        public StoryTimeMachine(Vector3 position, float heading, WormholeType wormholeType, SpawnFlags spawnFlags, DateTime spawnDate, DateTime deleteDate, bool isInvincible = false, DateTime destinationTime = default, DateTime previousTime = default)
        {
            Position = position;
            Heading = heading;
            WormholeType = wormholeType;
            SpawnFlags = spawnFlags;
            SpawnDate = spawnDate;
            DeleteDate = deleteDate;
            IsInvincible = isInvincible;
            DestinationTime = destinationTime;
            PreviousTime = previousTime;
        }

        public TimeMachine Spawn()
        {
            TimeMachine = TimeMachineHandler.Create(SpawnFlags, WormholeType, Position, Heading);

            if (DestinationTime != default)
                TimeMachine.Properties.DestinationTime = DestinationTime;

            if (PreviousTime != default)
                TimeMachine.Properties.PreviousTime = PreviousTime;

            TimeMachine.Vehicle.IsInvincible = IsInvincible;

            if (SpawnFlags.HasFlag(SpawnFlags.Broken))
                TimeMachine.Vehicle.DirtLevel = 15.0f;

            TimeMachineHandler.AddStory(TimeMachine);

            return TimeMachine;
        }

        public bool Exists(DateTime time)
        {
            return time >= SpawnDate && time <= DeleteDate;
        }

        public void Process()
        {
            if (!Spawned && IsUsed)
                IsUsed = false;

            if (Spawned && !IsUsed) 
            {
                if (!WarningMessageShowed && TimeMachine.Properties.FullDamaged && TimeMachine.Vehicle.Position.DistanceToSquared(Utils.PlayerPed.Position) < 20)
                {
                    DateTime diff = new DateTime((Utils.CurrentTime - SpawnDate).Ticks);

                    int years = diff.Year - 1;
                    int months = diff.Month - 1;
                    int days = diff.Day - 1;

                    string ret = Game.GetLocalizedString("BTTFV_Buried_Delorean");

                    if (years != 0 && months != 0 && days != 0)
                        ret = string.Format(ret, $"{years} {Game.GetLocalizedString("BTTFV_Years")}, {months} {Game.GetLocalizedString("BTTFV_Months")} {Game.GetLocalizedString("BTTFV_And_Conjunction")} {days} {Game.GetLocalizedString("BTTFV_Days")}");
                    else
                    {
                        if (years != 0 && months != 0)
                            ret = string.Format(ret, $"{years} {Game.GetLocalizedString("BTTFV_Years")} {Game.GetLocalizedString("BTTFV_And_Conjunction")} {months} {Game.GetLocalizedString("BTTFV_Months")}");
                        else
                        {
                            if (years != 0 && days != 0)
                                ret = string.Format(ret, $"{years} {Game.GetLocalizedString("BTTFV_Years")} {Game.GetLocalizedString("BTTFV_And_Conjunction")} {days} {Game.GetLocalizedString("BTTFV_Days")}");
                            else
                            {
                                if (months != 0 && days != 0)
                                    ret = string.Format(ret, $"{months} {Game.GetLocalizedString("BTTFV_Months")} {Game.GetLocalizedString("BTTFV_And_Conjunction")} {days} {Game.GetLocalizedString("BTTFV_Days")}");
                            }
                        }
                    }

                    Screen.ShowSubtitle(ret);
                    WarningMessageShowed = true;
                }
            }

            if (Spawned && !IsUsed && Utils.PlayerPed.IsInVehicle(TimeMachine))
            {
                TimeMachine.Vehicle.IsInvincible = false;
                TimeMachineHandler.RemoveStory(TimeMachine);
                    
                IsUsed = true;
                return;
            }
                
            if (!Exists(Utils.CurrentTime) && Spawned && !IsUsed)
            {
                TimeMachine.Dispose(true);
                return;
            }

            if (Exists(Utils.CurrentTime) && !Spawned)
                Spawn();
        }

        public static List<StoryTimeMachine> StoryTimeMachines { get; private set; } = new List<StoryTimeMachine>();
        
        static StoryTimeMachine()
        {
            //Inside mine            
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-595.14f, 2085.36f, 130.78f), 13.78f, WormholeType.BTTF2, SpawnFlags.ForcePosition | SpawnFlags.Broken, new DateTime(1885, 9, 1, 0, 0, 1), new DateTime(1955, 11, 15, 23, 59, 59), true, new DateTime(1885, 1, 1, 0, 0, 0), new DateTime(1955, 11, 12, 21, 43, 0)));

            //Parking lot
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-264.22f, -2092.08f, 26.76f), 287.57f, WormholeType.BTTF1, SpawnFlags.ForcePosition, new DateTime(1985, 10, 26, 1, 15, 0), new DateTime(1985, 10, 26, 1, 35, 0), false, new DateTime(1985, 10, 26, 1, 21, 0)));
        }

        public static void ProcessAll()
        {
            foreach(var x in StoryTimeMachines)
                x.Process();
        }

        public static void Abort()
        {
            foreach(var x in StoryTimeMachines)
            {
                if (x.Spawned && !x.IsUsed)
                    x.TimeMachine.Dispose();
            }
        }
    }
}
