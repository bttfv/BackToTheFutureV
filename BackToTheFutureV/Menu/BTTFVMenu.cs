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
            Subtitle = GetMenuTitle();
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");
        }

        public override string GetMenuTitle()
        {
            return TextHandler.Me.GetMenuTitle(InternalName);
        }

        public override string GetMenuDescription()
        {
            return TextHandler.Me.GetMenuDescription(InternalName);
        }

        public override string GetItemTitle(string itemName)
        {
            return TextHandler.Me.GetItemTitle(InternalName, itemName);
        }

        public override string GetItemDescription(string itemName)
        {
            return TextHandler.Me.GetItemDescription(InternalName, itemName);
        }

        public override string GetItemValueTitle(string itemName, string valueName)
        {
            return TextHandler.Me.GetItemValueTitle(InternalName, itemName, valueName);
        }

        public override string GetItemValueDescription(string itemName, string valueName)
        {
            return TextHandler.Me.GetItemValueDescription(InternalName, itemName, valueName);
        }
    }
}
