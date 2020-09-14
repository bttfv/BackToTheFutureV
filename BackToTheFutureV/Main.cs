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
        public List<Vehicle> modelList = new List<Vehicle>();

        public static MenuPool MenuPool { get; private set; }

        public static DateTime CurrentTime
        {
            get => Utils.GetWorldTime();
            set => Utils.SetWorldTime(value);
        }

        public static Ped PlayerPed => Game.Player.Character;

        public static Vehicle PlayerVehicle => PlayerPed.CurrentVehicle;

        public static cRogersSierra RogersSierra => Manager.CurrentRogersSierra;

        public static bool IsPlayerSwitchInProgress => Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS);

        public static bool IsManualPlayerSwitchInProgress => IsPlayerSwitchInProgress && PlayerSwitch.IsSwitching;

        public static bool DisablePlayerSwitching = false;

        public static bool HideGui = false;

        public static bool GamePaused = false;

        public static AudioEngine CommonAudioEngine = new AudioEngine() { BaseSoundFolder = "BackToTheFutureV\\Sounds" };

        public static CustomStopwatch CustomStopwatch = new CustomStopwatch();

        private bool _firstTick = true;        

        private readonly UdpClient udp = new UdpClient(1985);

        private IAsyncResult ar_ = null;

        private int _saveDelay;

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
            ar_ = udp.BeginReceive(Receive, new object());
        }

        public Main()
        {
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

            TimeMachineHandler.Abort();            
            FireTrailsHandler.Stop();
            TrainHandler.Abort();
            ModSettings.SaveSettings();            
        }

        private unsafe void Main_KeyDown(object sender, KeyEventArgs e)
        {
            ModMenuHandler.KeyDown(e);
            TimeMachineHandler.KeyDown(e.KeyCode);
            RCManager.KeyPress(e.KeyCode);
        }

        private unsafe void Main_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading)
                return;

            GamePaused = false;

            if (_firstTick)
            {                
                ModelHandler.RequestModels();

                TimeMachineHandler.LoadAllTimeMachines();
                RemoteTimeMachineHandler.Load();

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
            AnimatePropsHandler.Tick();
            RCManager.Process();
            TimeHandler.Tick();
            RemoteTimeMachineHandler.Tick();
            FireTrailsHandler.Process();
            InteractionMenuManager.Process();
            ScreenFlash.Process();
            TcdEditer.Tick();
            MissionHandler.Process();                        
            PlayerSwitch.Process();            

            //if (Game.GameTime > _saveDelay)
            //{
            TimeMachineHandler.SaveAllTimeMachines();
            //    _saveDelay = Game.GameTime + 2000;
            //}
        }
    }
}