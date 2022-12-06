using FusionLibrary;
using GTA;
using GTA.Native;
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

        public static bool WaybackStarted { get; private set; } = false;

        public static DateTime ResetDate { get; private set; }

        public static PedReplica ResetPed { get; private set; }

        public static VehicleReplica ResetVehicle { get; private set; }
        public static string ResetVehiclePlate { get; private set; }
        public static LicensePlateStyle ResetVehiclePlateStyle { get; private set; }

        public static bool IsTimeMachine { get; private set; } = false;

        public static ModsPrimitive Mods { get; private set; }

        public static PropertiesHandler Properties { get; private set; }

        public static DateTime NewGame { get; } = new DateTime(2003, 12, 15, 5, 0, 0);

        public static bool TutorialMission { get; private set; }

        public static bool StoryMode { get; private set; }

        public static int SwitchedPed { get; private set; }

        public static int? SwitchedVehicle { get; set; }

        public static bool SwitchedPersist { get; set; }

        public static CustomStopwatch CustomStopwatch { get; } = new CustomStopwatch();

        public Main()
        {
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);

            System.IO.File.AppendAllText($"./ScriptHookVDotNet.log", $"BackToTheFutureV - {Version} ({buildDate})" + Environment.NewLine);

            ModSettings.LoadSettings();

            if (ModSettings.PersistenceSystem)
            {
                InstantiateScript<PersistenceScript>();
            }

            if (ModSettings.WaybackSystem)
            {
                InstantiateScript<WaybackSystem>();
                WaybackStarted = true;
            }

            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void TextIntro()
        {
            Game.Player.CanControlCharacter = false;
            DateTime openingDate = FusionUtils.CurrentTime;
            AudioPlayer timeParadox = CommonAudioEngine.Create("story/bttf_subtitle2.wav", Presets.No3D);
            timeParadox.SourceEntity = FusionUtils.PlayerPed;
            timeParadox.Volume = 0.2f;
            timeParadox.Play();
            TimeText.DisplayText(openingDate);
            Game.Player.CanControlCharacter = true;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            World.RenderingCamera = null;

            Screen.FadeIn(1000);

            if (RemoteTimeMachineHandler.IsRemoteOn)
            {
                RemoteTimeMachineHandler.StopRemoteControl(true);
            }

            if (ModSettings.PersistenceSystem)
            {
                TimeMachineHandler.Save();
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
            if (Game.Version >= GameVersion.v1_0_2372_0_Steam)
            {
                if (Game.IsLoading || FusionUtils.FirstTick)
                {
                    LoadingPrompt.Show("Loading BTTFV");
                    Screen.FadeOut(0);
                    return;
                }

                if (FirstTick)
                {
                    if (FusionUtils.CurrentTime == NewGame && Game.IsMissionActive)
                    {
                        LoadingPrompt.Hide();
                        Screen.FadeIn(0);
                        TutorialMission = true;
                        StoryMode = true;
                    }
                    else
                    {
                        ResetPed = new PedReplica(FusionUtils.PlayerPed);
                        SwitchedPed = FusionUtils.PlayerPed.Handle;
                    }

                    if (!TutorialMission)
                    {
                        //Screen.ShowHelpText("BackToTheFutureV loading...", -1, true, true);

                        RemoteTimeMachineHandler.MAX_REMOTE_TIMEMACHINES = ModSettings.MaxRecordedMachines;

                        while (!ModelHandler.RequestModels())
                        {
                            LoadingPrompt.Show("Loading BTTFV");
                        }
                        LoadingPrompt.Hide();

                        //Disable fake shake of the cars.
                        Function.Call(Hash._SET_CAR_HIGH_SPEED_BUMP_SEVERITY_MULTIPLIER, 0);

                        if (ModSettings.PersistenceSystem)
                        {
                            TimeMachineHandler.Load();
                            RemoteTimeMachineHandler.Load();
                        }

                        FusionUtils.RandomTrains = ModSettings.RandomTrains;
                        TimeHandler.RealTime = ModSettings.RealTime;
                        TimeHandler.TrafficVolumeYearBased = ModSettings.YearTraffic;

                        DecoratorsHandler.Register();
                        WeatherHandler.Register();
                    }
                }

                if (!FirstTick)
                {
                    if (Game.IsMissionActive && !StoryMode)
                    {
                        StoryMode = true;
                    }

                    if (!StoryMode)
                    {
                        if (ModSettings.WaybackSystem && !WaybackStarted)
                        {
                            InstantiateScript<WaybackSystem>();
                            WaybackStarted = true;
                        }
                        else if (ModSettings.WaybackSystem && WaybackStarted)
                        {
                            WaybackSystem.Tick();
                        }
                        if (ModSettings.TimeParadox)
                        {
                            if (PlayerSwitch.IsInProgress && FusionUtils.PlayerPed.Model == ResetPed.Model)
                            {
                                SwitchedPed = FusionUtils.PlayerPed.Handle;
                            }
                            else if (FusionUtils.PlayerPed.Model != ResetPed.Model)
                            {
                                if (!Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, SwitchedPed))
                                {
                                    Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, SwitchedPed, 1, 1);
                                }
                            }

                            if (FusionUtils.PlayerPed.Model == ResetPed.Model && FusionUtils.PlayerPed.IsSittingInVehicle() && SwitchedVehicle == null)
                            {
                                SwitchedVehicle = FusionUtils.PlayerVehicle.Handle;
                            }
                            else if (FusionUtils.PlayerPed.Model == ResetPed.Model && !FusionUtils.PlayerPed.IsSittingInVehicle() && SwitchedVehicle != null && FusionUtils.PlayerPed.LastVehicle != null)
                            {
                                if (!FusionUtils.PlayerPed.LastVehicle.IsTimeMachine() && SwitchedPersist)
                                {
                                    FusionUtils.PlayerPed.LastVehicle.IsPersistent = false;
                                    SwitchedPersist = false;
                                }
                                SwitchedVehicle = null;
                            }
                            else if (FusionUtils.PlayerPed.Model == ResetPed.Model && !FusionUtils.PlayerPed.IsSittingInVehicle() && SwitchedVehicle != null && FusionUtils.PlayerPed.LastVehicle == null)
                            {
                                SwitchedVehicle = null;
                            }
                            else if (FusionUtils.PlayerPed.Model != ResetPed.Model && SwitchedVehicle != null)
                            {
                                if (!Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, SwitchedVehicle))
                                {
                                    Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, SwitchedVehicle, 1, 1);
                                    SwitchedPersist = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Function.Call<bool>(Hash.IS_MISSION_COMPLETE_PLAYING))
                        {
                            if (TutorialMission)
                            {
                                TutorialMission = false;
                                StoryMode = false;
                                FirstTick = true;
                                return;
                            }
                            StoryMode = false;
                        }
                    }
                }

                if (!TutorialMission)
                {
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
                }

                if (FirstTick)
                {
                    if (!StoryMode)
                    {
                        ResetDate = FusionUtils.CurrentTime;
                        if (WaybackStarted)
                        {
                            WaybackSystem.Tick();
                        }
                    }
                    if (TutorialMission)
                    {
                        FirstTick = false;
                        return;
                    }

                    TrafficHandler.ModelSwaps.Add(new ModelSwap
                    {
                        Enabled = true,
                        Model = ModelHandler.DMC12,
                        VehicleType = VehicleType.Automobile,
                        VehicleClass = VehicleClass.Sports,
                        DateBased = true,
                        StartProductionDate = new DateTime(1981, 1, 21, 0, 0, 0),
                        EndProductionDate = new DateTime(1982, 12, 24, 23, 59, 59),
                        MaxInWorld = 25,
                        MaxSpawned = 3,
                        WaitBetweenSpawns = 10000
                    });

                    if (FusionUtils.PlayerPed.IsInVehicle() && !StoryMode)
                    {
                        ResetVehicle = new VehicleReplica(FusionUtils.PlayerVehicle);
                        ResetVehiclePlate = FusionUtils.PlayerVehicle.Mods.LicensePlate;
                        ResetVehiclePlateStyle = FusionUtils.PlayerVehicle.Mods.LicensePlateStyle;
                        if (FusionUtils.PlayerVehicle.IsTimeMachine())
                        {
                            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle);
                            IsTimeMachine = true;
                            Mods = timeMachine.Mods.Clone();
                            Properties = timeMachine.Properties.Clone();
                        }
                    }

                    //Screen.ShowHelpText("BackToTheFutureV loaded correctly.");
                    if (Screen.IsFadedOut)
                    {
                        TextIntro();
                    }
                    //For some reason applying impulse doesn't work while faded out, so try again
                    foreach (TimeMachine x in TimeMachineHandler.TimeMachines)
                    {
                        x.Vehicle.Velocity += GTA.Math.Vector3.UnitY * 0.3f;
                    }
                    FirstTick = false;
                }
            }
            else
            {
                Screen.ShowHelpText("~r~ERROR:~s~ ~y~BackToTheFutureV~s~ is ~o~not~s~ compatible with the currently installed version of the GTAV.~n~Please ~b~update~s~ your game to version ~g~2372~s~ or newer.", 100);
            }
        }
    }
}
