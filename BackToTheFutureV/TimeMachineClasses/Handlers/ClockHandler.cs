using FusionLibrary;
using GTA;
using System;
using System.Windows.Forms;
using static FusionLibrary.FusionEnums;
using Control = GTA.Control;

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

            InstrumentalMenu.AddControl(Control.PhoneCancel, TextHandler.GetLocalizedText("TCDEdit_CancelButton"));
            InstrumentalMenu.AddControl(Control.PhoneSelect, TextHandler.GetLocalizedText("TCDEdit_SaveButton"));
            InstrumentalMenu.AddControl(Control.PhoneOption, TextHandler.GetLocalizedText("Clock_DisableAlarm"));
            InstrumentalMenu.AddControl(Control.PhoneExtraOption, TextHandler.GetLocalizedText("Clock_SetAlarm"));
            InstrumentalMenu.AddControl(Control.PhoneDown, TextHandler.GetLocalizedText("Clock_SubMinutes"));
            InstrumentalMenu.AddControl(Control.PhoneUp, TextHandler.GetLocalizedText("Clock_AddMinutes"));
        }

        public ClockHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        public override void Dispose()
        {

        }

        private void ProcessButton(Keys key)
        {
            if (FusionUtils.IsCameraInFirstPerson() && (key == Keys.O || (IsPlaying && (Game.IsControlJustPressed(Control.PhoneCancel) || Game.IsControlJustPressed(Control.PhoneSelect)))))
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
                oldTime = oldTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime * (TimeHandler.RealTime ? 1 : 30) * Game.TimeScale));

                InstrumentalMenu.UpdatePanel();

                InstrumentalMenu.Render2DFullscreen();

                if (Properties.AlarmSet)
                    TextHandler.ShowHelp("Clock_AlarmSetTo", true, Properties.AlarmTime.ToShortTimeString());

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
                        TextHandler.ShowNotification("Clock_AlarmSet");
                    }

                    if (Game.IsControlJustPressed(Control.PhoneOption))
                    {
                        Properties.AlarmSet = false;
                        TextHandler.ShowNotification("Clock_AlarmDisabled");
                    }
                }
            }
            else
                Properties.SpawnTime = Properties.SpawnTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime * (TimeHandler.RealTime ? 1 : 30) * Game.TimeScale));

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

            Props.BulovaClockMinute.setRotation(Coordinate.Y, Properties.SpawnTime.Minute * 6 + (TimeHandler.RealTime ? Properties.SpawnTime.Second * 0.1f : 0));
            Props.BulovaClockHour.setRotation(Coordinate.Y, Properties.SpawnTime.Hour * 30 + Properties.SpawnTime.Minute * 0.5f);
        }

        public override void Stop()
        {

        }
    }
}
