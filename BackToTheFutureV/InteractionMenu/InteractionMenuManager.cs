using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using GTA;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Settings;
using GTA.NaturalMotion;
using BackToTheFutureV.TimeMachineClasses;

namespace BackToTheFutureV.InteractionMenu
{
    public class InteractionMenuManager
    {
        public static PhotoMenu PhotoMenu { get; private set; }

        public static StatisticsMenu StatisticsMenu { get; private set; }

        public static RCMenu RCMenu { get; private set; }

        public static TimeMachineMenu TimeMachineMenu { get; private set; }

        public static CustomMenu SpawnMenu { get; private set; }

        public static CustomMenu SpawnMenuContext { get; private set; }

        public static PresetsMenu PresetsMenu { get; private set; }

        //public static TrainMissionMenu TrainMissionMenu { get; private set; }
        
        public static void Init()
        {
            // Build the initial menu           
            Main.MenuPool.Add(PhotoMenu = new PhotoMenu());
            Main.MenuPool.Add(StatisticsMenu = new StatisticsMenu());
            Main.MenuPool.Add(RCMenu = new RCMenu());
            Main.MenuPool.Add(SpawnMenu = new CustomMenu());
            Main.MenuPool.Add(SpawnMenuContext = new CustomMenu());
            Main.MenuPool.Add(PresetsMenu = new PresetsMenu());
            Main.MenuPool.Add(TimeMachineMenu = new TimeMachineMenu());
            //Main.MenuPool.Add(TrainMissionMenu = new TrainMissionMenu());
        }

        public static void Process()
        {
            if (Game.IsControlPressed(Control.CharacterWheel) && Game.IsControlPressed(Control.VehicleHandbrake) && !Main.MenuPool.IsAnyMenuOpen() && !TcdEditer.IsEditing)
            {
                if (TimeMachineHandler.CurrentTimeMachine != null)
                {
                    if (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                        return;
                }

                OpenMenu();
            }

            if (TimeMachineHandler.CurrentTimeMachine == null)
            {
                if (TimeMachineMenu.Visible)
                    TimeMachineMenu.Visible = false;

                if (PhotoMenu.Visible)
                    PhotoMenu.Visible = false;

                if (SpawnMenuContext.Visible)
                    SpawnMenuContext.Visible = false;
            }

            if (TimeMachineHandler.CurrentTimeMachine != null)
            {
                if (RCMenu.Visible)
                    RCMenu.Visible = false;
            }

            if (Main.MenuPool.IsAnyMenuOpen())
            {
                if (ModMenuHandler.MainMenu.Visible)
                    ModMenuHandler.Process();

                if (RCMenu.Visible)
                    RCMenu.Process();

                if (TimeMachineMenu.Visible)
                    TimeMachineMenu.Process();

                if (PresetsMenu.Visible)
                    PresetsMenu.Process();

                if (StatisticsMenu.Visible)
                    StatisticsMenu.Process();

                if (PhotoMenu.Visible)
                    PhotoMenu.Process();

                if (SpawnMenu.Visible)
                    SpawnMenu.Process();

                if (SpawnMenuContext.Visible)
                    SpawnMenuContext.Process();
            }
        }

        public static void OpenMenu()
        {
            Main.MenuPool.CloseAllMenus();

            if (TimeMachineHandler.CurrentTimeMachine != null)
                TimeMachineMenu.Visible = true;
            else
                ModMenuHandler.MainMenu.Visible = true;
        }
    }
}
