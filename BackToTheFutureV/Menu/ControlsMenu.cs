using BackToTheFutureV.Settings;
using FusionLibrary;
using LemonUI.Menus;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV.Menu
{
    internal class ControlsMenu : BTTFVMenu
    {
        private NativeCheckboxItem UseControlForMainMenu;
        private NativeListItem<Keys> MainMenu;

        private NativeCheckboxItem CombinationsForInteractionMenu;
        private NativeListItem<ControlInfo> InteractionMenu1;
        private NativeListItem<ControlInfo> InteractionMenu2;

        private NativeCheckboxItem LongPressForHover;
        private NativeListItem<ControlInfo> Hover;
        private NativeListItem<ControlInfo> HoverBoost;
        private NativeListItem<ControlInfo> HoverVTOL;
        private NativeListItem<Keys> HoverAltitudeHold;

        private NativeListItem<Keys> TCToggle;
        private NativeListItem<Keys> CutsceneToggle;
        private NativeListItem<Keys> InputToggle;

        private NativeItem Reset;

        private bool _doNotUpdate;

        public ControlsMenu() : base("Controls")
        {
            Banner = null;

            Offset = new PointF(0, 60);

            Width = 600;

            Shown += ControlsMenu_Shown;
            Closing += ControlsMenu_Closing;
            OnItemCheckboxChanged += ControlsMenu_OnItemCheckboxChanged;
            OnItemActivated += ControlsMenu_OnItemActivated;

            MainMenu = NewListItem<Keys>("GoToMainMenu");
            MainMenu.Items = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            MainMenu.ItemChanged += MainMenu_ItemChanged;

            UseControlForMainMenu = NewCheckboxItem("UseControlForMainMenu");


            InteractionMenu1 = NewListItem<ControlInfo>("InteractionMenu1");
            InteractionMenu1.Items = ControlInfo.CustomControls;
            InteractionMenu1.ItemChanged += InteractionMenu1_ItemChanged;

            InteractionMenu2 = NewListItem<ControlInfo>("InteractionMenu2");
            InteractionMenu2.Items = ControlInfo.CustomControls;
            InteractionMenu2.ItemChanged += InteractionMenu2_ItemChanged;

            CombinationsForInteractionMenu = NewCheckboxItem("CombinationsForInteractionMenu");


            Hover = NewListItem<ControlInfo>("Hover");
            Hover.Items = ControlInfo.CustomControls;
            Hover.ItemChanged += Hover_ItemChanged;

            LongPressForHover = NewCheckboxItem("LongPressForHover");

            HoverBoost = NewListItem<ControlInfo>("HoverBoost");
            HoverBoost.Items = ControlInfo.CustomControls;
            HoverBoost.ItemChanged += HoverBoost_ItemChanged;

            HoverVTOL = NewListItem<ControlInfo>("HoverVTOL");
            HoverVTOL.Items = ControlInfo.CustomControls;
            HoverVTOL.ItemChanged += HoverVTOL_ItemChanged;

            HoverAltitudeHold = NewListItem<Keys>("HoverAltitudeHold");
            HoverAltitudeHold.Items = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            HoverAltitudeHold.ItemChanged += HoverAltitudeHold_ItemChanged;


            TCToggle = NewListItem<Keys>("TCToggle");
            TCToggle.Items = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            TCToggle.ItemChanged += TCToggle_ItemChanged;

            CutsceneToggle = NewListItem<Keys>("CutsceneToggle");
            CutsceneToggle.Items = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            CutsceneToggle.ItemChanged += CutsceneToggle_ItemChanged;

            InputToggle = NewListItem<Keys>("InputToggle");
            InputToggle.Items = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            InputToggle.ItemChanged += InputToggle_ItemChanged;

            Reset = NewItem("Reset");
        }

        private void ControlsMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == Reset)
            {
                ModControls.Reset();
                ControlsMenu_Shown(this, new EventArgs());
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

        private void ControlsMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
                return;

            ModControls.HoverVTOL = e.Object.Control;
            TextHandler.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void HoverBoost_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
                return;

            ModControls.HoverBoost = e.Object.Control;
            TextHandler.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void Hover_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
                return;

            ModControls.Hover = e.Object.Control;
            TextHandler.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void InteractionMenu2_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
                return;

            ModControls.InteractionMenu2 = e.Object.Control;
            TextHandler.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void InteractionMenu1_ItemChanged(object sender, ItemChangedEventArgs<ControlInfo> e)
        {
            if (_doNotUpdate)
                return;

            ModControls.InteractionMenu1 = e.Object.Control;
            TextHandler.ShowHelp("SelectButton", false, e.Object.Button);
        }

        private void MainMenu_ItemChanged(object sender, ItemChangedEventArgs<Keys> e)
        {
            ModControls.MainMenu = e.Object;
        }

        private void ControlsMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == UseControlForMainMenu)
                ModControls.UseControlForMainMenu = Checked;
            else if (sender == CombinationsForInteractionMenu)
                ModControls.CombinationsForInteractionMenu = Checked;
            else if (sender == LongPressForHover)
                ModControls.LongPressForHover = Checked;
        }

        private void ControlsMenu_Shown(object sender, EventArgs e)
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
    }
}
