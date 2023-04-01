using FusionLibrary;
using GTA;
using GTA.Native;
using GTA.UI;
//using KlangRageAudioLibrary;
using System;
using System.Globalization;
using System.Media;

namespace BackToTheFutureV
{
    internal class IntroHandler : Script
    {
        public static IntroHandler Me { get; private set; }
        public bool IsPlaying { get; private set; }

        private readonly CultureInfo dateFormat = CultureInfo.CreateSpecificCulture("en-US");

        private TimedEventHandler TimedEventHandler { get; } = new TimedEventHandler();
        SoundPlayer timeParadoxShort = new SoundPlayer("scripts\\BackToTheFutureV\\Sounds\\story\\bttf_subtitle1.wav");
        SoundPlayer timeParadox = new SoundPlayer("scripts\\BackToTheFutureV\\Sounds\\story\\bttf_subtitle3.wav");
        /*private readonly AudioPlayer timeParadox = Main.CommonAudioEngine.Create("story/bttf_subtitle3.wav", Presets.No3D);
        private readonly AudioPlayer timeParadoxShort = Main.CommonAudioEngine.Create("story/bttf_subtitle1.wav", Presets.No3D);*/

        private readonly string FirstBlock = $"BTTF V TEAM\nPresents";
        private readonly string SecondBlock = $"A\nBACK TO THE FUTURE\nMod";

        private DateTime date;
        private string DayBlock => date.ToString("dddd", dateFormat);
        private string DateBlock => "\n" + date.ToString("MMMM d, yyyy", dateFormat);
        private string TimeBlock => "\n\n" + date.ToString("h:mm tt", dateFormat);

        public IntroHandler()
        {
            TimedEventHandler.Add(0, 1, 623, 0, 1, 823);
            TimedEventHandler.Last.SetFloat(0, 255);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(FirstBlock, TextFade(timedEvent.CurrentFloat));
            };

            TimedEventHandler.Add(0, 1, 823, 0, 5, 500);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(FirstBlock, 255);
            };

            TimedEventHandler.Add(0, 5, 500, 0, 5, 700);
            TimedEventHandler.Last.SetFloat(255, 0);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(FirstBlock, TextFade(timedEvent.CurrentFloat));
            };


            TimedEventHandler.Add(0, 7, 623, 0, 7, 823);
            TimedEventHandler.Last.SetFloat(0, 255);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(SecondBlock, TextFade(timedEvent.CurrentFloat), 2);
            };

            TimedEventHandler.Add(0, 7, 823, 0, 11, 500);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(SecondBlock, 255, 2);
            };

            TimedEventHandler.Add(0, 11, 500, 0, 11, 700);
            TimedEventHandler.Last.SetFloat(255, 0);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(SecondBlock, TextFade(timedEvent.CurrentFloat), 2);
            };

            TimedEventHandler.Add(0, 12, 762, 0, 13, 346);
            TimedEventHandler.Last.SetFloat(0, 255);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, TextFade(timedEvent.CurrentFloat), -1f);
            };

            TimedEventHandler.Add(0, 13, 346, 0, 14, 013);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, 255, -1f);
            };

            TimedEventHandler.Add(0, 14, 013, 0, 14, 556);
            TimedEventHandler.Last.SetFloat(0, 255);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, 255, -1f);
                TextBlock(DateBlock, TextFade(timedEvent.CurrentFloat), -1f);
            };

            TimedEventHandler.Add(0, 14, 556, 0, 15, 348);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, 255, -1f);
                TextBlock(DateBlock, 255, -1f);
            };

            TimedEventHandler.Add(0, 15, 348, 0, 15, 849);
            TimedEventHandler.Last.SetFloat(0, 255);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, 255, -1f);
                TextBlock(DateBlock, 255, -1f);
                TextBlock(TimeBlock, TextFade(timedEvent.CurrentFloat), -1f);
            };

            TimedEventHandler.Add(0, 15, 849, 0, 16, 975);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, 255, -1f);
                TextBlock(DateBlock, 255, -1f);
                TextBlock(TimeBlock, 255, -1f);
            };

            TimedEventHandler.Add(0, 16, 975, 0, 17, 600);
            TimedEventHandler.Last.SetFloat(255, 0);
            TimedEventHandler.Last.OnExecute += (TimedEvent timedEvent) =>
            {
                TextBlock(DayBlock, TextFade(timedEvent.CurrentFloat), -1f);
                TextBlock(DateBlock, TextFade(timedEvent.CurrentFloat), -1f);
                TextBlock(TimeBlock, TextFade(timedEvent.CurrentFloat), -1f);
            };

            TimedEventHandler.Add(0, 18, 750, 0, 22, 45);
            TimedEventHandler.Last.OnExecute += End_OnExecute;

            Tick += TimeText_Tick;

            Me = this;
        }

        private void TimeText_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading || Main.FirstMission || !IsPlaying)
                return;

            Function.Call(Hash.HIDE_LOADING_ON_FADE_THIS_FRAME);
            TimedEventHandler.RunEvents();
        }

        public void Start(bool shortIntro = false, DateTime forceDate = default)
        {
            if (IsPlaying)
                return;

            FusionUtils.HideGUI = true;

            Game.TimeScale = 1;

            Screen.FadeOut(0);

            date = forceDate == default ? FusionUtils.CurrentTime : forceDate;

            if (shortIntro)
            {
                TimedEventHandler.CurrentTime = new TimeSpan(0, 0, 0, 12, 762);

                /*timeParadoxShort.SourceEntity = FusionUtils.PlayerPed;
                timeParadoxShort.Volume = 0.2f;*/
                timeParadoxShort.Play();
            }
            else
            {
                /*timeParadox.SourceEntity = FusionUtils.PlayerPed;
                timeParadox.Volume = 0.2f;*/
                timeParadox.Play();
            }

            IsPlaying = true;
        }

        private void End_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            Function.Call(Hash.RESET_SCRIPT_GFX_ALIGN);

            if (Screen.IsFadedOut && !Screen.IsFadingIn)
                Screen.FadeIn(1000);

            TimedEventHandler.ResetExecution();
            IsPlaying = false;

            FusionUtils.CurrentTime = date;
            FusionUtils.HideGUI = false;
        }

        private float TextFade(float value)
        {
            if (value > 0 && value < 255)
                return value;
            else if (value >= 255)
                return 255;
            else
                return 0;
        }

        private void TextBlock(string input, float alpha, float lines = 1)
        {
            // -1f to 1f.
            // X - Horizontal
            // Y - Vertical

            Function.Call(Hash.SET_TEXT_SCALE, 1.25f, 1.25f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, (int)alpha);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING"); // draw crap on screen
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, input);
            var fVar1 = Function.Call<float>(Hash.GET_RENDERED_CHARACTER_HEIGHT, 1.25f, 0);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.5f - (fVar1 * lines), 0);
        }
    }
}
