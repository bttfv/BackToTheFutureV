using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Story;
using LemonUI;
using Screen = GTA.UI.Screen;
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
using RogersSierraRailway;
using GTA.Math;
using FusionLibrary;

namespace BackToTheFutureV
{
    public class Main : Script
    {        
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
            StoryTimeMachine.Abort();
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
                //Disable fake shake of the cars.
                Function.Call((Hash)0x84FD40F56075E816, 0);

                ModelHandler.RequestModels();

                if (ModSettings.PersistenceSystem)
                {
                    TimeMachineHandler.LoadAllTimeMachines();
                    RemoteTimeMachineHandler.Load();
                }

                Utils.RandomTrains = ModSettings.RandomTrains;

                TimeHandler.TrafficVolumeYearBased = true;

                StartListening();

                _firstTick = false;
            }

            WaybackMachineHandler.Process();
            CustomTrainHandler.Process();
            DMC12Handler.Process();
            TimeMachineHandler.Process();
            RCManager.Process();
            RemoteTimeMachineHandler.Process();
            FireTrailsHandler.Process();            
            TcdEditer.Process();
            MissionHandler.Process();                                    
            StoryTimeMachine.ProcessAll();
            MenuHandler.Process();

            if (ModSettings.PersistenceSystem && _saveDelay < Game.GameTime)
            {
                TimeMachineHandler.SaveAllTimeMachines();

                _saveDelay = Game.GameTime + 2000;
            }                
        }
    }
}