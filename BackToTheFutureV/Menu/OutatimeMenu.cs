using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Menu
{
    public class OutatimeMenu : CustomNativeMenu
    {
        private NativeListItem<RemoteTimeMachine> TimeMachines { get; }
        private NativeItem TypeDescription { get; }
        private NativeItem DestinationTimeDescription { get; }
        private NativeItem LastTimeDescription { get; }
        private NativeCheckboxItem Spawned { get; }
        private NativeCheckboxItem ShowBlip { get; }
        private NativeItem ForceReenter { get; }

        private RemoteTimeMachine CurrentRemoteTimeMachine => TimeMachines.SelectedItem;

        public OutatimeMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += OutatimeMenu_Shown;
            OnItemCheckboxChanged += OutatimeMenu_OnItemCheckboxChanged;
            OnItemActivated += OutatimeMenu_OnItemActivated;

            Add(TimeMachines = new NativeListItem<RemoteTimeMachine>(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Deloreans"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Deloreans_Description")));
            Add(TypeDescription = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type_Description")));
            Add(DestinationTimeDescription = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime_Description")));
            Add(LastTimeDescription = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime_Description")));
            Add(Spawned = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Spawned"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Spawned_Description")));
            Add(ShowBlip = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ShowBlip"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ShowBlip_Description")));
            Add(ForceReenter = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ForceReenter"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ForceReenter_Description")));

            TypeDescription.Enabled = false;
            DestinationTimeDescription.Enabled = false;
            LastTimeDescription.Enabled = false;
            Spawned.Enabled = false;
        }

        private void UpdateInfos()
        {
            TypeDescription.Title = $"{Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type")}: {CurrentRemoteTimeMachine.TimeMachineClone.Mods.WormholeType}";
            DestinationTimeDescription.Title = Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime") + " " + CurrentRemoteTimeMachine.TimeMachineClone.Properties.DestinationTime.ToString("MM/dd/yyyy hh:mm tt");
            LastTimeDescription.Title = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime") + " " + CurrentRemoteTimeMachine.TimeMachineClone.Properties.PreviousTime.ToString("MM/dd/yyyy hh:mm tt");

            Spawned.Checked = CurrentRemoteTimeMachine.Spawned;

            ShowBlip.Checked = CurrentRemoteTimeMachine.Blip != null && CurrentRemoteTimeMachine.Blip.Exists();
        }

        private void OutatimeMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == ForceReenter && !CurrentRemoteTimeMachine.Spawned)
                CurrentRemoteTimeMachine.Spawn(ReenterType.Forced);        
        }

        private void OutatimeMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == ShowBlip)
            {
                if (Checked)
                {
                    Vector3 pos = CurrentRemoteTimeMachine.TimeMachineClone.Vehicle.Position;

                    CurrentRemoteTimeMachine.Blip = World.CreateBlip(pos);
                    CurrentRemoteTimeMachine.Blip.Sprite = BlipSprite.SlowTime;

                    switch (CurrentRemoteTimeMachine.TimeMachineClone.Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            CurrentRemoteTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF1")}";
                            CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer22;
                            break;

                        case WormholeType.BTTF2:
                            CurrentRemoteTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF2")}";
                            CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer21;
                            break;

                        case WormholeType.BTTF3:
                            if (CurrentRemoteTimeMachine.TimeMachineClone.Mods.Wheel == WheelType.RailroadInvisible)
                            {
                                CurrentRemoteTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3RR")}";
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.Orange;
                            }
                            else
                            {
                                CurrentRemoteTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3")}";
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.Red;
                            }
                            break;
                    }
                }
                else
                    CurrentRemoteTimeMachine.Blip?.Delete();
            }
        }

        private void OutatimeMenu_Shown(object sender, EventArgs e)
        {
            TimeMachines.Items = RemoteTimeMachineHandler.RemoteTimeMachinesOnlyReentry;
        }

        public override void Tick()
        {
            UpdateInfos();
        }
    }
}
