using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class OverrideMenu : BTTFVMenu
    {
        public NativeCheckboxItem OverrideCheckbox;
        public NativeSliderItem SIDSpeed;
        public NativeSliderItem TTSfxSpeed;
        public NativeSliderItem TTSpeed;
        public NativeItem WormholeLength;

        public OverrideMenu() : base("Override")
        {
            OverrideCheckbox = NewCheckboxItem("Set");

            SIDSpeed = NewSliderItem("SID", 200, 0);
            TTSfxSpeed = NewSliderItem("TTSfx", 200, 0);
            TTSpeed = NewSliderItem("TT", 200, 0);
            WormholeLength = NewItem("WormholeLength");
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            switch (sender)
            {
                case NativeItem _ when sender == WormholeLength:
                    bool ret = int.TryParse(Game.GetUserInput(WindowTitle.EnterMessage20, CurrentTimeMachine.Properties.OverrideWormholeLengthTime.ToString(), 4), out int num);

                    if (ret)
                    {
                        CurrentTimeMachine.Properties.OverrideWormholeLengthTime = num;
                        CurrentTimeMachine.Properties.OverrideSet = true;
                    }
                    break;
                case NativeSliderItem _ when sender == SIDSpeed:
                    bool ret2 = int.TryParse(Game.GetUserInput(WindowTitle.EnterMessage20, CurrentTimeMachine.Properties.OverrideSIDSpeed.ToString(), 3), out int num2);
                    if (num2 < 0)
                    {
                        num2 = 0;
                    }
                    else if (num2 > 200)
                    {
                        num2 = 200;
                    }

                    if (ret2)
                    {
                        CurrentTimeMachine.Properties.OverrideSIDSpeed = num2;
                        CurrentTimeMachine.Properties.OverrideSet = true;
                    }
                    break;
                case NativeSliderItem _ when sender == TTSfxSpeed:
                    bool ret3 = int.TryParse(Game.GetUserInput(WindowTitle.EnterMessage20, CurrentTimeMachine.Properties.OverrideTTSfxSpeed.ToString(), 3), out int num3);
                    if (num3 < 0)
                    {
                        num3 = 0;
                    }
                    else if (num3 > 200)
                    {
                        num3 = 200;
                    }

                    if (ret3)
                    {
                        CurrentTimeMachine.Properties.OverrideTTSfxSpeed = num3;
                        CurrentTimeMachine.Properties.OverrideSet = true;
                    }
                    break;
                case NativeSliderItem _ when sender == TTSpeed:
                    bool ret4 = int.TryParse(Game.GetUserInput(WindowTitle.EnterMessage20, CurrentTimeMachine.Properties.OverrideTTSpeed.ToString(), 3), out int num4);
                    if (num4 < 0)
                    {
                        num4 = 0;
                    }
                    else if (num4 > 200)
                    {
                        num4 = 200;
                    }

                    if (ret4)
                    {
                        CurrentTimeMachine.Properties.OverrideTTSpeed = num4;
                        CurrentTimeMachine.Properties.OverrideSet = true;
                    }
                    break;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == OverrideCheckbox)
            {
                CurrentTimeMachine.Properties.OverrideTimeTravelConstants = Checked;
                CurrentTimeMachine.Properties.OverrideSet = true;
            }
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {
            switch (sender)
            {
                case NativeSliderItem _ when sender == SIDSpeed:
                    CurrentTimeMachine.Properties.OverrideSIDSpeed = sender.Value;
                    break;
                case NativeSliderItem _ when sender == TTSfxSpeed:
                    CurrentTimeMachine.Properties.OverrideTTSfxSpeed = sender.Value;
                    break;
                case NativeSliderItem _ when sender == TTSpeed:
                    CurrentTimeMachine.Properties.OverrideTTSpeed = sender.Value;
                    break;
            }

            CurrentTimeMachine.Properties.OverrideSet = true;
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            OverrideCheckbox.Checked = CurrentTimeMachine.Properties.OverrideTimeTravelConstants;
        }

        public override void Tick()
        {
            SIDSpeed.Value = CurrentTimeMachine.Properties.OverrideSIDSpeed;
            TTSfxSpeed.Value = CurrentTimeMachine.Properties.OverrideTTSfxSpeed;
            TTSpeed.Value = CurrentTimeMachine.Properties.OverrideTTSpeed;
            SIDSpeed.Title = $"{GetItemTitle("SID")}: {SIDSpeed.Value}";
            TTSfxSpeed.Title = $"{GetItemTitle("TTSfx")}: {TTSfxSpeed.Value}";
            TTSpeed.Title = $"{GetItemTitle("TT")}: {TTSpeed.Value}";
            WormholeLength.Title = $"{GetItemTitle("WormholeLength")}: {CurrentTimeMachine.Properties.OverrideWormholeLengthTime}";
        }
    }
}
