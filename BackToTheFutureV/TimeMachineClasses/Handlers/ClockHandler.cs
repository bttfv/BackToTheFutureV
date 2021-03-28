using FusionLibrary;
using GTA;
using GTA.UI;
using System;
using System.Windows.Forms;
using static FusionLibrary.Enums;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV
{
    internal class ClockHandler : HandlerPrimitive
    {
        private float gameTimer;
        private int pressedTime;

        private bool save;

        private DateTime oldTime;

        private static readonly InstrumentalMenu InstrumentalMenu;

        static ClockHandler()
        {
            InstrumentalMenu = new InstrumentalMenu();

            InstrumentalMenu.ClearPanel();

            InstrumentalMenu.AddControl(Control.PhoneCancel, "Cancel");
            InstrumentalMenu.AddControl(Control.PhoneSelect, "Save");
            InstrumentalMenu.AddControl(Control.PhoneOption, "Disable alarm");
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
            if (Utils.IsCameraInFirstPerson() && (key == Keys.O || (IsPlaying && (Game.IsControlJustPressed(Control.PhoneCancel) || Game.IsControlJustPressed(Control.PhoneSelect)))))
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

        public override void KeyDown(KeyEventArgs e)
        {
            ProcessButton(e.KeyCode);
        }

        public override void Tick()
        {
            if (IsPlaying)
            {
                oldTime = oldTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime * 30 * Game.TimeScale));

                InstrumentalMenu.UpdatePanel();

                InstrumentalMenu.Render2DFullscreen();

                if (Properties.AlarmSet)
                    Screen.ShowHelpTextThisFrame($"Alarm set to {Properties.AlarmTime.ToShortTimeString()}");

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
                        Properties.AlarmTime = Properties.SpawnTime;
                        Properties.SpawnTime = oldTime;
                        Properties.AlarmSet = true;
                        Notification.Show("Alarm set!");
                    }

                    if (Game.IsControlJustPressed(Control.PhoneOption))
                    {
                        Properties.AlarmSet = false;
                        Notification.Show("Alarm disabled!");
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

            if (Properties.AlarmSet && Properties.AlarmTime.Hour == checkTime.Hour && checkTime.Minute >= Properties.AlarmTime.Minute && checkTime.Minute <= Properties.AlarmTime.Minute + 2)
            {
                if (!Props.BulovaClockRing.IsPlaying)
                    Props.BulovaClockRing.Play();

                if (!Sounds.Alarm.IsAnyInstancePlaying)
                    Sounds.Alarm.Play();
            }
            else
            {
                if (Props.BulovaClockRing.IsPlaying)
                    Props.BulovaClockRing.Stop();

                if (Sounds.Alarm.IsAnyInstancePlaying)
                    Sounds.Alarm.Stop();
            }

            Props.BulovaClockMinute.setRotation(Coordinate.Y, Properties.SpawnTime.Minute * 6);
            Props.BulovaClockHour.setRotation(Coordinate.Y, Properties.SpawnTime.Hour * 30 + Properties.SpawnTime.Minute * 0.5f);
        }

        public override void Stop()
        {

        }
    }
}
