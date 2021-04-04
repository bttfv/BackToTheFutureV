using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using KlangRageAudioLibrary;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.Enums;

namespace BackToTheFutureV
{
    internal class RemoteTimeMachine
    {
        public TimeMachineClone TimeMachineClone { get; }
        public TimeMachine TimeMachine { get; private set; }
        public Blip Blip { get; set; }
        public bool Spawned => TimeMachineHandler.Exists(TimeMachine);

        private int _timer;
        private bool _hasPlayedWarningSound;

        private static AudioPlayer WarningSound;

        public Wayback WaybackMachine { get; }

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
                TimeMachine = null;

            if (Game.GameTime < _timer)
                return;

            if (!Spawned && Utils.CurrentTime.Near(TimeMachineClone.Properties.DestinationTime, new TimeSpan(0, 1, 0), true))
            {
                if (!_hasPlayedWarningSound)
                {
                    if (!Utils.PlayerPed.IsInVehicle())
                        Utils.PlayerPed.Task.PlayAnimation("amb@code_human_wander_idles@male@idle_a", "idle_a_wristwatch", 8f, -1, AnimationFlags.UpperBodyOnly);

                    WarningSound.SourceEntity = Utils.PlayerPed;
                    WarningSound.Play();

                    _hasPlayedWarningSound = true;
                }
            }

            if (!Spawned && Utils.CurrentTime.Near(TimeMachineClone.Properties.DestinationTime, new TimeSpan(0, 0, 5), true))
            {
                Spawn(ReenterType.Normal);

                _hasPlayedWarningSound = false;
                _timer = Game.GameTime + 3000;
            }
        }

        public TimeMachine Spawn(ReenterType reenterType)
        {
            if (Blip != null && Blip.Exists())
                Blip.Delete();

            switch (reenterType)
            {
                case ReenterType.Forced:
                case ReenterType.Normal:
                    SpawnFlags spawnFlags = SpawnFlags.ForceReentry;

                    if (ModSettings.WaybackSystem)
                        spawnFlags |= SpawnFlags.NoOccupants;

                    TimeMachine = TimeMachineClone.Spawn(spawnFlags);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    return TimeMachine;
                case ReenterType.Spawn:
                    spawnFlags = SpawnFlags.NoVelocity;

                    if (ModSettings.WaybackSystem)
                        spawnFlags |= SpawnFlags.NoOccupants;

                    TimeMachine = TimeMachineClone.Spawn(spawnFlags);
                    TimeMachine.LastDisplacementClone = TimeMachineClone;

                    if (!TimeMachine.Properties.HasBeenStruckByLightning && TimeMachine.Mods.IsDMC12)
                        TimeMachine.Properties.ReactorCharge--;

                    TimeMachine.Events.OnVehicleSpawned?.Invoke();
                    return TimeMachine;
            }

            return null;
        }

        public void ExistenceCheck(DateTime time)
        {
            if (TimeMachineClone.Properties.DestinationTime < time && !Spawned)
                Spawn(ReenterType.Spawn);
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