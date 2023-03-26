using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using GTA.UI;
using System;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class TimeParadox : Script
    {
        private WaybackRecord StartRecord;
        private ModsPrimitive StartMods;
        private WaybackRecord LastRecord;

        public static bool ParadoxInProgress { get; private set; }
        private int gameTime;

        public TimeParadox()
        {
            Tick += TimeParadox_Tick;
            //KeyDown += TimeParadox_KeyDown;
        }

        /*private void TimeParadox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode != System.Windows.Forms.Keys.E)
                return;

            StartParadox();
        }*/

        public void TimeParadox_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading || Main.FirstTick || Game.IsMissionActive)
                return;

            if (StartRecord == null)
                StartRecord = new WaybackRecord(FusionUtils.PlayerPed);

            if (FusionUtils.PlayerVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.IsTimeMachine() && TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle).Mods.IsDMC12)
                StartMods = TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle).Mods.Clone();

            if (!ModSettings.WaybackSystem || !ModSettings.TimeParadox)
                return;

            if (ParadoxInProgress)
            {
                Process();
                return;
            }

            if (FusionUtils.PlayerPed.Model == StartRecord.Ped.Replica.Model)
                LastRecord = new WaybackRecord(FusionUtils.PlayerPed);

            foreach (WaybackMachine waybackMachine in WaybackSystem.CurrentReplaying)
            {
                if ((waybackMachine.Ped == null || waybackMachine.Ped.Model != StartRecord.Ped.Replica.Model || waybackMachine.Ped.IsAlive) && !(waybackMachine.Ped.LastVehicle.NotNullAndExists() && waybackMachine.Ped.LastVehicle.IsConsideredDestroyed))
                    continue;

                StartParadox();
            }
        }

        private void StartParadox()
        {
            ParadoxInProgress = true;
            Game.Player.CanControlCharacter = false;

            if (FusionUtils.PlayerVehicle.NotNullAndExists() && FusionUtils.PlayerVehicle.IsTimeMachine() && TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled)
            {
                RemoteTimeMachineHandler.StopRemoteControl();
            }

            if (LastRecord.Ped.Replica.Model != FusionUtils.PlayerPed.Model)
            {
                LastRecord.Ped.Replica.Position.LoadScene();

                Ped newPed = LastRecord.Spawn(LastRecord);

                LastRecord.Apply(newPed, LastRecord);

                PlayerSwitch.OnSwitchingComplete += KillPlayer;
                PlayerSwitch.Switch(newPed, true, true);

                return;
            }

            KillPlayer();
        }

        private void KillPlayer()
        {
            PlayerSwitch.OnSwitchingComplete -= KillPlayer;

            if (FusionUtils.PlayerPed.IsInVehicle() && !(FusionUtils.PlayerVehicle.IsInAir || FusionUtils.PlayerVehicle.IsInWater))
            {
                if (FusionUtils.PlayerVehicle.Speed > 5)
                {
                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, FusionUtils.PlayerPed, FusionUtils.PlayerVehicle, FusionEnums.DriveAction.BrakeStrong, 5000);
                    while (FusionUtils.PlayerVehicle.Speed > 5)
                        Yield();
                }

                FusionUtils.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);

                while (FusionUtils.PlayerPed.IsInVehicle())
                    Yield();
            }

            FusionUtils.PlayerPed.Kill();

            Game.TimeScale = 0.4f;
            Screen.FadeOut(6000);

            Process();
        }

        private void Process()
        {
            Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "respawn_controller");

            Function.Call(Hash.IGNORE_NEXT_RESTART, true);
            Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, true);
            Function.Call(Hash.SET_NO_LOADING_SCREEN, true);
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);

            if (gameTime > Game.GameTime)
                return;

            if (FusionUtils.PlayerPed.DecreaseAlpha() > AlphaLevel.L0)
            {
                gameTime = Game.GameTime + 350;
                return;
            }

            if (Screen.IsFadingOut)
                return;

            IntroHandler.Me.Start(true, StartRecord.Time);

            WaybackSystem.Abort();
            TimeMachineHandler.RemoveAllTimeMachines();
            RemoteTimeMachineHandler.DeleteAll();

            TimeHandler.TimeTravelTo(StartRecord.Time);
            MomentReplica.MomentReplicas.Clear();
            WeatherHandler.Register();

            Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, FusionUtils.PlayerPed);
            Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, FusionUtils.PlayerPed.Position.X, FusionUtils.PlayerPed.Position.Y, FusionUtils.PlayerPed.Position.Z, FusionUtils.PlayerPed.Heading, false, false);
            Function.Call(Hash.FORCE_GAME_STATE_PLAYING);

            StartRecord.Ped.Replica.Position.LoadScene();

            Ped oldPed = FusionUtils.PlayerPed;
            Ped newPed = StartRecord.Spawn(StartRecord);

            StartRecord.Apply(newPed, StartRecord);

            PlayerSwitch.Switch(newPed, true, true);

            oldPed.Delete();

            if (StartRecord.Vehicle != null && StartRecord.Vehicle.IsTimeMachine)
            {
                SpawnFlags flag;
                bool isDMC12 = false;

                if (StartRecord.Vehicle.Replica.Model == ModelHandler.DMC12 || StartRecord.Vehicle.Replica.Model == ModelHandler.DMCDebugModel)
                {
                    flag = SpawnFlags.NoVelocity | SpawnFlags.NoMods;
                    isDMC12 = true;
                }
                else
                {
                    flag = SpawnFlags.NoVelocity;
                }
                TimeMachine timeMachine = StartRecord.Vehicle.Replica.Spawn(flag).TransformIntoTimeMachine();

                if (isDMC12)
                    StartMods.ApplyTo(timeMachine);

                StartRecord.Vehicle.Properties.ApplyTo(timeMachine);
                FusionUtils.PlayerPed.SetIntoVehicle(timeMachine.Vehicle, StartRecord.Ped.Replica.Seat);
            }

            Game.Player.CanControlCharacter = true;

            ParadoxInProgress = false;
        }
    }
}
