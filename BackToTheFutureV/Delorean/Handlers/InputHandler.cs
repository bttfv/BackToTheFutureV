using BackToTheFutureV.Utility;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{ 
    public class InputHandler : Handler
    {
        public bool InputMode { get; private set; }

        public static string InputBuffer;
        public static bool EnterInputBuffer;

        private readonly AudioPlayer _inputEnter;
        private readonly AudioPlayer _inputEnterError;
        private string _destinationTimeRaw;
        private int _nextReset;

        private readonly Dictionary<int, AudioPlayer> _keypadSounds;

        public InputHandler(TimeCircuits circuits) : base(circuits)
        {
            _inputEnter = circuits.AudioEngine.Create("general/timeCircuits/tfcEnter.wav", Presets.Interior);
            _inputEnterError = circuits.AudioEngine.Create("general/timeCircuits/tfcError.wav", Presets.Interior);

            _inputEnter.SourceBone = "bttf_tcd_green";
            _inputEnterError.SourceBone = "bttf_tcd_green";

            _keypadSounds = new Dictionary<int, AudioPlayer>();

            for (int i = 0; i <= 9; i++)
                _keypadSounds[i] = circuits.AudioEngine.Create("general/keypad/" + i + ".wav", Presets.Interior);

            foreach (var keypad in _keypadSounds)
            {
                keypad.Value.Volume = 0.45f;
                keypad.Value.SourceBone = "bttf_tcd_green";
            }
        }

        public void InputDate(DateTime date)
        {
            _inputEnter.Play();
            DestinationTime = date;
            InputMode = false;
            _destinationTimeRaw = string.Empty;

            TimeCircuits.OnDestinationDateChange?.Invoke();
        }

        public override void KeyPress(Keys key)
        {
            if (Main.PlayerVehicle != Vehicle || !IsOn || TcdEditer.IsEditing || IsRemoteControlled || !Vehicle.IsVisible) return;

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

            if((InputMode && ModSettings.UseInputToggle) || !ModSettings.UseInputToggle && !Main.MenuPool.IsAnyMenuOpen())
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
                _keypadSounds[int.Parse(number)].Play();

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
                _inputEnterError.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
                return;
            }

            var dateTime = Utils.ParseFromRawString(_destinationTimeRaw, DestinationTime);

            if (dateTime == null)
            {
                _inputEnterError.Play();
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
            if (IsOn && Main.PlayerVehicle != null && Main.PlayerVehicle == Vehicle)
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

            if (Main.PlayerVehicle == null || !DeloreanHandler.IsVehicleATimeMachine(Main.PlayerVehicle) || (Main.PlayerVehicle == Vehicle && !IsOn))
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
            _inputEnter.Dispose();
            _inputEnterError.Dispose();
            _keypadSounds.Values.ToList().ForEach(x => x?.Dispose());
        }
    }
}