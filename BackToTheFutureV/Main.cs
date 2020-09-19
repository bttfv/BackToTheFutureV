using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Story;
using NativeUI;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.Entities;
using BackToTheFutureV.InteractionMenu;
using BackToTheFutureV.Settings;
using System.Net.Sockets;
using System.Net;
using System.Text;
using KlangRageAudioLibrary;
using RogersSierra;
using BackToTheFutureV.Vehicles;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Players;
using System.Collections.Generic;
using BackToTheFutureV.TimeMachineClasses.Handlers;

namespace BackToTheFutureV
{
    public class Main : Script
    {
        public static MenuPool MenuPool { get; private set; }
        public static DateTime CurrentTime
        {
            get => Utils.GetWorldTime();
            set => Utils.SetWorldTime(value);
        }
        public static Ped PlayerPed => Game.Player.Character;
        public static Vehicle PlayerVehicle => PlayerPed.CurrentVehicle;
        public static cRogersSierra CurrentRogersSierra => Manager.CurrentRogersSierra;
        public static List<cRogersSierra> RogersSierra => Manager.RogersSierra;
        public static bool IsPlayerSwitchInProgress => Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS);
        public static bool IsManualPlayerSwitchInProgress => IsPlayerSwitchInProgress && PlayerSwitch.IsSwitching;
        public static bool DisablePlayerSwitching { get; set; } = false;
        public static bool HideGui { get; set; } = false;
        public static AudioEngine CommonAudioEngine { get; set; } = new AudioEngine() { BaseSoundFolder = "BackToTheFutureV\\Sounds" };

        private bool _firstTick = true;                
        private readonly UdpClient udp = new UdpClient(1985);

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 1985);

            string message = Encoding.ASCII.GetString(udp.EndReceive(ar, ref ip));

            if (message.StartsWith("BTTFV="))
            {
                message = message.Replace("BTTFV=", "");

                if (message == "enter")
                    InputHandler.EnterInputBuffer = true;
                else
                    InputHandler.InputBuffer = message;
            }

            StartListening();
        }

        private void StartListening()
        {
            udp.BeginReceive(Receive, new object());
        }

        public Main()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                    .AddDays(version.Build).AddSeconds(version.Revision * 2);

            System.IO.File.AppendAllText($"./ScriptHookVDotNet.log", $"BackToTheFutureV - {version} ({buildDate})" + Environment.NewLine);

            MenuPool = new MenuPool();

            ModSettings.LoadSettings();            
            InteractionMenuManager.Init();
            ModMenuHandler.Initialize();
            
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            World.RenderingCamera = null;

            Screen.FadeIn(1000);

            if (RCManager.RemoteControlling != null)
                RCManager.StopRemoteControl(true);

            if (ModSettings.PersistenceSystem)
                TimeMachineHandler.SaveAllTimeMachines();

            TimeMachineHandler.Abort();            
            FireTrailsHandler.Stop();
            TrainHandler.Abort();           
        }

        private unsafe void Main_KeyDown(object sender, KeyEventArgs e)
        {
            ModMenuHandler.KeyDown(e);
            TimeMachineHandler.KeyDown(e.KeyCode);
            RCManager.KeyDown(e.KeyCode);

            if (e.KeyCode == Keys.L)
                TimeMachineHandler.CurrentTimeMachine?.Events.StartTimeTravel?.Invoke();
        }

        private unsafe void Main_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading)
                return;

            if (_firstTick)
            {
                ModelHandler.RequestModels();

                if (ModSettings.PersistenceSystem)
                {
                    TimeMachineHandler.LoadAllTimeMachines();
                    RemoteTimeMachineHandler.Load();
                }
                
                StartListening();

                _firstTick = false;
            }

            if (MenuPool != null && MenuPool.IsAnyMenuOpen())
                MenuPool.ProcessMenus();

            if (HideGui)
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);

            if (DisablePlayerSwitching)
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 19, true);

            TrainHandler.Process();
            DMC12Handler.Process();
            TimeMachineHandler.Process();
            AnimatePropsHandler.Process();
            RCManager.Process();
            TimeHandler.Process();
            RemoteTimeMachineHandler.Process();
            FireTrailsHandler.Process();
            InteractionMenuManager.Process();
            ScreenFlash.Process();
            TcdEditer.Process();
            MissionHandler.Process();                        
            PlayerSwitch.Process();
        }
    }
}