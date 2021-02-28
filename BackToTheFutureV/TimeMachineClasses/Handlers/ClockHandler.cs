using FusionLibrary;
using GTA;
using System;
using System.Windows.Forms;
using static FusionLibrary.Enums;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    internal class ClockHandler : Handler
    {
        private float gameTimer;
        private int pressedTime;

        private bool save;

        private DateTime oldTime;

        private bool alarmSet;
        private DateTime alarmTime;

        private static readonly InstrumentalMenu InstrumentalMenu;

        static ClockHandler()
        {
            InstrumentalMenu = new InstrumentalMenu();

            InstrumentalMenu.ClearPanel();

            InstrumentalMenu.AddControl(Control.PhoneCancel, "Cancel");
            InstrumentalMenu.AddControl(Control.PhoneSelect, "Save");                       
            InstrumentalMenu.AddControl(Control.PhoneExtraOption, "Set alarm time");
            InstrumentalMenu.AddControl(Control.PhoneDown, "Substract minutes");
            InstrumentalMenu.AddControl(Control.PhoneUp, "Add minutes");
        }

        public ClockHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        public override void Dispose()
        {

        }

        private void ProcessButton(Keys key)
        {
            if (Utils.IsPlayerUseFirstPerson() && (key == Keys.O || (IsPlaying && (Game.IsControlJustPressed(Control.PhoneCancel) || Game.IsControlJustPressed(Control.PhoneSelect)))))
            {
                IsPlaying = !IsPlaying;

                if (Game.IsControlJustPressed(Control.PhoneSelect))
                    save = true;

                if (IsPlaying)
                {
                    oldTime = Properties.SpawnTime;
                    save = false;
                }
                else
                {
                    if (!save)
                        Properties.SpawnTime = oldTime;
                }
            }
        }

        public override void KeyDown(Keys key)
        {
            ProcessButton(key);
        }

        public override void Process()
        {
            if (IsPlaying)
            {
                oldTime = oldTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime * 30 * Game.TimeScale));

                InstrumentalMenu.UpdatePanel();

                InstrumentalMenu.Render2DFullscreen();

                if (alarmSet)
                    Screen.ShowHelpTextThisFrame($"Alarm set to {alarmTime.ToShortTimeString()}");

                ProcessButton(Keys.None);

                Game.DisableAllControlsThisFrame();

                if (Game.GameTime > gameTimer)
                {
                    if (pressedTime > 0 && !Game.IsControlPressed(Control.PhoneUp) && !Game.IsControlPressed(Control.PhoneDown))
                        pressedTime = 0;

                    if (Game.IsControlPressed(Control.PhoneUp))
                    {
                        Properties.SpawnTime = Properties.SpawnTime.AddMinutes(1);
                        pressedTime++;

                        if (pressedTime > 5)
                            gameTimer = Game.GameTime + 10;
                        else
                            gameTimer = Game.GameTime + 150;
                    }

                    if (Game.IsControlPressed(Control.PhoneDown))
                    {
                        Properties.SpawnTime = Properties.SpawnTime.AddMinutes(-1);
                        pressedTime++;

                        if (pressedTime > 5)
                            gameTimer = Game.GameTime + 10;
                        else
                            gameTimer = Game.GameTime + 150;
                    }

                    if (Game.IsControlJustPressed(Control.PhoneExtraOption))
                    {
                        alarmTime = Properties.SpawnTime;
                        Properties.SpawnTime = oldTime;
                        alarmSet = true;
                        GTA.UI.Notification.Show("Alarm time set!");
                    }
                }
            }
            else
                Properties.SpawnTime = Properties.SpawnTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime * 30 * Game.TimeScale));

            DateTime checkTime;

            if (IsPlaying)
                checkTime = oldTime;
            else
                checkTime = Properties.SpawnTime;

            if (alarmTime.Hour == checkTime.Hour && checkTime.Minute >= alarmTime.Minute && checkTime.Minute <= alarmTime.Minute + 2)
            {
                if (!Props.BulovaClockRing.IsPlaying)
                    Props.BulovaClockRing.Play();
            }
            else if (Props.BulovaClockRing.IsPlaying)
                Props.BulovaClockRing.Stop();

            Props.BulovaClockMinute.setRotation(Coordinate.Y, Properties.SpawnTime.Minute * 6);
            Props.BulovaClockHour.setRotation(Coordinate.Y, Properties.SpawnTime.Hour * 30 + Properties.SpawnTime.Minute * 0.5f);
        }

        public override void Stop()
        {

        }
    }
}
