using FusionLibrary;
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
        public TimeMachine TimeMachine { get; private set; }
        public Blip Blip { get; set; }
        public bool Spawned => TimeMachine.IsFunctioning() && !TimeMachine.Properties.PlayerUsed;

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
            if (!Spawned && TimeMachine != null)
            {
                TimeMachine = null;
            }

            if (Spawned || Game.GameTime < _timer)
            {
                return;
            }

            if ((!TimeHandler.RealTime && TimeMachineClone.Properties.DestinationTime.Between(FusionUtils.CurrentTime.AddSeconds(-45), FusionUtils.CurrentTime)) || (TimeHandler.RealTime && FusionUtils.CurrentTime == TimeMachineClone.Properties.DestinationTime.AddSeconds(-3)))
            {
                if (!_hasPlayedWarningSound && !TimeMachineClone.Properties.IsWayback && !TimeMachineClone.Properties.HasBeenStruckByLightning)
                {
                    if (!FusionUtils.PlayerPed.IsInVehicle())
                    {
                        FusionUtils.PlayerPed.Task.PlayAnimation("amb@code_human_wander_idles@male@idle_a", "idle_a_wristwatch", 8f, -1, AnimationFlags.UpperBodyOnly);
                    }

                    WarningSound.SourceEntity = FusionUtils.PlayerPed;
                    WarningSound.Play();

                    _hasPlayedWarningSound = true;
                }
            }

            if ((!TimeHandler.RealTime && !TimeMachineClone.Properties.IsWayback && TimeMachineClone.Properties.DestinationTime.Between(FusionUtils.CurrentTime.AddSeconds(-30), FusionUtils.CurrentTime)) || (TimeHandler.RealTime && !TimeMachineClone.Properties.IsWayback && FusionUtils.CurrentTime == TimeMachineClone.Properties.DestinationTime.AddSeconds(-2)))
            {
                Spawn(ReenterType.Normal);

                _hasPlayedWarningSound = false;
                _timer = Game.GameTime + 10000;
            }
            else if ((!TimeHandler.RealTime && TimeMachineClone.Properties.IsWayback && TimeMachineClone.Properties.DestinationTime.Between(FusionUtils.CurrentTime.AddSeconds(-30), FusionUtils.CurrentTime)) || (TimeHandler.RealTime && TimeMachineClone.Properties.IsWayback && FusionUtils.CurrentTime == TimeMachineClone.Properties.DestinationTime.AddSeconds(-1)))
            {
                Spawn(ReenterType.Normal);
            }
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
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.ForceReentry);
                    TimeMachine.Properties.DestinationTime = TimeMachineClone.Properties.DestinationTime;
                    //TimeMachine.LastDisplacementClone = TimeMachineClone;

                    break;
                case ReenterType.Forced:
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.ForceReentry | SpawnFlags.NoOccupants);
                    //TimeMachine.LastDisplacementClone = TimeMachineClone;

                    break;
                case ReenterType.Spawn:
                    TimeMachine = TimeMachineClone.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants);
                    //TimeMachine.LastDisplacementClone = TimeMachineClone;

                    if (!TimeMachine.Properties.HasBeenStruckByLightning && TimeMachine.Mods.IsDMC12)
                    {
                        TimeMachine.Properties.ReactorCharge--;
                    }

                    if (TimeMachine.Properties.HasBeenStruckByLightning && TimeMachine.Mods.IsDMC12 && TimeMachine.Properties.IsFlying)
                    {
                        TimeMachine.Properties.AreTimeCircuitsOn = false;
                        TimeMachine.Properties.AreFlyingCircuitsBroken = true;
                        TimeMachine.Properties.AreTimeCircuitsBroken = true;
                        TimeMachine.Properties.HasBeenStruckByLightning = false;
                    }

                    TimeMachine.Events.OnVehicleSpawned?.Invoke();
                    break;
            }

            TimeMachine.Vehicle.SetPlayerLights(true);

            return TimeMachine;
        }

        public void ExistenceCheck(DateTime time)
        {
            if (TimeMachineClone.Properties.DestinationTime < time && !TimeMachineClone.Properties.IsWayback && !Spawned)
            {
                Spawn(ReenterType.Spawn);
            }
        }

        public void Dispose()
        {
            Blip?.Delete();
        }

        public override string ToString()
        {
            return RemoteTimeMachineHandler.RemoteTimeMachines.IndexOf(this).ToString();
        }
    }
}