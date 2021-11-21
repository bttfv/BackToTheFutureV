using FusionLibrary;
using LemonUI.Elements;
using System.Drawing;

namespace BackToTheFutureV
{
    internal abstract class BTTFVMenu : CustomNativeMenu
    {
        public BTTFVMenu(string name) : base("")
        {
            InternalName = name;
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");
            CustomText = TextHandler.Me;
            Subtitle = GetMenuTitle();
        }
    }
}
