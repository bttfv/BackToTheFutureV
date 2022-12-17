using FusionLibrary;
using GTA;
using GTA.Native;
using System;
using System.Linq;
using System.Windows.Forms;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class InputHandler : HandlerPrimitive
    {
        public bool InputMode { get; private set; }

        public Keys lastInput = Keys.None;

        private string _destinationTimeRaw;
        private int _nextReset;

        private DateTime _simulateDate;
        private int _simulateDatePos = -1;
        private int _simulateDateCheck;

        public InputHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SimulateInputDate += SimulateDateInput;
        }

        public void InputDate(DateTime date, InputType inputType)
        {
            Sounds.InputEnter?.Play();
            Properties.DestinationTime = date;
            InputMode = false;
            _destinationTimeRaw = string.Empty;

            if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
            {
                Events.OnDestinationDateChange?.Invoke(inputType);
            }
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!Properties.AreTimeCircuitsOn || TcdEditer.IsEditing || RCGUIEditer.IsEditing || Properties.IsRemoteControlled || !Vehicle.IsVisible || CustomNativeMenu.ObjectPool.AreAnyVisible)
            {
                return;
            }

            if (ModSettings.UseInputToggle && e.KeyCode == ModControls.InputToggle)
            {
                InputMode = !InputMode;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;

                TextHandler.Me.ShowHelp("InputMode", true, TextHandler.Me.GetOnOff(InputMode));
            }

            if (!ModSettings.UseInputToggle || InputMode)
            {
                string keyCode = e.KeyCode.ToString();

                if (keyCode.Contains("NumPad") || (keyCode.Contains("D") && keyCode.Where(char.IsDigit).Count() > 0))
                {
                    if (lastInput == e.KeyCode)
                    {
                        return;
                    }

                    lastInput = e.KeyCode;
                    ProcessInputNumber(new string(keyCode.Where(char.IsDigit).ToArray()));
                }

                if (e.KeyCode == Keys.Enter)
                {
                    if (lastInput == e.KeyCode)
                    {
                        return;
                    }

                    lastInput = e.KeyCode;
                    ProcessInputEnter();
                }
            }
        }

        private void SimulateDateInput(DateTime dateTime)
        {
            if (_simulateDatePos > -1)
            {
                return;
            }

            _simulateDate = dateTime;
            _simulateDatePos = 0;
            _simulateDateCheck = 0;
        }

        public void ProcessInputNumber(string number)
        {
            try
            {
                Sounds.Keypad[int.Parse(number)]?.Play();

                _destinationTimeRaw += number;
                _nextReset = Game.GameTime + 15000;
            }
            catch (Exception)
            {
            }
        }

        public void ProcessInputEnter()
        {
            if (Mods.IsDMC12 && Driver != null && Driver == FusionUtils.PlayerPed)
            {
                Driver.Task.PlayAnimation("veh@low@front_ds@base", "change_station", 8f, -1, AnimationFlags.AllowRotation);
            }

            // If its not a valid length/mode
            if (_destinationTimeRaw.Length != 12 && _destinationTimeRaw.Length != 4 && _destinationTimeRaw.Length != 8)
            {
                Sounds.InputEnterError?.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
                return;
            }
            // If player tries to set Jan 1 0001 through Jan 3 0001 or year 9999 script breaks; set these dates to invalid to error out TCD input
            if ((_destinationTimeRaw.Length >= 8 && _destinationTimeRaw.Substring(0, 8) == "01010001") || (_destinationTimeRaw.Length >= 8 && _destinationTimeRaw.Substring(0, 8) == "01020001") || (_destinationTimeRaw.Length >= 8 && _destinationTimeRaw.Substring(0, 8) == "01030001") || (_destinationTimeRaw.Length >= 8 && _destinationTimeRaw.Substring(4, 4) == "9999"))
            {
                _destinationTimeRaw = "000000000000";
            }

            DateTime? dateTime = FusionUtils.ParseFromRawString(_destinationTimeRaw, Properties.DestinationTime, out InputType inputType);

            if (dateTime == null)
            {
                Sounds.InputEnterError?.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
            }
            else
            {
                InputDate(dateTime.GetValueOrDefault(), inputType);
            }
        }

        private string DateToInput(DateTime dateTime, int pos)
        {
            return dateTime.ToString("MMddyyyyHHmm")[pos].ToString();
        }

        public override void Tick()
        {
            if (Driver != null && Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Driver, "veh@low@front_ds@base", "change_station", 3) && Game.IsControlJustPressed(GTA.Control.VehicleExit))
            {
                Driver.Task.ClearAnimation("veh@low@front_ds@base", "change_station");
            }

            if (lastInput != Keys.None && !Game.IsKeyPressed(lastInput))
            {
                lastInput = Keys.None;
            }

            if (Properties.AreTimeCircuitsOn)
            {
                if (_simulateDatePos > -1)
                {
                    if (_simulateDateCheck < Game.GameTime)
                    {
                        if (_simulateDatePos > 11)
                        {
                            _simulateDatePos = -1;
                            ProcessInputEnter();
                        }
                        else
                        {
                            ProcessInputNumber(DateToInput(_simulateDate, _simulateDatePos));

                            _simulateDatePos++;

                            _simulateDateCheck = Game.GameTime + 200;
                        }
                    }
                }
            }

            if (_simulateDatePos == -1 && Game.GameTime > _nextReset)
            {
                _destinationTimeRaw = string.Empty;
            }
        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {

        }
    }
}
