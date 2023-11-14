using FusionLibrary;
using GTA;
using GTA.Chrono;
using System;
using System.Linq;
using System.Windows.Forms;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class InputHandler : HandlerPrimitive
    {
        private readonly CrClipAsset inputAnim = new CrClipAsset(clipDictName: "veh@low@front_ds@base", animName: "change_station");
        public bool InputMode { get; private set; }

        public Keys lastInput = Keys.None;

        private string _destinationTimeRaw;
        private int _nextReset;

        private GameClockDateTime _simulateDate;
        private int _simulateDatePos = -1;
        private int _simulateDateCheck;

        public InputHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SimulateInputDate += SimulateDateInput;
        }

        public void InputDate(GameClockDateTime date, InputType inputType)
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

            if (ModSettings.UseInputToggle && (e.KeyCode == ModControls.InputToggle || e.KeyCode == Keys.OemQuestion && ModControls.InputToggle == Keys.Divide))
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

        private void SimulateDateInput(GameClockDateTime dateTime)
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
                Driver?.Task?.PlayAnimation(inputAnim.ClipDictionary.Name, inputAnim.ClipName, 8f, -1, AnimationFlags.Secondary);
            }

            // If it's not a valid length/mode
            if (_destinationTimeRaw.Length != 12 && _destinationTimeRaw.Length != 4 && _destinationTimeRaw.Length != 8)
            {
                Sounds.InputEnterError?.Play();
                InputMode = false;
                _nextReset = 0;
                _destinationTimeRaw = string.Empty;
                return;
            }

            GameClockDateTime? dateTime = FusionUtils.ParseFromRawString(_destinationTimeRaw, Properties.DestinationTime, out InputType inputType);

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

        private string DateToInput(GameClockDateTime dateTime, int pos)
        {
            // Returns a single character from the DateTime string at a time in MMddyyyyHHmm format
            return FusionUtils.ParseToRawString(dateTime)[pos].ToString();
        }

        public override void Tick()
        {
            if (Driver != null && Driver.IsPlayingAnimation(inputAnim) && Game.IsControlJustPressed(GTA.Control.VehicleExit))
            {
                Driver?.Task?.StopScriptedAnimationTask(inputAnim);
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
                        // What happens if I type too many numbers, enter it, then go back and watch Wayback?
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
