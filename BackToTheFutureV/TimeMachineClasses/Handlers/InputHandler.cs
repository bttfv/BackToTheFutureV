using BackToTheFutureV.Settings;
using BackToTheFutureV.Utility;
using FusionLibrary;
using GTA;
using System;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class InputHandler : Handler
    {
        public bool InputMode { get; private set; }

        public static string InputBuffer;
        public static bool EnterInputBuffer;

        private string _destinationTimeRaw;
        private int _nextReset;

        private DateTime _simulateDate;
        private int _simulateDatePos = -1;
        private int _simulateDateCheck;

        public InputHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SimulateInputDate += SimulateDateInput;
        }

        public void InputDate(DateTime date)
        {
            Sounds.InputEnter?.Play();
            Properties.DestinationTime = date;
            InputMode = false;
            _destinationTimeRaw = string.Empty;

            Events.OnDestinationDateChange?.Invoke();            
        }

        public override void KeyDown(Keys key)
        {
            if (Utils.PlayerVehicle != Vehicle || !Properties.AreTimeCircuitsOn || TcdEditer.IsEditing || Properties.IsRemoteControlled || !Vehicle.IsVisible) return;

            if(key == ModControls.InputToggle && ModSettings.UseInputToggle)
            {
                InputMode = !InputMode;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;

                var inputMode = Game.GetLocalizedString("BTTFV_Input_Mode");
                var on = Game.GetLocalizedString("BTTFV_On");
                var off = Game.GetLocalizedString("BTTFV_Off");
                Utils.DisplayHelpText($"{inputMode} {(InputMode ? on : off)}");
            }

            if((InputMode && ModSettings.UseInputToggle) || !ModSettings.UseInputToggle && !CustomNativeMenu.ObjectPool.AreAnyVisible)
            {
                string keyCode = key.ToString();

                if (keyCode.Contains("NumPad") || (keyCode.Contains("D") && keyCode.Where(Char.IsDigit).Count() > 0))
                    ProcessInputNumber(new string(keyCode.Where(Char.IsDigit).ToArray()));

                if (key == Keys.Enter)
                    ProcessInputEnter();
            }
        }

        private void SimulateDateInput(DateTime dateTime)
        {
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
            if (Mods.IsDMC12)
                Utils.PlayerPed.Task.PlayAnimation("veh@low@front_ds@base", "change_station", 8f, -1, AnimationFlags.CancelableWithMovement);

            // If its not a valid length/mode
            if (_destinationTimeRaw.Length != 12 && _destinationTimeRaw.Length != 4 && _destinationTimeRaw.Length != 8)
            {
                Sounds.InputEnterError?.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
                return;
            }

            var dateTime = Utils.ParseFromRawString(_destinationTimeRaw, Properties.DestinationTime);

            if (dateTime == null)
            {
                Sounds.InputEnterError?.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
            }
            else
            {
                InputDate(dateTime.GetValueOrDefault());
            }
        }

        private string DateToInput(DateTime dateTime, int pos)
        {
            return dateTime.ToString("MMddyyyyHHmm")[pos].ToString();
        }

        public override void Process()
        {
            if (Properties.AreTimeCircuitsOn && Utils.PlayerVehicle != null && Utils.PlayerVehicle == Vehicle)
            {
                if (_simulateDatePos > -1)
                {
                    if (_simulateDateCheck < Game.GameTime)
                    {
                        if (_simulateDatePos > 11)
                        {
                            _simulateDatePos = -1;
                            ProcessInputEnter();
                        } else
                        {
                            ProcessInputNumber(DateToInput(_simulateDate, _simulateDatePos));

                            _simulateDatePos++;

                            _simulateDateCheck = Game.GameTime + 200;
                        }
                    }
                }

                if (EnterInputBuffer)
                {
                    EnterInputBuffer = false;
                    ProcessInputEnter();
                }

                if (InputBuffer != null)
                {
                    ProcessInputNumber(InputBuffer);
                    InputBuffer = null;
                }
            }

            if (Utils.PlayerVehicle == null || !Utils.PlayerVehicle.IsTimeMachine() || (Utils.PlayerVehicle == Vehicle && !Properties.AreTimeCircuitsOn))
            {
                if (EnterInputBuffer)
                    EnterInputBuffer = false;

                InputBuffer = null;
            }

            if (Game.GameTime > _nextReset)
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