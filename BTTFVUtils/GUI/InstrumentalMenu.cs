using System.Collections.Generic;
using GTA;
using GTA.Native;

namespace FusionLibrary
{
    public class InstrumentalButton
    {
        public InstrumentalButton(Control control, string tittle)
        {
            Control = control;
            Tittle = tittle;
        }
        public Control Control { get; }
        public string Tittle { get; }
    }
    public class InstrumentalMenu : ScaleformGui
    {
        private readonly List<InstrumentalButton> _buttonList;

        public InstrumentalMenu() : base("instructional_buttons")
        {
            _buttonList = new List<InstrumentalButton>();

            CallFunction("SET_DATA_SLOT_EMPTY");
        }

        public void AddControl(Control control, string tittle)
        {
            _buttonList.Add(new InstrumentalButton(control, tittle));
        }

        public void RemoveControls()
        {
            _buttonList.Clear();
        }

        // Needs to be called on tick to update button icons (Controller / Pc)
        public void UpdatePanel()
        {
            ClearPanel();
            foreach (var button in _buttonList)
            {
                CallFunction("SET_DATA_SLOT", _buttonList.IndexOf(button), GetButtonIdFromControl(button.Control), button.Tittle);
            }
            SetButtons();
        }

        public int GetButtonIdFromControl(Control control)
        {
            var controlName = Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTON, 2, control, true);
            controlName = controlName.Substring(2);

            return int.Parse(controlName);
        }

        public void ClearPanel()
        {
            CallFunction("SET_DATA_SLOT_EMPTY");
        }

        private void SetButtons()
        {
            CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", "-1");
        }
    }
}