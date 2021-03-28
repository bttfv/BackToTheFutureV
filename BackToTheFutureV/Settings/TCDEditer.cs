using FusionLibrary;
using GTA;
using System.Drawing;

namespace BackToTheFutureV
{
    internal class TcdEditer
    {
        static TcdEditer()
        {
            InstrumentalMenu = new InstrumentalMenu();
            AddButtons();
        }

        public static bool IsEditing { get; private set; }

        private static readonly InstrumentalMenu InstrumentalMenu;

        private static int exitDelay = 500;

        private static PointF origSIDPos;
        private static float origSIDScale;

        private static PointF origPos;
        private static float origScale;

        private static float posSIDX;
        private static float posSIDY;
        private static float scaleSID;

        private static float posX;
        private static float posY;
        private static float scale;

        private const float Offset = 0.00025f;
        private const float MultiplierAdd = 0.35f;
        private const float MultiplierMax = 15f;

        private static float multiplier = 1;

        private static void Save()
        {
            IsEditing = false;

            ModSettings.SaveSettings();
            ModSettings.OnGUIChange?.Invoke();
        }

        public static void SetEditMode(bool toggle)
        {
            if (!toggle)
            {
                ModSettings.SIDPosition = origSIDPos;
                ModSettings.SIDScale = origSIDScale;

                ModSettings.TCDPosition = origPos;
                ModSettings.TCDScale = origScale;

                TextHandler.ShowNotification("TCDEdit_Cancel");

                Save();
                return;
            }

            origSIDPos = ModSettings.SIDPosition;
            origSIDScale = ModSettings.SIDScale;

            origPos = ModSettings.TCDPosition;
            origScale = ModSettings.TCDScale;

            exitDelay = Game.GameTime + 500;

            posSIDX = origSIDPos.X;
            posSIDY = origSIDPos.Y;
            scaleSID = origSIDScale;

            posX = origPos.X;
            posY = origPos.Y;
            scale = ModSettings.TCDScale;

            IsEditing = true;
        }

        public static void ResetToDefault()
        {
            ModSettings.SIDPosition = new PointF(0.947f, 0.437f);
            ModSettings.SIDScale = 0.3f;

            ModSettings.TCDPosition = new PointF(0.88f, 0.75f);
            ModSettings.TCDScale = 0.3f;

            ModSettings.SaveSettings();
        }

        private static void AddButtons()
        {
            InstrumentalMenu.ClearPanel();

            InstrumentalMenu.AddControl(Control.PhoneDown, TextHandler.GetLocalizedText("TCDEdit_MoveDown"));
            InstrumentalMenu.AddControl(Control.PhoneUp, TextHandler.GetLocalizedText("TCDEdit_MoveUp"));
            InstrumentalMenu.AddControl(Control.PhoneRight, TextHandler.GetLocalizedText("TCDEdit_MoveRight"));
            InstrumentalMenu.AddControl(Control.PhoneLeft, TextHandler.GetLocalizedText("TCDEdit_MoveLeft"));

            InstrumentalMenu.AddControl(Control.ReplayFOVIncrease, TextHandler.GetLocalizedText("TCDEdit_ScaleUp"));
            InstrumentalMenu.AddControl(Control.ReplayFOVDecrease, TextHandler.GetLocalizedText("TCDEdit_ScaleDown"));

            InstrumentalMenu.AddControl(Control.PhoneCancel, TextHandler.GetLocalizedText("TCDEdit_Cancel"));
            InstrumentalMenu.AddControl(Control.PhoneSelect, TextHandler.GetLocalizedText("TCDEdit_Save"));
        }

        public static void Tick()
        {
            if (!IsEditing)
                return;

            InstrumentalMenu.UpdatePanel();

            InstrumentalMenu.Render2DFullscreen();

            ScaleformsHandler.GUI.Draw2D();

            ScaleformsHandler.SID2D.Draw2D();

            Game.DisableAllControlsThisFrame();

            float _posX, _posY, _scale;

            if (Game.IsControlPressed(Control.VehicleHandbrake))
            {
                _posX = posSIDX;
                _posY = posSIDY;
                _scale = scaleSID;
            }
            else
            {
                _posX = posX;
                _posY = posY;
                _scale = scale;
            }

            // This is a long mess but i dont think there any other way to write it
            if (Game.IsControlPressed(Control.PhoneUp) && Game.IsControlPressed(Control.PhoneRight)) // Up Right
            {
                multiplier += MultiplierAdd;
                _posY -= Offset * multiplier;
                _posX += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneUp) && Game.IsControlPressed(Control.PhoneLeft)) // Up Left
            {
                multiplier += MultiplierAdd;
                _posY -= Offset * multiplier;
                _posX -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneDown) && Game.IsControlPressed(Control.PhoneRight)) // Down Right
            {
                multiplier += MultiplierAdd;
                _posY += Offset * multiplier;
                _posX += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneDown) && Game.IsControlPressed(Control.PhoneLeft)) // Down Left
            {
                multiplier += MultiplierAdd;
                _posY += Offset * multiplier;
                _posX -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneUp)) // Up
            {
                multiplier += MultiplierAdd;
                _posY -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneDown)) // Down
            {
                multiplier += MultiplierAdd;
                _posY += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneLeft)) // Left
            {
                multiplier += MultiplierAdd;
                _posX -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneRight)) // Right
            {
                multiplier += MultiplierAdd;
                _posX += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.ReplayFOVDecrease)) // Scale down
            {
                multiplier += MultiplierAdd;
                _scale -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.ReplayFOVIncrease)) // Scale up
            {
                multiplier += MultiplierAdd;
                _scale += Offset * multiplier;
            }
            else
            {
                multiplier = 1;
            }

            if (multiplier > MultiplierMax)
                multiplier = MultiplierMax;

            // Limit for scale
            if (_scale > 1.0f)
                _scale = 1.0f;
            else if (_scale < 0.1f)
                _scale = 0.1f;

            if (Game.IsControlPressed(Control.VehicleHandbrake))
            {
                posSIDX = _posX;
                posSIDY = _posY;
                scaleSID = _scale;

                ModSettings.SIDPosition = new PointF(posSIDX, posSIDY);
                ModSettings.SIDScale = scaleSID;
            }
            else
            {
                posX = _posX;
                posY = _posY;
                scale = _scale;

                ModSettings.TCDPosition = new PointF(posX, posY);
                ModSettings.TCDScale = scale;
            }

            // Otherwise game instantly saves changes
            if (Game.GameTime < exitDelay)
                return;

            // Save / Cancel changes
            if (Game.IsControlJustPressed(Control.PhoneCancel))
            {
                SetEditMode(false);
            }
            else if (Game.IsControlJustPressed(Control.PhoneSelect))
            {
                Save();
                TextHandler.ShowNotification("TCDEdit_Save");
            }
        }
    }
}