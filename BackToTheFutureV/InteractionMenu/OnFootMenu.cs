using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Utility;
using GTA;
using NativeUI;

namespace BackToTheFutureV.InteractionMenu
{
    public class OnFootMenu : UIMenu
    {
        public UIMenuItem BackToMainMenu { get; }

        public UIMenuItem Refuel { get; }

        public OnFootMenu() : base(Game.GetLocalizedString("BTTFV_Menu_OnFoot"), Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            AddItem(BackToMainMenu = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu"), Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu_Description")));
            Utils.AttachSubmenu(this, InteractionMenuManager.RCMenu, Game.GetLocalizedString("BTTFV_Menu_RCMenu"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_Description"));            
            AddItem(Refuel = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_OnFoot_Refuel"), Game.GetLocalizedString("BTTFV_Menu_OnFoot_Refuel_Description")));

            OnItemSelect += OnFootMenu_OnItemSelect;
        }


        private void OnFootMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if(selectedItem == BackToMainMenu)
            {
                Main.MenuPool.CloseAllMenus();
                ModMenuHandler.MainMenu.Visible = true;
            }

            if(selectedItem == Refuel && Refuel.Enabled)
                DeloreanHandler.ClosestTimeMachine.Circuits.GetHandler<FuelHandler>().Refuel(Main.PlayerPed);
        }

        public void Process()
        {
            if(DeloreanHandler.ClosestTimeMachine != null && DeloreanHandler.SquareDistToClosestTimeMachine <= 5f * 5f)
            {
                var fuelHandler = DeloreanHandler.ClosestTimeMachine.Circuits.GetHandler<FuelHandler>();

                if (fuelHandler.IsRefueling)
                {
                    Refuel.Enabled = false;
                    return;
                }

                if (fuelHandler.CanRefuel(Main.PlayerPed))
                {
                    if (fuelHandler.IsFueled)
                    {
                        Refuel.Enabled = false;
                        Refuel.Text = Game.GetLocalizedString("BTTFV_Menu_OnFoot_Refuel_Full");

                        return;
                    }

                    Refuel.Enabled = true;
                    Refuel.Text = Game.GetLocalizedString("BTTFV_Menu_OnFoot_Refuel");

                    return;
                }
            }

            Refuel.Enabled = false;
            Refuel.Text = Game.GetLocalizedString("BTTFV_Menu_OnFoot_Refuel_Error");
        }
    }
}
