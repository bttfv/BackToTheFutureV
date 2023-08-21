﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using KlangRageAudioLibrary;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class RemoteTimeMachine
    {
        public TimeMachineClone TimeMachineClone { get; }
        public TimeMachine TimeMachine => TimeMachineHandler.GetTimeMachineFromGUID(TimeMachineClone.Properties.GUID);
        public Blip Blip { get; set; }
        public bool Spawned => TimeMachine.NotNullAndExists();

        private int _timer;
        private bool _hasPlayedWarningSound;

        private static readonly AudioPlayer WarningSound;

        static RemoteTimeMachine()
        {
            WarningSound = Main.CommonAudioEngine.Create("general/rc/warning.wav", Presets.No3D);
        }

        public RemoteTimeMachine(TimeMachineClone timeMachineClone)
        {
            TimeMachineClone = timeMachineClone;

            TimeMachineClone.Properties.TimeTravelPhase = TimeTravelPhase.Completed;

            _timer = Game.GameTime + 3000;
        }

        public void Tick()
        {
            if (Spawned || Game.GameTime < _timer || TimeParadox.ParadoxInProgress)
            {
                return;
            }

            if (!TimeMachineClone.Properties.HasBeenStruckByLightning && !TimeMachineClone.Properties.IsWayback)
            {
                if (!_hasPlayedWarningSound && ((!TimeHandler.RealTime && IsTimeBeforeSeconds(60)) || (TimeHandler.RealTime && IsTimeBeforeMilliseconds(2500))))
                {
                    if (!FusionUtils.PlayerPed.IsInVehicle())
                    {
                        FusionUtils.PlayerPed?.Task?.PlayAnimation("amb@code_human_wander_idles@male@idle_a", "idle_a_wristwatch", 8f, -1, AnimationFlags.UpperBodyOnly);
                    }

                    WarningSound.SourceEntity = FusionUtils.PlayerPed;
                    WarningSound.Play();

                    _hasPlayedWarningSound = true;
                }
            }

            if ((!TimeHandler.RealTime && IsTimeBeforeSeconds(37)) || (TimeHandler.RealTime && IsTimeBeforeMilliseconds(1250)))
            {
                Spawn(ReenterType.Normal);

                _hasPlayedWarningSound = false;
                _timer = Game.GameTime + 10000;
            }
        }

        private bool IsTimeBeforeSeconds(int value)
        {
            return FusionUtils.CurrentTime.Between(TimeMachineClone.Properties.DestinationTime.AddSeconds(-value), TimeMachineClone.Properties.DestinationTime);
        }

        private bool IsTimeBeforeMilliseconds(int value)
        {
            return FusionUtils.CurrentTime.Between(TimeMachineClone.Properties.DestinationTime.AddMilliseconds(-value), TimeMachineClone.Properties.DestinationTime);
        }

        public TimeMachine Spawn(ReenterType reenterType)
        {
            if (Blip != null && Blip.Exists())
            {
                Blip.Delete();
            }

            switch (reenterType)
            {
                case ReenterType.Normal:
                    TimeMachineClone.Spawn(SpawnFlags.ForceReentry);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    break;
                case ReenterType.Spawn:
                    TimeMachineClone timeMachineClone = TimeMachineClone;

                    bool spawnElsewhere = WaybackSystem.GetWaybackMachineFromGUID(timeMachineClone.Properties.GUID) == null && FusionUtils.CurrentTime >= TimeMachineClone.Properties.DestinationTime.AddMinutes(10);

                    if (spawnElsewhere)
                    {
                        timeMachineClone.Vehicle.Position = timeMachineClone.Vehicle.Position.Around(FusionUtils.Random.Next(0, 1000));
                        timeMachineClone.Vehicle.Position.RequestCollision();
                    }

                    timeMachineClone.Spawn(SpawnFlags.NoVelocity);

                    if (spawnElsewhere)
                    {
                        TimeMachine.Vehicle.PlaceOnNextStreet();
                        TimeMachine.Vehicle.IsEngineRunning = false;
                    }

                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    if (TimeMachine.Mods.IsDMC12)
                    {
                        if (TimeMachine.Properties.HasBeenStruckByLightning)
                        {
                            TimeMachine.Properties.AreTimeCircuitsOn = false;
                            TimeMachine.Properties.AreTimeCircuitsBroken = true;
                            TimeMachine.Properties.HasBeenStruckByLightning = false;
                            TimeMachine.Properties.AreFlyingCircuitsBroken = TimeMachine.Properties.IsFlying;
                        }
                        else
                        {
                            TimeMachine.Properties.ReactorCharge--;
                        }
                    }

                    TimeMachine.Events.OnVehicleSpawned?.Invoke();
                    break;
            }

            TimeMachine.Vehicle.SetPlayerLights(true);

            return TimeMachine;
        }

        public void ExistenceCheck(DateTime time)
        {
            if (time.Between(TimeMachineClone.Properties.DestinationTime.AddMinutes(1), TimeMachineClone.ExistsUntil) && !Spawned && !TimeMachineClone.Properties.PlayerUsed)
            {
                Spawn(ReenterType.Spawn);
            }
        }

        public void Dispose()
        {
            Blip?.Delete();
            WarningSound?.Dispose();
        }

        public override string ToString()
        {
            return RemoteTimeMachineHandler.RemoteTimeMachines.IndexOf(this).ToString();
        }
    }
}
