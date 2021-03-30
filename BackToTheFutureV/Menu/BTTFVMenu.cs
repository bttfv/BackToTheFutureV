using FusionLibrary;
using GTA;
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
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Title");
        }

        public override string GetMenuDescription()
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Description");
        }

        public override string GetItemTitle(string itemName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Title");
        }

        public override string GetItemDescription(string itemName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Description");
        }

        public override string GetItemValueTitle(string itemName, string valueName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueName}_Title");
        }

        public override string GetItemValueDescription(string itemName, string valueName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueName}_Description");
        }
    }
}
