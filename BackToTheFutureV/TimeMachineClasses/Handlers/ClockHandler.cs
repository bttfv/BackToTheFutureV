using FusionLibrary;
using GTA;
using System;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
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
        public static int finishTime;

        private static readonly InstrumentalMenu InstrumentalMenu;

        static ClockHandler()
        {
            InstrumentalMenu = new InstrumentalMenu();

            InstrumentalMenu.ClearPanel();

            InstrumentalMenu.AddControl(Control.PhoneCancel, TextHandler.Me.GetLocalizedText("TCDEdit_CancelButton"));
            InstrumentalMenu.AddControl(Control.PhoneSelect, TextHandler.Me.GetLocalizedText("TCDEdit_SaveButton"));
            InstrumentalMenu.AddControl(Control.PhoneCameraSelfie, TextHandler.Me.GetLocalizedText("Clock_Sync"));
            InstrumentalMenu.AddControl(Control.PhoneOption, TextHandler.Me.GetLocalizedText("Clock_DisableAlarm"));
            InstrumentalMenu.AddControl(Control.PhoneExtraOption, TextHandler.Me.GetLocalizedText("Clock_SetAlarm"));
            InstrumentalMenu.AddControl(Control.PhoneDown, TextHandler.Me.GetLocalizedText("Clock_SubMinutes"));
            InstrumentalMenu.AddControl(Control.PhoneUp, TextHandler.Me.GetLocalizedText("Clock_AddMinutes"));
            InstrumentalMenu.AddControl(Control.PhoneRight, TextHandler.Me.GetLocalizedText("Clock_AddSeconds"));
            InstrumentalMenu.AddControl(Control.PhoneLeft, TextHandler.Me.GetLocalizedText("Clock_SubSeconds"));
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
                        TextHandler.Me.ShowHelp("Clock_AlarmSetTo", false, Properties.AlarmTime.ToString("hh:mm:ss"));
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
            if (!Props.BulovaClockRing.IsSpawned)
            {
                return;
            }

            if (Game.GameTime < finishTime)
            {
                Game.DisableControlThisFrame(GTA.Control.VehicleCinCam);
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

                        TextHandler.Me.ShowNotification("Clock_SyncToggle", false, TextHandler.Me.GetOnOff(Properties.SyncWithCurTime));
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

                        TextHandler.Me.ShowHelp("Clock_CurSecond", false, tempTime.Second.ToString());

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

                        TextHandler.Me.ShowHelp("Clock_CurSecond", false, tempTime.Second.ToString());

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

                        TextHandler.Me.ShowHelp("Clock_AlarmSetTo", false, Properties.AlarmTime.ToString("hh:mm:ss"));

                        TextHandler.Me.ShowNotification("Clock_AlarmSet");
                    }

                    if (Game.IsControlJustPressed(Control.PhoneOption))
                    {
                        Properties.AlarmSet = false;
                        TextHandler.Me.ShowNotification("Clock_AlarmDisabled");
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
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.BulovaSetup, CameraSwitchType.Instant, 32);
                Props.BulovaClockMinute.SetRotation(Coordinate.Y, (tempTime.Minute * 6) + (TimeHandler.RealTime ? tempTime.Second * 0.1f : 0));
                Props.BulovaClockHour.SetRotation(Coordinate.Y, (tempTime.Hour * 30) + (tempTime.Minute * 0.5f));
                finishTime = Game.GameTime + 256;
            }
            else
            {
                Props.BulovaClockMinute.SetRotation(Coordinate.Y, (Properties.ClockTime.Minute * 6) + (TimeHandler.RealTime ? Properties.ClockTime.Second * 0.1f : 0));
                Props.BulovaClockHour.SetRotation(Coordinate.Y, (Properties.ClockTime.Hour * 30) + (Properties.ClockTime.Minute * 0.5f));
            }
        }

        public override void Stop()
        {

        }
    }
}
