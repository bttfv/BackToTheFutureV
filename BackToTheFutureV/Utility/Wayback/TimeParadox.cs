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
        private WaybackRecord LastRecord;

        public static bool ParadoxInProgress { get; private set; }
        private int gameTime;

        public TimeParadox()
        {
            Tick += TimeParadox_Tick;
            //KeyDown += TimeParadox_KeyDown;
        }

        //private void TimeParadox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.KeyCode != System.Windows.Forms.Keys.E)
        //        return;

        //    StartParadox();
        //}

        public void TimeParadox_Tick(object sender, EventArgs e)
        {
            if (ParadoxInProgress)
            {
                Process();
                return;
            }

            if (Game.IsLoading || Main.FirstTick || !ModSettings.WaybackSystem || !ModSettings.TimeParadox)
                return;

            if (StartRecord == null)
                StartRecord = new WaybackRecord(FusionUtils.PlayerPed);

            if (FusionUtils.PlayerPed.Model == StartRecord.Ped.Replica.Model)
                LastRecord = new WaybackRecord(FusionUtils.PlayerPed);

            foreach (WaybackMachine waybackMachine in WaybackSystem.CurrentReplaying)
            {
                if (waybackMachine.Ped == null || waybackMachine.Ped.Model != StartRecord.Ped.Replica.Model || waybackMachine.Ped.IsAlive)
                    continue;

                StartParadox();
            }
        }

        private void StartParadox()
        {
            ParadoxInProgress = true;

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

            if (FusionUtils.PlayerPed.IsInVehicle())
            {
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

            Function.Call(Hash.NETWORK_REQUEST_CONTROL_OF_ENTITY, FusionUtils.PlayerPed);
            Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, FusionUtils.PlayerPed.Position.X, FusionUtils.PlayerPed.Position.Y, FusionUtils.PlayerPed.Position.Z, FusionUtils.PlayerPed.Heading, false, false);
            Function.Call(Hash.FORCE_GAME_STATE_PLAYING);

            StartRecord.Ped.Replica.Position.LoadScene();

            Ped oldPed = FusionUtils.PlayerPed;
            Ped newPed = StartRecord.Spawn(StartRecord);

            StartRecord.Apply(newPed, StartRecord);

            PlayerSwitch.Switch(newPed, true, true);

            oldPed.Delete();

            ParadoxInProgress = false;
        }
    }
}
