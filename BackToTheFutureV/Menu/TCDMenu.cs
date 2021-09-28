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
        private readonly NativeCheckboxItem useExternalTCD;
        private readonly NativeCheckboxItem useNetworkTCD;
        private readonly NativeCheckboxItem hideIngameTCD;

        public TCDMenu() : base("TCD")
        {
            tcdBackground = NewListItem("Background", TCDBackground.Metal, TCDBackground.Transparent);
            tcdBackground.ItemChanged += TcdBackground_ItemChanged;

            changeTCD = NewItem("Edit");
            changeRCGUI = NewItem("EditRC");
            resetToDefaultTCD = NewItem("Reset");

            hideSID = NewCheckboxItem("HideSID", ModSettings.HideSID);
            useExternalTCD = NewCheckboxItem("External", ModSettings.ExternalTCDToggle);
            useNetworkTCD = NewCheckboxItem("Remote", ModSettings.RemoteTCDToggle);
            hideIngameTCD = NewCheckboxItem("HideHUD", ModSettings.HideIngameTCDToggle);
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == useExternalTCD)
            {
                ModSettings.ExternalTCDToggle = Checked;
            }

            if (sender == useNetworkTCD)
            {
                ModSettings.RemoteTCDToggle = Checked;
            }

            if (sender == hideIngameTCD)
            {
                ModSettings.HideIngameTCDToggle = Checked;
            }

            if (sender == hideSID)
            {
                ModSettings.HideSID = Checked;
            }

            ModSettings.SaveSettings();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == changeTCD)
            {
                TcdEditer.SetEditMode(true);

                Visible = false;
            }

            if (sender == resetToDefaultTCD)
            {
                RCGUIEditer.ResetToDefault();
                TcdEditer.ResetToDefault();

                ModSettings.HideIngameTCDToggle = false;
                ModSettings.ExternalTCDToggle = false;
                ModSettings.RemoteTCDToggle = false;

                ModSettings.SaveSettings();
            }

            if (sender == changeRCGUI)
            {
                RCGUIEditer.SetEditMode(true);

                Visible = false;
            }
        }

        private void TcdBackground_ItemChanged(object sender, ItemChangedEventArgs<TCDBackground> e)
        {
            ModSettings.TCDBackground = e.Object;
            ModSettings.SaveSettings();
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            useExternalTCD.Checked = ModSettings.ExternalTCDToggle;
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
