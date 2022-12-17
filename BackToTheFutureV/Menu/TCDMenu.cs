using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class TCDMenu : BTTFVMenu
    {
        private readonly NativeListItem<TCDBackground> tcdBackground;
        private readonly NativeItem changeTCD;
        private readonly NativeItem resetToDefaultTCD;

        private readonly NativeItem changeRCGUI;

        private readonly NativeCheckboxItem hideSID;
        private readonly NativeCheckboxItem hideIngameTCD;

        public TCDMenu() : base("TCD")
        {
            tcdBackground = NewListItem("Background", TCDBackground.BTTF1, TCDBackground.BTTF3, TCDBackground.Transparent);
            tcdBackground.ItemChanged += TcdBackground_ItemChanged;

            changeTCD = NewItem("Edit");
            changeRCGUI = NewItem("EditRC");
            resetToDefaultTCD = NewItem("Reset");

            hideSID = NewCheckboxItem("HideSID", ModSettings.HideSID);
            hideIngameTCD = NewCheckboxItem("HideHUD", ModSettings.HideIngameTCDToggle);
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            switch (sender)
            {
                case NativeCheckboxItem _ when sender == hideSID:
                    ModSettings.HideSID = Checked;
                    break;
                case NativeCheckboxItem _ when sender == hideIngameTCD:
                    ModSettings.HideIngameTCDToggle = Checked;
                    break;
            }

            ModSettings.SaveSettings();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            switch (sender)
            {
                case NativeItem _ when sender == changeTCD:
                    TcdEditer.SetEditMode(true);
                    Visible = false;
                    break;
                case NativeItem _ when sender == changeRCGUI:
                    RCGUIEditer.SetEditMode(true);
                    Visible = false;
                    break;
                case NativeItem _ when sender == resetToDefaultTCD:
                    RCGUIEditer.ResetToDefault();
                    TcdEditer.ResetToDefault();
                    ModSettings.HideIngameTCDToggle = false;
                    ModSettings.SaveSettings();
                    break;
            }
        }

        private void TcdBackground_ItemChanged(object sender, ItemChangedEventArgs<TCDBackground> e)
        {
            ModSettings.TCDBackground = e.Object;
            ModSettings.SaveSettings();
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {

        }

        public override void Tick()
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
