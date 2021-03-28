using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System.Drawing;

namespace BackToTheFutureV
{
    internal class BTTFVMenu : CustomNativeMenu
    {
        public BTTFVMenu(string name) : base("")
        {
            InternalName = name;
            Subtitle = GetMenuTitle();
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");
        }

        public string GetMenuTitle()
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Title");
        }

        public string GetMenuDescription()
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Description");
        }

        public string GetItemTitle(string itemName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Title");
        }

        public string GetItemDescription(string itemName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Description");
        }

        public string GetItemValueTitle(string itemName, string valueName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueName}_Title");
        }

        public string[] GetItemValueTitle(string itemName, params string[] valueNames)
        {
            string[] ret = new string[valueNames.Length];

            for (int i = 0; i < valueNames.Length; i++)
                ret[i] = Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueNames[i]}_Title");

            return ret;
        }

        public string GetItemValueDescription(string itemName, string valueName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueName}_Description");
        }

        public string[] GetItemValueDescription(string itemName, params string[] valueNames)
        {
            string[] ret = new string[valueNames.Length];

            for (int i = 0; i < valueNames.Length; i++)
                ret[i] = Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueNames[i]}_Description");

            return ret;
        }

        public NativeSubmenuItem NewSubmenu(NativeMenu menu, string menuName)
        {
            NativeSubmenuItem item = AddSubMenu(menu);
            item.Title = GetItemTitle(menuName);
            item.Description = GetItemDescription(menuName);

            return item;
        }

        public NativeCheckboxItem NewCheckboxItem(string itemName)
        {
            NativeCheckboxItem item;

            Add(item = new NativeCheckboxItem(GetItemTitle(itemName), GetItemDescription(itemName)));

            return item;
        }

        public NativeCheckboxItem NewCheckboxItem(string itemName, bool isChecked)
        {
            NativeCheckboxItem item;

            Add(item = new NativeCheckboxItem(GetItemTitle(itemName), GetItemDescription(itemName), isChecked));

            return item;
        }

        public NativeListItem<T> NewListItem<T>(string itemName, params T[] itemValues)
        {
            NativeListItem<T> item;

            Add(item = new NativeListItem<T>(GetItemTitle(itemName), GetItemDescription(itemName), itemValues));

            return item;
        }

        public NativeListItem<T> NewListItem<T>(string itemName)
        {
            NativeListItem<T> item;

            Add(item = new NativeListItem<T>(GetItemTitle(itemName), GetItemDescription(itemName)));

            return item;
        }

        public NativeSliderItem NewSliderItem(string itemName, int max, int value)
        {
            NativeSliderItem item;

            Add(item = new NativeSliderItem(GetItemTitle(itemName), GetItemDescription(itemName), max, value));

            return item;
        }

        public NativeItem NewItem(string itemName)
        {
            NativeItem item;

            Add(item = new NativeItem(GetItemTitle(itemName), GetItemDescription(itemName)));

            return item;
        }

        public NativeItem NewItem(int pos, string itemName)
        {
            NativeItem item;

            Add(pos, item = new NativeItem(GetItemTitle(itemName), GetItemDescription(itemName)));

            return item;
        }

        public override void Tick()
        {

        }
    }
}
