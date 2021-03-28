using BackToTheFutureV.TimeMachineClasses;
using GTA;
using GTA.Math;
using LemonUI.Menus;
using System;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.Menu
{
    internal class OutatimeMenu : BTTFVMenu
    {
        private NativeListItem<RemoteTimeMachine> TimeMachines { get; }
        private NativeItem TypeDescription { get; }
        private NativeItem DestinationTimeDescription { get; }
        private NativeItem LastTimeDescription { get; }
        private NativeCheckboxItem Spawned { get; }
        private NativeCheckboxItem ShowBlip { get; }
        private NativeItem ForceReenter { get; }

        private RemoteTimeMachine CurrentRemoteTimeMachine => TimeMachines.SelectedItem;

        public OutatimeMenu() : base("Outatime")
        {
            Shown += OutatimeMenu_Shown;
            OnItemCheckboxChanged += OutatimeMenu_OnItemCheckboxChanged;
            OnItemActivated += OutatimeMenu_OnItemActivated;

            TimeMachines = NewListItem<RemoteTimeMachine>("List");
            TypeDescription = NewItem("Type");
            DestinationTimeDescription = NewItem("Destination");
            LastTimeDescription = NewItem("Last");
            Spawned = NewCheckboxItem("Spawned");
            ShowBlip = NewCheckboxItem("Blip");
            ForceReenter = NewItem("Reenter");

            TypeDescription.Enabled = false;
            DestinationTimeDescription.Enabled = false;
            LastTimeDescription.Enabled = false;
            Spawned.Enabled = false;
        }

        private void UpdateInfos()
        {
            TypeDescription.Title = $"{GetItemTitle("Type")}: {CurrentRemoteTimeMachine.TimeMachineClone.Mods.WormholeType}";
            DestinationTimeDescription.Title = GetItemTitle("Destination") + " " + CurrentRemoteTimeMachine.TimeMachineClone.Properties.DestinationTime.ToString("MM/dd/yyyy hh:mm tt");
            LastTimeDescription.Title = GetItemTitle("Last") + " " + CurrentRemoteTimeMachine.TimeMachineClone.Properties.PreviousTime.ToString("MM/dd/yyyy hh:mm tt");

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
                            CurrentRemoteTimeMachine.Blip.Name = TextHandler.GetLocalizedText("BTTF1");
                            CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer22;
                            break;

                        case WormholeType.BTTF2:
                            CurrentRemoteTimeMachine.Blip.Name = TextHandler.GetLocalizedText("BTTF2");
                            CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer21;
                            break;

                        case WormholeType.BTTF3:
                            if (CurrentRemoteTimeMachine.TimeMachineClone.Mods.Wheel == WheelType.RailroadInvisible)
                            {
                                CurrentRemoteTimeMachine.Blip.Name = TextHandler.GetLocalizedText("BTTF3RR");
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.Orange;
                            }
                            else
                            {
                                CurrentRemoteTimeMachine.Blip.Name = TextHandler.GetLocalizedText("BTTF3");
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
            TimeMachines.Items = RemoteTimeMachineHandler.RemoteTimeMachines;
        }

        public override void Tick()
        {
            UpdateInfos();
        }
    }
}
