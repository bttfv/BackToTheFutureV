using GTA;
using GTA.Math;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
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

            ForceReenter.Enabled = !CurrentRemoteTimeMachine.TimeMachineClone.Properties.AreTimeCircuitsBroken && !CurrentRemoteTimeMachine.Spawned;
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == ForceReenter && !CurrentRemoteTimeMachine.Spawned)
            {
                CurrentRemoteTimeMachine.Spawn(ReenterType.Normal).Properties.NewGUID();
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
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
                            if (CurrentRemoteTimeMachine.TimeMachineClone.Mods.Hook == HookState.On || CurrentRemoteTimeMachine.TimeMachineClone.Mods.Hook == HookState.OnDoor)
                            {
                                CurrentRemoteTimeMachine.Blip.Name = TextHandler.Me.GetLocalizedText("BTTF1H");
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer20;
                            }
                            else
                            {
                                CurrentRemoteTimeMachine.Blip.Name = TextHandler.Me.GetLocalizedText("BTTF1");
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer22;
                            }
                            break;

                        case WormholeType.BTTF2:
                            CurrentRemoteTimeMachine.Blip.Name = TextHandler.Me.GetLocalizedText("BTTF2");
                            CurrentRemoteTimeMachine.Blip.Color = BlipColor.NetPlayer21;
                            break;

                        case WormholeType.BTTF3:
                            if (CurrentRemoteTimeMachine.TimeMachineClone.Mods.Wheel == WheelType.RailroadInvisible)
                            {
                                CurrentRemoteTimeMachine.Blip.Name = TextHandler.Me.GetLocalizedText("BTTF3RR");
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.Orange;
                            }
                            else
                            {
                                CurrentRemoteTimeMachine.Blip.Name = TextHandler.Me.GetLocalizedText("BTTF3");
                                CurrentRemoteTimeMachine.Blip.Color = BlipColor.Red;
                            }
                            break;
                    }
                }
                else
                {
                    CurrentRemoteTimeMachine.Blip?.Delete();
                }
            }
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            TimeMachines.Items = RemoteTimeMachineHandler.RemoteTimeMachines;
        }

        public override void Tick()
        {
            UpdateInfos();
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }
    }
}
