using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using GTA;
using GTA.Math;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.GUI;

namespace BackToTheFutureV.InteractionMenu
{
    public class PresetsMenu : UIMenu
    {
        private InstrumentalMenu _instrumentalMenu;
        public PresetsMenu() : base(Game.GetLocalizedString("BTTFV_Input_PresetsMenu"), Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Choose"))
        {            
            OnMenuOpen += PresetsMenu_OnMenuOpen;
            OnItemSelect += PresetsMenu_OnItemSelect;

            DisableInstructionalButtons(true);

            _instrumentalMenu = new InstrumentalMenu();

            _instrumentalMenu.AddControl(Control.PhoneCancel, Game.GetLocalizedString("HUD_INPUT3"));
            _instrumentalMenu.AddControl(Control.PhoneSelect, Game.GetLocalizedString("HUD_INPUT2"));
            _instrumentalMenu.AddControl(Control.PhoneRight, Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Delete"));
            _instrumentalMenu.AddControl(Control.PhoneLeft, Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Rename"));
            _instrumentalMenu.AddControl(Control.PhoneExtraOption, Game.GetLocalizedString("BTTFV_Input_PresetsMenu_New"));
        }

        public void Process()
        {
            _instrumentalMenu.UpdatePanel();
            _instrumentalMenu.Render2DFullscreen();

            if (Game.IsControlJustPressed(Control.PhoneLeft))
            {
                string _name = Game.GetUserInput(WindowTitle.EnterMessage20, MenuItems[CurrentSelection].Text, 20);

                if (_name == null || _name == "")
                    return;

                DeloreanModsCopy.RenameSave(MenuItems[CurrentSelection].Text, _name);
                ReloadList();
            }

            if (Game.IsControlJustPressed(Control.PhoneRight))
            {
                DeloreanModsCopy.DeleteSave(MenuItems[CurrentSelection].Text);
                ReloadList();
            }

            if (Game.IsControlJustPressed(Control.PhoneExtraOption))
            {
                Main.MenuPool.CloseAllMenus();

                InteractionMenuManager.SpawnMenu.ForceNew = true;
                InteractionMenuManager.SpawnMenu.Visible = true;
            }
        }

        private void PresetsMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (ModSettings.CinematicSpawn)
                DeloreanHandler.SpawnWithReentry(DeloreanType.BTTF1, selectedItem.Text);
            else
                DeloreanHandler.Spawn(DeloreanType.BTTF1).Mods.Load(selectedItem.Text);

            Main.MenuPool.CloseAllMenus();
        }

        private void PresetsMenu_OnMenuOpen(UIMenu sender)
        {            
            ReloadList();
        }

        private void ReloadList()
        {
            Clear();

            foreach (string preset in DeloreanModsCopy.ListPresets())
                AddItem(new UIMenuItem(preset));

            RefreshIndex();
        }
    }
}
