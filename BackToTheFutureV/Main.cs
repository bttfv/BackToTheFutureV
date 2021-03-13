using BackToTheFutureV.Menu;
using BackToTheFutureV.Players;
using BackToTheFutureV.Settings;
using BackToTheFutureV.Story;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Native;
using KlangRageAudioLibrary;
using System;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV
{
    internal class Main : Script
    {
        public static Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public static AudioEngine CommonAudioEngine { get; set; } = new AudioEngine() { BaseSoundFolder = "BackToTheFutureV\\Sounds" };

        public static bool FirstTick { get; private set; } = true;

        public static CustomStopwatch CustomStopwatch { get; } = new CustomStopwatch();

        public Main()
        {
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);

            System.IO.File.AppendAllText($"./ScriptHookVDotNet.log", $"BackToTheFutureV - {Version} ({buildDate})" + Environment.NewLine);

            ModSettings.LoadSettings();

            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            World.RenderingCamera = null;

            Screen.FadeIn(1000);

            if (RemoteTimeMachineHandler.IsRemoteOn)
                RemoteTimeMachineHandler.StopRemoteControl(true);

            if (ModSettings.PersistenceSystem)
                TimeMachineHandler.SaveAllTimeMachines();

            WaybackMachineHandler.Abort();
            MissionHandler.Abort();
            StoryTimeMachineHandler.Abort();
            RemoteTimeMachineHandler.Abort();
            TimeMachineHandler.Abort();
            FireTrailsHandler.Abort();
            CustomTrainHandler.Abort();
            DMC12Handler.Abort();

            ExternalHUD.Stop();
        }

        private unsafe void Main_KeyDown(object sender, KeyEventArgs e)
        {
            TimeMachineHandler.KeyDown(e);
            MissionHandler.KeyDown(e);
            MenuHandler.KeyDown(e);
        }

        private unsafe void Main_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading)
                return;

            if (FirstTick)
            {
                ModelHandler.RequestModels();

                //Disable fake shake of the cars.
                Function.Call((Hash)0x84FD40F56075E816, 0);

                if (ModSettings.PersistenceSystem)
                {
                    TimeMachineHandler.LoadAllTimeMachines();
                    RemoteTimeMachineHandler.Load();
                }

                Utils.RandomTrains = ModSettings.RandomTrains;

                if (ModSettings.ExternalTCDToggle)
                    ExternalHUD.Toggle(true);

                ExternalHUD.SetOff();

                FirstTick = false;
            }

            if (ModSettings.ExternalTCDToggle != ExternalHUD.IsActive)
                ExternalHUD.Toggle(ModSettings.ExternalTCDToggle);

            TrashHandler.Tick();
            CustomTrainHandler.Tick();
            DMC12Handler.Tick();
            TimeMachineHandler.Tick();
            RemoteTimeMachineHandler.Tick();
            FireTrailsHandler.Tick();
            TcdEditer.Tick();
            RCGUIEditer.Tick();
            MissionHandler.Tick();
            StoryTimeMachineHandler.Tick();
            MenuHandler.Tick();
        }
    }
}