using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Story;
using LemonUI;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Settings;
using System.Net.Sockets;
using System.Net;
using System.Text;
using KlangRageAudioLibrary;
//using RogersSierra;
using BackToTheFutureV.Vehicles;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Players;
using System.Collections.Generic;
using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.Menu;

namespace BackToTheFutureV
{
    public class Main : Script
    {
        public static ObjectPool ObjectPool { get; private set; }
        public static DateTime CurrentTime
        {
            get => Utils.GetWorldTime();
            set => Utils.SetWorldTime(value);
        }
        public static Ped PlayerPed => Game.Player.Character;
        public static Vehicle PlayerVehicle => PlayerPed.CurrentVehicle;
        //public static cRogersSierra CurrentRogersSierra => Manager.CurrentRogersSierra;
        //public static List<cRogersSierra> RogersSierra => Manager.RogersSierra;
        public static bool IsPlayerSwitchInProgress => Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS);
        public static bool IsManualPlayerSwitchInProgress => IsPlayerSwitchInProgress && PlayerSwitch.IsSwitching;
        public static bool DisablePlayerSwitching { get; set; } = false;
        public static bool HideGui { get; set; } = false;
        public static Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public static AudioEngine CommonAudioEngine { get; set; } = new AudioEngine() { BaseSoundFolder = "BackToTheFutureV\\Sounds" };

        private bool _firstTick = true;
        private int _saveDelay;
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
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Version.Build).AddSeconds(Version.Revision * 2);

            System.IO.File.AppendAllText($"./ScriptHookVDotNet.log", $"BackToTheFutureV - {Version} ({buildDate})" + Environment.NewLine);

            ObjectPool = new ObjectPool();

            ModSettings.LoadSettings();
            
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            World.RenderingCamera = null;

            Screen.FadeIn(1000);

            if (RCManager.IsRemoteOn)
                RCManager.StopRemoteControl(true);

            if (ModSettings.PersistenceSystem)
                TimeMachineHandler.SaveAllTimeMachines();

            MissionHandler.Abort();
            RemoteTimeMachineHandler.Dispose();
            TimeMachineHandler.Abort();
            FireTrailsHandler.Stop();
            CustomTrainHandler.Abort();
            DMC12Handler.Abort();
        }

        private unsafe void Main_KeyDown(object sender, KeyEventArgs e)
        {
            TimeMachineHandler.KeyDown(e.KeyCode);
            MissionHandler.KeyDown(e);
            MenuHandler.KeyDown(e);
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

                Function.Call(Hash.SET_RANDOM_TRAINS, ModSettings.RandomTrains);

                if (!ModSettings.RandomTrains)
                    Function.Call(Hash.DELETE_ALL_TRAINS);

                StartListening();

                _firstTick = false;
            }

            ObjectPool.Process();

            if (HideGui)
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);

            if (DisablePlayerSwitching)
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 19, true);

            CustomTrainHandler.Process();
            DMC12Handler.Process();
            TimeMachineHandler.Process();
            AnimatePropsHandler.Process();
            RCManager.Process();
            TimeHandler.Process();
            RemoteTimeMachineHandler.Process();
            FireTrailsHandler.Process();
            ScreenFlash.Process();
            TcdEditer.Process();
            MissionHandler.Process();                        
            PlayerSwitch.Process();
            MenuHandler.Process();

            if (ModSettings.PersistenceSystem && _saveDelay < Game.GameTime)
            {
                TimeMachineHandler.SaveAllTimeMachines();

                _saveDelay = Game.GameTime + 2000;
            }                
        }
    }
}