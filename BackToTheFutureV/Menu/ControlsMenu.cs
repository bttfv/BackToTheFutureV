using FusionLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal class ControlsMenu : BTTFVMenu
    {
        private readonly NativeCheckboxItem UseControlForMainMenu;
        private readonly NativeListItem<Keys> MainMenu;

        private readonly NativeCheckboxItem CombinationsForInteractionMenu;
        private readonly NativeListItem<ControlInfo> InteractionMenu1;
        private readonly NativeListItem<ControlInfo> InteractionMenu2;

        private readonly NativeCheckboxItem LongPressForHover;
        private readonly NativeListItem<ControlInfo> Hover;
        private readonly NativeListItem<ControlInfo> HoverBoost;
        private readonly NativeListItem<ControlInfo> HoverVTOL;
        private readonly NativeListItem<Keys> HoverAltitudeHold;

        private readonly NativeListItem<Keys> TCToggle;
        private readonly NativeListItem<Keys> CutsceneToggle;
        private readonly NativeListItem<Keys> InputToggle;

        private readonly NativeItem Reset;

        private bool _doNotUpdate;

        public ControlsMenu() : base("Controls")
        {
            Banner = null;

            Offset = new PointF(0, 60);

            Width = 600;

            MainMenu = NewListItem("GoToMainMenu", Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray());
            MainMenu.ItemChanged += MainMenu_ItemChanged;

            UseControlForMainMenu = NewCheckboxItem("UseControlForMainMenu");

            InteractionMenu1 = NewListItem("InteractionMenu1", ControlInfo.CustomControls.ToArray());
            InteractionMenu1.ItemChanged += InteractionMenu1_ItemChanged;

            InteractionMenu2 = NewListItem("InteractionMenu2", ControlInfo.CustomControls.ToArray());
            InteractionMenu2.ItemChanged += InteractionMenu2_ItemChanged;

            CombinationsForInteractionMenu = NewCheckboxItem("CombinationsForInteractionMenu");

            Hover = NewListItem("Hover", ControlInfo.CustomControls.ToArray());
            Hover.ItemChanged += Hover_ItemChanged;

            LongPressForHover = NewCheckboxItem("LongPressForHover");

            HoverBoost = NewListItem("HoverBoost", ControlInfo.CustomControls.ToArray());
            HoverBoost.ItemChanged += HoverBoost_ItemChanged;

            HoverVTOL = NewListItem("HoverVTOL", ControlInfo.CustomControls.ToArray());
            HoverVTOL.ItemChanged += HoverVTOL_ItemChanged;

            HoverAltitudeHold = NewListItem("HoverAltitudeHold", Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray());
            HoverAltitudeHold.ItemChanged += HoverAltitudeHold_ItemChanged;

            TCToggle = NewListItem("TCToggle", Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray());
            TCToggle.ItemChanged += TCToggle_ItemChanged;

            CutsceneToggle = NewListItem("CutsceneToggle", Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray());
            CutsceneToggle.ItemChanged += CutsceneToggle_ItemChanged;

            InputToggle = NewListItem("InputToggle", Enum.GetValues(typeof(Keys)).Cast<Keys>().ToArray());
            InputToggle.ItemChanged += InputToggle_ItemChanged;

            Reset = NewItem("Reset");
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == Reset)
            {
                ModControls.Reset();
                Menu_Shown(this, new EventArgs());
            }
        }

        private void InputToggle_ItemChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModControls.InputToggle = e.Object;
        }

        private void CutsceneToggle_ItemChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModControls.CutsceneToggle = e.Object;
        }

        private void TCToggle_ItemChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModControls.TCToggle = e.Object;
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {
            ModSettings.SaveSettings();
        }

        private void HoverAltitudeHold_ItemChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModControls.HoverAltitudeHold = e.Object;
        }

        private void HoverVTOL_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
            {
                return;
            }

            ModControls.HoverVTOL = e.Object.Control;
            TextHandler.Me.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void HoverBoost_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
            {
                return;
            }

            ModControls.HoverBoost = e.Object.Control;
            TextHandler.Me.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void Hover_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
            {
                return;
            }

            ModControls.Hover = e.Object.Control;
            TextHandler.Me.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void InteractionMenu2_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
            {
                return;
            }

            ModControls.InteractionMenu2 = e.Object.Control;
            TextHandler.Me.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void InteractionMenu1_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
            {
                return;
            }

            ModControls.InteractionMenu1 = e.Object.Control;
            TextHandler.Me.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void MainMenu_ItemChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModControls.MainMenu = e.Object;
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == UseControlForMainMenu)
            {
                ModControls.UseControlForMainMenu = Checked;
            }
            else if (sender == CombinationsForInteractionMenu)
            {
                ModControls.CombinationsForInteractionMenu = Checked;
            }
            else if (sender == LongPressForHover)
            {
                ModControls.LongPressForHover = Checked;
            }
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            _doNotUpdate = true;

            UseControlForMainMenu.Checked = ModControls.UseControlForMainMenu;
            MainMenu.SelectedIndex = MainMenu.Items.IndexOf(ModControls.MainMenu);

            CombinationsForInteractionMenu.Checked = ModControls.CombinationsForInteractionMenu;
            InteractionMenu1.SelectedIndex = ControlInfo.IndexOf(ModControls.InteractionMenu1);
            InteractionMenu2.SelectedIndex = ControlInfo.IndexOf(ModControls.InteractionMenu2);

            LongPressForHover.Checked = ModControls.LongPressForHover;
            Hover.SelectedIndex = ControlInfo.IndexOf(ModControls.Hover);
            HoverBoost.SelectedIndex = ControlInfo.IndexOf(ModControls.HoverBoost);
            HoverVTOL.SelectedIndex = ControlInfo.IndexOf(ModControls.HoverVTOL);
            HoverAltitudeHold.SelectedIndex = HoverAltitudeHold.Items.IndexOf(ModControls.HoverAltitudeHold);

            TCToggle.SelectedIndex = TCToggle.Items.IndexOf(ModControls.TCToggle);
            CutsceneToggle.SelectedIndex = CutsceneToggle.Items.IndexOf(ModControls.CutsceneToggle);
            InputToggle.SelectedIndex = InputToggle.Items.IndexOf(ModControls.InputToggle);

            _doNotUpdate = false;
        }

        public override void Tick()
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }
    }
}
