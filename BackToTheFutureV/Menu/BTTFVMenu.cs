using FusionLibrary;
using LemonUI.Elements;
using System.Drawing;

namespace BackToTheFutureV
{
    internal abstract class BTTFVMenu : CustomNativeMenu
    {
        public TimeMachine CurrentTimeMachine => TimeMachineHandler.CurrentTimeMachine;

        private static readonly ScaledTexture banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

        public BTTFVMenu(string internalName) : base(TextHandler.Me, internalName, banner)
        {
            Title = null;
        }
    }
}
