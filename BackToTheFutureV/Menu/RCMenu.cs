using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class RCMenu : CustomNativeMenu
    {
        private NativeListItem<TimeMachine> timeMachinesList;
        private NativeCheckboxItem FuelChamberDescription;
        private NativeCheckboxItem TimeCircuitsOnDescription;
        private NativeItem DestinationTimeDescription;

        private TimeMachine CurrentTimeMachine => timeMachinesList.SelectedItem;
        private Camera CarCam { get; set; }
        private bool CanBeSelected { get; set; }

        public RCMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_RCMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += RCMenu_Shown;
            Closing += RCMenu_Closing;
            OnItemActivated += RCMenu_OnItemActivated;

            Add(timeMachinesList = new NativeListItem<TimeMachine>(Game.GetLocalizedString("BTTFV_Menu_RCMenu_Deloreans"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_Deloreans_Description")));

            timeMachinesList.ItemChanged += TimeMachinesList_ItemChanged;

            Add(FuelChamberDescription = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_FuelChamberFilled"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_FuelChamberFilled_Description")));
            Add(TimeCircuitsOnDescription = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_TimeCircuitsOn"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_TimeCircuitsOn_Description")));
            Add(DestinationTimeDescription = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime_Description")));

            FuelChamberDescription.Enabled = false;
            TimeCircuitsOnDescription.Enabled = false;
            DestinationTimeDescription.Enabled = false;
        }

        private void RCMenu_Shown(object sender, EventArgs e)
        {
            timeMachinesList.Items = TimeMachineHandler.TimeMachinesNoStory;

            CanBeSelected = TrySelectCar();
        }

        private void RCMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == timeMachinesList)
            {
                if (CanBeSelected)
                {
                    Close();

                    RCManager.RemoteControl(CurrentTimeMachine);                    
                }
            }
        }

        private void RCMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopPreviewing();
        }

        private void TimeMachinesList_ItemChanged(object sender, ItemChangedEventArgs<TimeMachine> e)
        {
            CanBeSelected = TrySelectCar();
        }

        private bool TrySelectCar()
        {
            FuelChamberDescription.Checked = CurrentTimeMachine.Properties.IsFueled;
            TimeCircuitsOnDescription.Checked = CurrentTimeMachine.Properties.AreTimeCircuitsOn;
            DestinationTimeDescription.Title = $"{Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime")} {CurrentTimeMachine.Properties.DestinationTime.ToString("MM/dd/yyyy hh:mm tt")}";

            float dist = CurrentTimeMachine.Vehicle.Position.DistanceToSquared(Utils.PlayerPed.Position);

            if (dist <= RCManager.MAX_DIST * RCManager.MAX_DIST)
            {
                PreviewCar();

                return true;
            }
            else
            {
                StopPreviewing();

                GTA.UI.Notification.Show(Game.GetLocalizedString("BTTFV_OutOfRange"));
                return false;
            }
        }

        public void PreviewCar()
        {
            if (CarCam != null)
                CarCam?.Delete();

            CarCam = World.CreateCamera(CurrentTimeMachine.Vehicle.GetOffsetPosition(new Vector3(0.0f, -5.0f, 3.0f)), World.RenderingCamera.Rotation, 75.0f);
            CarCam.PointAt(CurrentTimeMachine.Vehicle);

            World.RenderingCamera = CarCam;

            Function.Call(Hash.CLEAR_FOCUS);
            Function.Call(Hash.SET_FOCUS_ENTITY, CurrentTimeMachine.Vehicle);
        }

        public void StopPreviewing()
        {
            Function.Call(Hash.CLEAR_FOCUS);
            CarCam?.Delete();
            CarCam = null;
            World.RenderingCamera = null;
        }

        public override void Tick()
        {
            if (Utils.PlayerVehicle != null)
                Close();
        }
    }
}
