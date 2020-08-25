using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using GTA;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Delorean;
using BackToTheFutureV.Settings;
using GTA.NaturalMotion;

namespace BackToTheFutureV.InteractionMenu
{
    public class InteractionMenuManager
    {
        public static PhotoMenu PhotoMenu { get; private set; }

        public static StatisticsMenu StatisticsMenu { get; private set; }

        public static RCMenu RCMenu { get; private set; }

        public static TimeMachineMenu TimeMachineMenu { get; private set; }

        public static SpawnMenu SpawnMenu { get; private set; }

        public static SpawnMenu SpawnMenuContext { get; private set; }

        public static PresetsMenu PresetsMenu { get; private set; }

        public static TrainMissionMenu TrainMissionMenu { get; private set; }

        public static UIMenu CurrentlyOpen { get; private set; }

        public static bool MenuOpen { get; private set; }
        
        public static void Init()
        {
            // Build the initial menu           
            Main.MenuPool.Add(PhotoMenu = new PhotoMenu());
            Main.MenuPool.Add(StatisticsMenu = new StatisticsMenu());
            Main.MenuPool.Add(RCMenu = new RCMenu());
            Main.MenuPool.Add(SpawnMenu = new SpawnMenu());
            Main.MenuPool.Add(SpawnMenuContext = new SpawnMenu());
            Main.MenuPool.Add(PresetsMenu = new PresetsMenu());
            Main.MenuPool.Add(TimeMachineMenu = new TimeMachineMenu());
            Main.MenuPool.Add(TrainMissionMenu = new TrainMissionMenu());
        }

        public static void Process()
        {
            if (Game.IsControlPressed(Control.CharacterWheel) && Game.IsControlPressed(Control.VehicleHandbrake) && !Main.MenuPool.IsAnyMenuOpen() && !TcdEditer.IsEditing)
            {
                if (DeloreanHandler.CurrentTimeMachine != null)
                {
                    if (DeloreanHandler.CurrentTimeMachine.Circuits.IsTimeTraveling || DeloreanHandler.CurrentTimeMachine.Circuits.IsReentering)
                        return;
                }

                MenuOpen = true;

                OpenMenu();
            }

            if (DeloreanHandler.CurrentTimeMachine == null)
            {
                if(TimeMachineMenu.Visible)
                    TimeMachineMenu.Visible = false;
            }

            if (DeloreanHandler.CurrentTimeMachine != null)
            {
                if (RCMenu.Visible)
                    RCMenu.Visible = false;
            }

            MenuOpen = TimeMachineMenu.Visible || RCMenu.Visible || SpawnMenu.Visible || SpawnMenuContext.Visible || PresetsMenu.Visible || TrainMissionMenu.Visible || StatisticsMenu.Visible || PhotoMenu.Visible;

            if (MenuOpen)
            {
                if (RCMenu.Visible)
                    RCMenu.Process();

                if (TimeMachineMenu.Visible)
                    TimeMachineMenu.Process();

                if(PresetsMenu.Visible)
                    PresetsMenu.Process();

                if (StatisticsMenu.Visible)
                    StatisticsMenu.Process();

                if (PhotoMenu.Visible)
                    PhotoMenu.Process();
            }

        }

        private static void OpenMenu()
        {
            if (DeloreanHandler.CurrentTimeMachine != null)
            {
                if (!TimeMachineMenu.Visible)
                    TimeMachineMenu.Visible = true;

                if (RCMenu.Visible)
                    RCMenu.Visible = false;

                if (PhotoMenu.Visible)
                    PhotoMenu.Visible = false;
            }
            else
            {
                if (!ModMenuHandler.MainMenu.Visible)
                    ModMenuHandler.MainMenu.Visible = true;

                if (TimeMachineMenu.Visible)
                    TimeMachineMenu.Visible = false;

                if (RCMenu.Visible)
                    RCMenu.Visible = false;
            }
        }
    }
}
