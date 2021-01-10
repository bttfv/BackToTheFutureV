using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;
using static FusionLibrary.Enums;
using Control = GTA.Control;

namespace BackToTheFutureV.Menu
{
    public class PresetsMenu : CustomNativeMenu
    {
        private InstrumentalMenu _instrumentalMenu;

        public PresetsMenu() : base("", Game.GetLocalizedString("BTTFV_Input_PresetsMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += PresetsMenu_Shown;
            OnItemActivated += PresetsMenu_OnItemActivated;

            _instrumentalMenu = new InstrumentalMenu();

            _instrumentalMenu.AddControl(Control.PhoneCancel, Game.GetLocalizedString("HUD_INPUT3"));
            _instrumentalMenu.AddControl(Control.PhoneSelect, Game.GetLocalizedString("HUD_INPUT2"));
            _instrumentalMenu.AddControl(Control.PhoneRight, Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Delete"));
            _instrumentalMenu.AddControl(Control.PhoneLeft, Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Rename"));
            _instrumentalMenu.AddControl(Control.PhoneExtraOption, Game.GetLocalizedString("BTTFV_Input_PresetsMenu_New"));
        }

        private void PresetsMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (ModSettings.CinematicSpawn)
                TimeMachineHandler.Create(sender.Title, SpawnFlags.ForcePosition | SpawnFlags.NoOccupants | SpawnFlags.ResetValues | SpawnFlags.ForceReentry);
            else
                TimeMachineHandler.Create(sender.Title, SpawnFlags.ForcePosition | SpawnFlags.NoOccupants | SpawnFlags.ResetValues | SpawnFlags.NoVelocity | SpawnFlags.WarpPlayer);

            Close();
        }

        private void PresetsMenu_Shown(object sender, EventArgs e)
        {
            ReloadList();
        }

        private void ReloadList()
        {
            Clear();

            foreach (string preset in TimeMachineClone.ListPresets())
                Add(new NativeItem(preset));
        }

        public override void Tick()
        {
            _instrumentalMenu.UpdatePanel();
            _instrumentalMenu.Render2DFullscreen();

            if (Game.IsControlJustPressed(Control.PhoneLeft))
            {
                string _name = Game.GetUserInput(WindowTitle.EnterMessage20, SelectedItem.Title, 20);

                if (_name == null || _name == "")
                    return;

                TimeMachineClone.RenameSave(SelectedItem.Title, _name);
                ReloadList();
            }

            if (Game.IsControlJustPressed(Control.PhoneRight))
            {
                TimeMachineClone.DeleteSave(SelectedItem.Title);
                ReloadList();
            }

            if (Game.IsControlJustPressed(Control.PhoneExtraOption))
            {
                Close();

                MenuHandler.CustomMenuForced.Open();
            }
        }
    }
}
