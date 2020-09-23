using BackToTheFutureV.Utility;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{ 
    public class InputHandler : Handler
    {
        public bool InputMode { get; private set; }

        public static string InputBuffer;
        public static bool EnterInputBuffer;

        private string _destinationTimeRaw;
        private int _nextReset;

        public InputHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            
        }

        public void InputDate(DateTime date)
        {
            Sounds.InputEnter.Play();
            Properties.DestinationTime = date;
            InputMode = false;
            _destinationTimeRaw = string.Empty;

            Events.OnDestinationDateChange?.Invoke();
        }

        public override void KeyDown(Keys key)
        {
            if (Main.PlayerVehicle != Vehicle || !Properties.AreTimeCircuitsOn || TcdEditer.IsEditing || Properties.IsRemoteControlled || !Vehicle.IsVisible) return;

            if(key == Keys.Divide && ModSettings.UseInputToggle)
            {
                InputMode = !InputMode;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;

                var inputMode = Game.GetLocalizedString("BTTFV_Input_Mode");
                var on = Game.GetLocalizedString("BTTFV_On");
                var off = Game.GetLocalizedString("BTTFV_Off");
                Utils.DisplayHelpText($"{inputMode} {(InputMode ? on : off)}");
            }

            if((InputMode && ModSettings.UseInputToggle) || !ModSettings.UseInputToggle && !Main.ObjectPool.AreAnyVisible)
            {
                string keyCode = key.ToString();

                if (keyCode.Contains("NumPad") || (keyCode.Contains("D") && keyCode.Where(Char.IsDigit).Count() > 0))
                    ProcessInputNumber(new string(keyCode.Where(Char.IsDigit).ToArray()));

                if (key == Keys.Enter)
                    ProcessInputEnter();
            }
        }

        public void ProcessInputNumber(string number)
        {
            try
            {
                Sounds.Keypad[int.Parse(number)].Play();

                _destinationTimeRaw += number;
                _nextReset = Game.GameTime + 15000;
            }
            catch (Exception)
            {
            }
        }

        public void ProcessInputEnter()
        {
            // If its not a valid length/mode
            if (_destinationTimeRaw.Length != 12 && _destinationTimeRaw.Length != 4 && _destinationTimeRaw.Length != 8)
            {
                Sounds.InputEnterError.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
                return;
            }

            var dateTime = Utils.ParseFromRawString(_destinationTimeRaw, Properties.DestinationTime);

            if (dateTime == null)
            {
                Sounds.InputEnterError.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
            }
            else
            {
                InputDate(dateTime.GetValueOrDefault());
            }
        }

        public override void Process()
        {
            if (Properties.AreTimeCircuitsOn && Main.PlayerVehicle != null && Main.PlayerVehicle == Vehicle)
            {
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

            if (Main.PlayerVehicle == null || !Main.PlayerVehicle.IsTimeMachine() || (Main.PlayerVehicle == Vehicle && !Properties.AreTimeCircuitsOn))
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