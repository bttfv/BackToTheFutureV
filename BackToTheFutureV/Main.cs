﻿using FusionLibrary;
using GTA;
using GTA.Chrono;
using GTA.UI;
using KlangRageAudioLibrary;
using System;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV
{
    [ScriptAttributes(Author = "BTTFV Team", SupportURL = "https://discord.gg/MGpmPhSDYR")]
    internal class Main : Script
    {
        public static Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        public static AudioEngine CommonAudioEngine { get; set; } = new AudioEngine() { BaseSoundFolder = "BackToTheFutureV\\Sounds" };

        public static bool FirstTick { get; private set; } = true;

        public static CustomStopwatch CustomStopwatch { get; } = new CustomStopwatch();

        public static GameClockDateTime NewGameTime { get; } = new GameClockDateTime(GameClockDate.FromYmd(2003, 12, 15), GameClockTime.FromHms(5, 0, 0));

        public static bool FirstMission { get; private set; }

        //public static bool DeluxoProtoSupport { get; set; } = false;

        public static void Log(string message)
        {
            System.IO.File.AppendAllText($"./ScriptHookVDotNet.log", $"BackToTheFutureV - {message}" + Environment.NewLine);
        }

        public Main()
        {
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);

            Log($"{Version} ({buildDate})");

            ModSettings.LoadSettings();

            /*if (ModSettings.DeluxoProto && new Model("dproto").IsInCdImage)
            {
                DeluxoProtoSupport = true;
            }*/

            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            ScriptCameraDirector.StopRendering();
            LoadingPrompt.Hide();
            Screen.FadeIn(1000);

            if (RemoteTimeMachineHandler.IsRemoteOn)
            {
                RemoteTimeMachineHandler.StopRemoteControl(true);
            }

            GarageHandler.Abort();
            MissionHandler.Abort();
            StoryTimeMachineHandler.Abort();
            RemoteTimeMachineHandler.Abort();
            TimeMachineHandler.Abort();
            FireTrailsHandler.Abort();
            CustomTrainHandler.Abort();
            DMC12Handler.Abort();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            TimeMachineHandler.KeyDown(e);
            MissionHandler.KeyDown(e);
            MenuHandler.KeyDown(e);
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading || FusionUtils.FirstTick)
            {
                LoadingPrompt.Show("Loading BTTFV");
                Screen.FadeOut(0);
                return;
            }

            if (!new Model("deluxo").IsInCdImage && !FirstMission)
            {
                Screen.FadeIn(0);
                LoadingPrompt.Hide();
                Screen.ShowHelpText("~r~ERROR:~s~ ~y~BackToTheFutureV~s~ is ~o~not~s~ compatible with the currently installed version of the GTAV.~n~Please ~b~update~s~ your game to version ~g~2372~s~ or newer.");
                FirstMission = true;
                return;
            }

            if (FirstTick && GameClock.Now == NewGameTime && Game.IsMissionActive)
            {
                LoadingPrompt.Hide();
                Screen.FadeIn(0);
                FirstMission = true;
            }

            if (FirstMission && Game.IsMissionActive)
            {
                return;
            }
            else if (FirstMission)
            {
                FirstMission = false;
            }

            if (FirstTick)
            {
                while (!ModelHandler.RequestModels())
                {
                    LoadingPrompt.Show("Loading BTTFV");
                }

                //Disable fake shake of the cars.
                World.VehicleHighSpeedBumpMultiplier(0f);

                FusionUtils.RandomTrains = ModSettings.RandomTrains;
                TimeHandler.RealTime = ModSettings.RealTime;
                TimeHandler.TrafficVolumeYearBased = ModSettings.YearTraffic;

                DecoratorsHandler.Register();
                WeatherHandler.Register();

                InstantiateScript<IntroHandler>();
                InstantiateScript<TimeParadox>();

                TrafficHandler.ModelSwaps.Add(new ModelSwap
                {
                    Enabled = true,
                    Model = ModelHandler.DMC12,
                    VehicleType = VehicleType.Automobile,
                    VehicleClass = VehicleClass.SportsClassics,
                    DateBased = true,
                    StartProductionDate = new DateTime(1981, 1, 21, 0, 0, 0),
                    EndProductionDate = new DateTime(1982, 12, 24, 23, 59, 59),
                    MaxInWorld = 25,
                    MaxSpawned = 3,
                    WaitBetweenSpawns = 180000,
                    ChanceOfSpawn = 0.5f
                });
            }

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
            TrashHandler.Tick();
            GarageHandler.Tick();
            WeatherHandler.Tick();
            WaybackSystem.Tick();

            if (FirstTick)
            {
                if (ModSettings.HoverAllCars)
                {
                    InstantiateScript<HoverMode>();
                }

                TimeMachine temp = TimeMachineHandler.Create(position: new GTA.Math.Vector3(0, 0, 2400));
                while (!temp.IsReady)
                {
                    LoadingPrompt.Show("Loading BTTFV");
                }
                temp.Dispose();
                LoadingPrompt.Hide();
                IntroHandler.Me.Start(true);
                FirstTick = false;
            }
        }
    }
}
