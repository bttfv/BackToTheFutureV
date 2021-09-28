using FusionLibrary;
using FusionLibrary.Extensions;
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

        private DateTime tempTime;

        private static readonly InstrumentalMenu InstrumentalMenu;

        static ClockHandler()
        {
            InstrumentalMenu = new InstrumentalMenu();

            InstrumentalMenu.ClearPanel();

            InstrumentalMenu.AddControl(Control.PhoneCancel, TextHandler.GetLocalizedText("TCDEdit_CancelButton"));
            InstrumentalMenu.AddControl(Control.PhoneSelect, TextHandler.GetLocalizedText("TCDEdit_SaveButton"));
            InstrumentalMenu.AddControl(Control.PhoneCameraSelfie, TextHandler.GetLocalizedText("Clock_Sync"));
            InstrumentalMenu.AddControl(Control.PhoneOption, TextHandler.GetLocalizedText("Clock_DisableAlarm"));
            InstrumentalMenu.AddControl(Control.PhoneExtraOption, TextHandler.GetLocalizedText("Clock_SetAlarm"));
            InstrumentalMenu.AddControl(Control.PhoneDown, TextHandler.GetLocalizedText("Clock_SubMinutes"));
            InstrumentalMenu.AddControl(Control.PhoneUp, TextHandler.GetLocalizedText("Clock_AddMinutes"));
            InstrumentalMenu.AddControl(Control.PhoneRight, TextHandler.GetLocalizedText("Clock_AddSeconds"));
            InstrumentalMenu.AddControl(Control.PhoneLeft, TextHandler.GetLocalizedText("Clock_SubSeconds"));
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
                {
                    save = true;
                }

                if (IsPlaying)
                {
                    tempTime = Properties.ClockTime.AddSeconds(-Properties.ClockTime.Second);
                    save = false;

                    if (Properties.AlarmSet)
                    {
                        TextHandler.ShowHelp("Clock_AlarmSetTo", false, Properties.AlarmTime.ToString("hh:mm:ss"));
                    }
                }
                else
                {
                    if (save)
                    {
                        Properties.ClockTime = tempTime;
                    }
                }
            }
        }

        public override void KeyDown(KeyEventArgs e)
        {
            ProcessButton(e.KeyCode);
        }

        public override void Tick()
        {
            if (Properties.AlarmSet && FusionUtils.CurrentTime.Between(new DateTime(1955, 11, 12, 21, 30, 00), new DateTime(1955, 11, 12, 22, 3, 50)) && Properties.AlarmTime.Between(new DateTime(1955, 11, 12, 21, 30, 00), new DateTime(1955, 11, 12, 22, 3, 50)) && Vehicle.GetStreetInfo().Street == ClocktowerMission.LightningRunStreet && !Properties.IsEngineStalling && Vehicle.GetMPHSpeed() == 0 && FusionUtils.Random.NextDouble() > 0.3f)
            {
                Events.SetEngineStall?.Invoke(true);
            }

            if (Properties.SyncWithCurTime)
            {
                Properties.ClockTime = FusionUtils.CurrentTime;
            }
            else
            {
                Properties.ClockTime = Properties.ClockTime.Add(TimeSpan.FromSeconds(Game.LastFrameTime * (TimeHandler.RealTime ? 1 : 30)));
            }

            if (IsPlaying)
            {
                InstrumentalMenu.UpdatePanel();

                InstrumentalMenu.Render2DFullscreen();

                ProcessButton(Keys.None);

                Game.DisableAllControlsThisFrame();

                if (Game.GameTime > gameTimer)
                {
                    if (pressedTime > 0 && !Game.IsControlPressed(Control.PhoneUp) && !Game.IsControlPressed(Control.PhoneDown))
                    {
                        pressedTime = 0;
                    }

                    if (Game.IsControlJustPressed(Control.PhoneCameraSelfie))
                    {
                        Properties.SyncWithCurTime = !Properties.SyncWithCurTime;

                        TextHandler.ShowNotification("Clock_SyncToggle", false, TextHandler.GetOnOff(Properties.SyncWithCurTime));
                    }

                    if (Game.IsControlPressed(Control.PhoneUp))
                    {
                        tempTime = tempTime.AddMinutes(1);
                        pressedTime++;

                        if (pressedTime > 5)
                        {
                            gameTimer = Game.GameTime + 10;
                        }
                        else
                        {
                            gameTimer = Game.GameTime + 150;
                        }
                    }

                    if (Game.IsControlPressed(Control.PhoneDown))
                    {
                        tempTime = tempTime.AddMinutes(-1);
                        pressedTime++;

                        if (pressedTime > 5)
                        {
                            gameTimer = Game.GameTime + 10;
                        }
                        else
                        {
                            gameTimer = Game.GameTime + 150;
                        }
                    }

                    if (Game.IsControlPressed(Control.PhoneRight))
                    {
                        int second = tempTime.Second;

                        second++;

                        if (second > 59)
                        {
                            second = 0;
                        }

                        tempTime = tempTime.AddSeconds(-tempTime.Second).AddSeconds(second);
                        pressedTime++;

                        TextHandler.ShowHelp("Clock_CurSecond", false, tempTime.Second.ToString());

                        if (pressedTime > 5)
                        {
                            gameTimer = Game.GameTime + 10;
                        }
                        else
                        {
                            gameTimer = Game.GameTime + 150;
                        }
                    }

                    if (Game.IsControlPressed(Control.PhoneLeft))
                    {
                        int second = tempTime.Second;

                        second--;

                        if (second < 0)
                        {
                            second = 59;
                        }

                        tempTime = tempTime.AddSeconds(-tempTime.Second).AddSeconds(second);
                        pressedTime++;

                        TextHandler.ShowHelp("Clock_CurSecond", false, tempTime.Second.ToString());

                        if (pressedTime > 5)
                        {
                            gameTimer = Game.GameTime + 10;
                        }
                        else
                        {
                            gameTimer = Game.GameTime + 150;
                        }
                    }

                    if (Game.IsControlJustPressed(Control.PhoneExtraOption) && !Game.IsControlPressed(Control.PhoneCameraSelfie))
                    {
                        Properties.AlarmTime = tempTime;
                        Properties.AlarmSet = true;

                        TextHandler.ShowHelp("Clock_AlarmSetTo", false, Properties.AlarmTime.ToString("hh:mm:ss"));

                        TextHandler.ShowNotification("Clock_AlarmSet");
                    }

                    if (Game.IsControlJustPressed(Control.PhoneOption))
                    {
                        Properties.AlarmSet = false;
                        TextHandler.ShowNotification("Clock_AlarmDisabled");
                    }
                }
            }

            if (TimeHandler.RealTime)
            {
                if (Properties.AlarmSet && Properties.AlarmTime.ToString("hh") == Properties.ClockTime.ToString("hh") && Properties.ClockTime.Minute == Properties.AlarmTime.Minute && Properties.ClockTime.Second >= Properties.AlarmTime.Second && Properties.ClockTime.Second <= Properties.AlarmTime.Second + 5)
                {
                    if (!Props.BulovaClockRing.IsPlaying)
                    {
                        Props.BulovaClockRing.Play();
                    }

                    if (!Sounds.Alarm.IsAnyInstancePlaying)
                    {
                        Sounds.Alarm.Play();
                    }
                }
                else
                {
                    if (Props.BulovaClockRing.IsPlaying)
                    {
                        Props.BulovaClockRing.Stop();
                    }

                    if (Sounds.Alarm.IsAnyInstancePlaying)
                    {
                        Sounds.Alarm.Stop();
                    }
                }
            }
            else
            {
                if (Properties.AlarmSet && Properties.AlarmTime.ToString("hh") == Properties.ClockTime.ToString("hh") && Properties.ClockTime.Minute >= Properties.AlarmTime.Minute && Properties.ClockTime.Minute <= Properties.AlarmTime.Minute + 2)
                {
                    if (!Props.BulovaClockRing.IsPlaying)
                    {
                        Props.BulovaClockRing.Play();
                    }

                    if (!Sounds.Alarm.IsAnyInstancePlaying)
                    {
                        Sounds.Alarm.Play();
                    }
                }
                else
                {
                    if (Props.BulovaClockRing.IsPlaying)
                    {
                        Props.BulovaClockRing.Stop();
                    }

                    if (Sounds.Alarm.IsAnyInstancePlaying)
                    {
                        Sounds.Alarm.Stop();
                    }
                }
            }

            if (IsPlaying)
            {
                Props.BulovaClockMinute.setRotation(Coordinate.Y, tempTime.Minute * 6 + (TimeHandler.RealTime ? tempTime.Second * 0.1f : 0));
                Props.BulovaClockHour.setRotation(Coordinate.Y, tempTime.Hour * 30 + tempTime.Minute * 0.5f);
            }
            else
            {
                Props.BulovaClockMinute.setRotation(Coordinate.Y, Properties.ClockTime.Minute * 6 + (TimeHandler.RealTime ? Properties.ClockTime.Second * 0.1f : 0));
                Props.BulovaClockHour.setRotation(Coordinate.Y, Properties.ClockTime.Hour * 30 + Properties.ClockTime.Minute * 0.5f);
            }
        }

        public override void Stop()
        {

        }
    }
}
