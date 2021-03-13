using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using FusionLibrary;
using GTA;
using GTA.UI;
using System.Drawing;

namespace BackToTheFutureV.Settings
{
    internal class RCGUIEditer
    {
        static RCGUIEditer()
        {
            InstrumentalMenu = new InstrumentalMenu();
            AddButtons();
        }

        public static bool IsEditing { get; private set; }

        private static readonly InstrumentalMenu InstrumentalMenu;

        private static int exitDelay = 500;

        private static PointF origPos;
        private static float origScale;

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
                ModSettings.RCGUIPosition = origPos;
                ModSettings.RCGUIScale = origScale;

                Notification.Show(Game.GetLocalizedString("BTTFV_MENU_TCDEditMode_Cancel"));

                Save();
                return;
            }

            origPos = ModSettings.RCGUIPosition;
            origScale = ModSettings.RCGUIScale;

            exitDelay = Game.GameTime + 500;

            posX = origPos.X;
            posY = origPos.Y;
            scale = ModSettings.RCGUIScale;

            IsEditing = true;
        }

        public static void ResetToDefault()
        {
            ModSettings.RCGUIPosition = new PointF(0.901f, 0.879f);
            ModSettings.RCGUIScale = 0.15f;

            ModSettings.SaveSettings();
        }

        private static void AddButtons()
        {
            InstrumentalMenu.ClearPanel();

            InstrumentalMenu.AddControl(Control.PhoneDown, Game.GetLocalizedString("BTTFV_TCD_Edit_MoveDown"));
            InstrumentalMenu.AddControl(Control.PhoneUp, Game.GetLocalizedString("BTTFV_TCD_Edit_MoveUp"));
            InstrumentalMenu.AddControl(Control.PhoneRight, Game.GetLocalizedString("BTTFV_TCD_Edit_MoveRight"));
            InstrumentalMenu.AddControl(Control.PhoneLeft, Game.GetLocalizedString("BTTFV_TCD_Edit_MoveLeft"));

            InstrumentalMenu.AddControl(Control.ReplayFOVIncrease, Game.GetLocalizedString("BTTFV_TCD_Edit_ScaleUp"));
            InstrumentalMenu.AddControl(Control.ReplayFOVDecrease, Game.GetLocalizedString("BTTFV_TCD_Edit_ScaleDown"));

            InstrumentalMenu.AddControl(Control.PhoneCancel, Game.GetLocalizedString("BTTFV_TCD_Edit_Cancel"));
            InstrumentalMenu.AddControl(Control.PhoneSelect, Game.GetLocalizedString("BTTFV_TCD_Edit_Save"));
        }

        public static void Tick()
        {
            if (!IsEditing)
                return;

            InstrumentalMenu.UpdatePanel();

            InstrumentalMenu.Render2DFullscreen();

            ScaleformsHandler.RCGUI.Preview();

            Game.DisableAllControlsThisFrame();

            // This is a long mess but i dont think there any other way to write it
            if (Game.IsControlPressed(Control.PhoneUp) && Game.IsControlPressed(Control.PhoneRight)) // Up Right
            {
                multiplier += MultiplierAdd;
                posY -= Offset * multiplier;
                posX += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneUp) && Game.IsControlPressed(Control.PhoneLeft)) // Up Left
            {
                multiplier += MultiplierAdd;
                posY -= Offset * multiplier;
                posX -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneDown) && Game.IsControlPressed(Control.PhoneRight)) // Down Right
            {
                multiplier += MultiplierAdd;
                posY += Offset * multiplier;
                posX += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneDown) && Game.IsControlPressed(Control.PhoneLeft)) // Down Left
            {
                multiplier += MultiplierAdd;
                posY += Offset * multiplier;
                posX -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneUp)) // Up
            {
                multiplier += MultiplierAdd;
                posY -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneDown)) // Down
            {
                multiplier += MultiplierAdd;
                posY += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneLeft)) // Left
            {
                multiplier += MultiplierAdd;
                posX -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.PhoneRight)) // Right
            {
                multiplier += MultiplierAdd;
                posX += Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.ReplayFOVDecrease)) // Scale down
            {
                multiplier += MultiplierAdd;
                scale -= Offset * multiplier;
            }
            else if (Game.IsControlPressed(Control.ReplayFOVIncrease)) // Scale up
            {
                multiplier += MultiplierAdd;
                scale += Offset * multiplier;
            }
            else
            {
                multiplier = 1;
            }

            if (multiplier > MultiplierMax)
                multiplier = MultiplierMax;

            // Limit for scale
            if (scale > 1.0f)
                scale = 1.0f;
            else if (scale < 0.1f)
                scale = 0.1f;

            ModSettings.RCGUIPosition = new PointF(posX, posY);
            ModSettings.RCGUIScale = scale;

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
                Notification.Show(Game.GetLocalizedString("BTTFV_MENU_TCDEditMode_Save"));
            }
        }
    }
}