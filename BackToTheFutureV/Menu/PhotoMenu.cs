using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemonUI.Menus;
using BackToTheFutureV.TimeMachineClasses;
using LemonUI.Elements;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class PhotoMenu : CustomNativeMenu
    {
        private NativeCheckboxItem Wormhole;
        private NativeCheckboxItem Coils;
        private NativeCheckboxItem Ice;
        //private NativeCheckboxItem FluxCapacitor;
        private NativeCheckboxItem HideHUD;

        private TimeMachine TimeMachine => TimeMachineHandler.CurrentTimeMachine;

        public PhotoMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_PhotoMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += PhotoMenu_Shown;
            OnItemCheckboxChanged += PhotoMenu_OnItemCheckboxChanged;

            Add(Wormhole = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Wormhole"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Wormhole_Description")));
            Add(Coils = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Coils"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Coils_Description")));
            Add(Ice = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Ice"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Ice_Description")));
            //Add(FluxCapacitor = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_FluxCapacitor"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_FluxCapacitor_Description")));
            Add(HideHUD = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_HideHUD"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_HideHUD_Description")));
        }

        private void PhotoMenu_Shown(object sender, EventArgs e)
        {
            Coils.Enabled = TimeMachine.Mods.IsDMC12;
            Ice.Enabled = TimeMachine.Mods.IsDMC12;
            //FluxCapacitor.Enabled = TimeMachine.Mods.IsDMC12;
        }

        private void PhotoMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == Wormhole)
                TimeMachine.Properties.PhotoWormholeActive = Checked;

            if (sender == Coils)
                TimeMachine.Properties.PhotoGlowingCoilsActive = Checked;

            if (sender == Ice)
                TimeMachine.Events.SetFreeze(!TimeMachine.Properties.IsFreezed);

            //if (sender == FluxCapacitor)
            //    TimeMachine.Properties.PhotoFluxCapacitorActive = Checked;

            if (sender == HideHUD)
                Main.HideGui = Checked;
        }

        public override void Tick()
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
            {
                Close();

                return;
            }

            Wormhole.Checked = TimeMachine.Properties.PhotoWormholeActive;
            Coils.Checked = TimeMachine.Properties.PhotoGlowingCoilsActive;
            Ice.Checked = TimeMachine.Properties.IsFreezed;
            //FluxCapacitor.Checked = TimeMachine.Properties.PhotoFluxCapacitorActive;
            HideHUD.Checked = Main.HideGui;
        }
    }
}
