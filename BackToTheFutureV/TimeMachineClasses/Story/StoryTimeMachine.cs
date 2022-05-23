using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.UI;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
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
        public bool Spawned => TimeMachine.IsFunctioning();

        public bool FirstTime { get; private set; } = false;

        public bool IsUsed { get; private set; }
        public bool WarningMessageShowed { get; private set; }

        private float delaySpawn;

        private bool delaySet;

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
            {
                TimeMachine.Mods.Wheel = WheelType.RailroadInvisible;
            }

            if (DestinationTime != default)
            {
                TimeMachine.Properties.DestinationTime = DestinationTime;
            }

            if (PreviousTime != default)
            {
                TimeMachine.Properties.PreviousTime = PreviousTime;
            }

            TimeMachine.Vehicle.IsInvincible = IsInvincible;

            if (SpawnFlags.HasFlag(SpawnFlags.Broken))
            {
                TimeMachine.Vehicle.DirtLevel = 15.0f;
            }

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
            {
                IsUsed = false;
            }

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

                if (FusionUtils.PlayerPed.DistanceToSquared2D(TimeMachine, 4.47f))
                {
                    if (TimeMachine.Properties.Story)
                    {
                        TimeMachineHandler.RemoveStory(TimeMachine);
                    }

                    if (!WarningMessageShowed && TimeMachine.Constants.FullDamaged)
                    {
                        DateTime diff = new DateTime((FusionUtils.CurrentTime - SpawnDate).Ticks);

                        int years = diff.Year - 1;
                        int months = diff.Month - 1;
                        int days = diff.Day - 1;

                        if (years > 1)
                        {
                            string ret = TextHandler.Me.GetLocalizedText("Buried");

                            if (months != 0 && days != 0)
                            {
                                ret = string.Format(ret, $"{years} {TextHandler.Me.GetLocalizedText("Years")}, {months} {TextHandler.Me.GetLocalizedText("Months")} {TextHandler.Me.GetLocalizedText("And")} {days} {TextHandler.Me.GetLocalizedText("Days")}");
                            }
                            else
                            {
                                if (months != 0)
                                {
                                    ret = string.Format(ret, $"{years} {TextHandler.Me.GetLocalizedText("Years")} {TextHandler.Me.GetLocalizedText("And")} {months} {TextHandler.Me.GetLocalizedText("Months")}");
                                }
                                else
                                {
                                    if (days != 0)
                                    {
                                        ret = string.Format(ret, $"{years} {TextHandler.Me.GetLocalizedText("Years")} {TextHandler.Me.GetLocalizedText("And")} {days} {TextHandler.Me.GetLocalizedText("Days")}");
                                    }
                                }
                            }
                            Screen.ShowSubtitle(ret);
                            WarningMessageShowed = true;
                        }
                    }
                }
                else if (!TimeMachine.Properties.Story)
                {
                    TimeMachineHandler.AddStory(TimeMachine);
                }
            }

            if (Spawned && !IsUsed && FusionUtils.PlayerPed.IsInVehicle(TimeMachine))
            {
                TimeMachine.Vehicle.IsInvincible = false;

                IsUsed = true;
                return;
            }

            if (!Exists(FusionUtils.CurrentTime) && Spawned && !IsUsed)
            {
                TimeMachine.Dispose(true);
                return;
            }

            if (Exists(FusionUtils.CurrentTime) && !Spawned && !FirstTime)
            {
                Spawn();
                FirstTime = true;
                return;
            }

            if (Exists(FusionUtils.CurrentTime) && !Spawned && FirstTime)
            {
                if (!delaySet)
                {
                    delaySpawn = Game.GameTime + 30000;
                    delaySet = true;
                }
                if (Game.GameTime > delaySpawn)
                {
                    Spawn();
                    delaySet = false;
                }

            }
        }
    }
}