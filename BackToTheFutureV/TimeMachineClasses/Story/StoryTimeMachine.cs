using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.UI;
using System;
using static BackToTheFutureV.Utility.InternalEnums;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses
{
    internal class StoryTimeMachine
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
        public bool IsOnTracks { get; }

        public TimeMachine TimeMachine { get; private set; }
        public bool Spawned => TimeMachineHandler.Exists(TimeMachine);
        public bool IsUsed { get; private set; }
        public bool WarningMessageShowed { get; private set; }

        public StoryTimeMachine(Vector3 position, float heading, WormholeType wormholeType, SpawnFlags spawnFlags, DateTime spawnDate, DateTime deleteDate, bool isInvincible = false, DateTime destinationTime = default, DateTime previousTime = default, bool isOnTracks = false)
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
            IsOnTracks = isOnTracks;
        }

        public TimeMachine Spawn()
        {
            TimeMachine = TimeMachineHandler.Create(SpawnFlags, WormholeType, Position, Heading);

            if (IsOnTracks)
                TimeMachine.Mods.Wheel = WheelType.RailroadInvisible;

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

        public void Tick()
        {
            if (!Spawned && IsUsed)
                IsUsed = false;

            if (Spawned && !IsUsed)
            {
                if (TimeMachine.Mods.IsDMC12)
                {
                    VehicleWindowCollection windows = TimeMachine.Vehicle.Windows;
                    windows[VehicleWindowIndex.BackLeftWindow].Remove();
                    windows[VehicleWindowIndex.BackRightWindow].Remove();
                    windows[VehicleWindowIndex.ExtraWindow4].Remove();

                    TimeMachine.Vehicle.Doors[VehicleDoorIndex.Trunk].Break(false);
                    TimeMachine.Vehicle.Doors[VehicleDoorIndex.BackRightDoor].Break(false);
                }

                if (Utils.PlayerPed.DistanceToSquared2D(TimeMachine, 4.47f))
                {
                    if (TimeMachine.Properties.Story)
                        TimeMachineHandler.RemoveStory(TimeMachine);

                    if (!WarningMessageShowed && TimeMachine.Constants.FullDamaged)
                    {
                        DateTime diff = new DateTime((Utils.CurrentTime - SpawnDate).Ticks);

                        int years = diff.Year - 1;
                        int months = diff.Month - 1;
                        int days = diff.Day - 1;

                        string ret = TextHandler.GetLocalizedText("Buried");

                        if (years != 0 && months != 0 && days != 0)
                            ret = string.Format(ret, $"{years} {TextHandler.GetLocalizedText("Years")}, {months} {TextHandler.GetLocalizedText("Months")} {TextHandler.GetLocalizedText("And")} {days} {TextHandler.GetLocalizedText("Days")}");
                        else
                        {
                            if (years != 0 && months != 0)
                                ret = string.Format(ret, $"{years} {TextHandler.GetLocalizedText("Years")} {TextHandler.GetLocalizedText("And")} {months} {TextHandler.GetLocalizedText("Months")}");
                            else
                            {
                                if (years != 0 && days != 0)
                                    ret = string.Format(ret, $"{years} {TextHandler.GetLocalizedText("Years")} {TextHandler.GetLocalizedText("And")} {days} {TextHandler.GetLocalizedText("Days")}");
                                else
                                {
                                    if (months != 0 && days != 0)
                                        ret = string.Format(ret, $"{months} {TextHandler.GetLocalizedText("Months")} {TextHandler.GetLocalizedText("And")} {days} {TextHandler.GetLocalizedText("Days")}");
                                }
                            }
                        }

                        Screen.ShowSubtitle(ret);
                        WarningMessageShowed = true;
                    }
                }
                else if (!TimeMachine.Properties.Story)
                    TimeMachineHandler.AddStory(TimeMachine);
            }

            if (Spawned && !IsUsed && Utils.PlayerPed.IsInVehicle(TimeMachine))
            {
                TimeMachine.Vehicle.IsInvincible = false;

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
    }
}