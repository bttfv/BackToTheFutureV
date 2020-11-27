using GTA;
using System.Collections.Generic;

namespace FusionLibrary
{
    public delegate void OnControlJustPressed();
    public delegate void OnControlJustReleased();
    public delegate void OnControlPressed();
    public delegate void OnControlLongPressed();

    public class NativeInput
    {
        private static List<NativeInput> nativeInputs = new List<NativeInput>();

        internal static void ProcessAll()
        {
            nativeInputs.ForEach(x => x.Process());
        }

        public Control Input { get; protected set; }

        public bool DisableControl { get; protected set; }

        public bool DisableLongpressControl { get; protected set; }

        public OnControlJustPressed OnControlJustPressed{ get; set; }
        public OnControlJustReleased OnControlJustReleased { get; set; }
        public OnControlLongPressed OnControlLongPressed { get; set; }
        public OnControlPressed OnControlPressed { get; set; }

        private int pressedFor;
        private bool registerRelease;

        public NativeInput(Control input, bool disableControl = false, bool disableLongpressControl = false)
        {
            Input = input;
            DisableControl = disableControl;
            DisableLongpressControl = disableLongpressControl;

            nativeInputs.Add(this);
        }

        internal void Process()
        {
            if (DisableControl)
                Game.DisableControlThisFrame(Input);

            if (Game.IsControlJustPressed(Input))
            {
                pressedFor = 0;
                registerRelease = true;
                OnControlJustPressed?.Invoke();
            }

            if (Game.IsControlJustReleased(Input))
            {
                if(pressedFor < 500 && registerRelease)
                {
                    pressedFor = 0;
                    OnControlJustReleased?.Invoke();
                    registerRelease = false;
                }
            }

            if (Game.IsControlPressed(Input))
            {
                pressedFor += (int)(Game.LastFrameTime * 1000f);
                OnControlPressed?.Invoke();

                if (pressedFor >= 100 && DisableLongpressControl)
                    Game.DisableControlThisFrame(Input);
            }

            if(pressedFor >= 500)
            {
                OnControlLongPressed?.Invoke();
                pressedFor = 0;
                registerRelease = false;
            }
        }
    }
}
