using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    internal class BTTFVMenu : CustomNativeMenu
    {
        public BTTFVMenu(string title) : base("", GetLocalizedMenuTitle(title))
        {
            InternalName = title;
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");
        }

        public static string GetLocalizedText(string entry)
        {
            return Game.GetLocalizedString($"BTTFV_Text_{entry}");
        }

        public static string GetLocalizedMenuTitle(string entry)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{entry}_Title");
        }

        public static string GetLocalizedMenuDescription(string entry)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{entry}_Description");
        }

        public string GetLocalizedItemTitle(string itemName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Title");
        }

        public string GetLocalizedItemDescription(string itemName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Description");
        }

        public string GetLocalizedItemValueTitle(string itemName, string valueName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueName}_Title");
        }

        public string GetLocalizedItemValueDescription(string itemName, string valueName)
        {
            return Game.GetLocalizedString($"BTTFV_Menu_{InternalName}_Item_{itemName}_Value_{valueName}_Description");
        }

        public NativeSubmenuItem NewSubmenu(NativeMenu menu, string menuName)
        {
            NativeSubmenuItem item = AddSubMenu(menu);
            item.Title = GetLocalizedItemTitle(menuName);
            item.Description = GetLocalizedItemDescription(menuName);

            return item;
        }

        public NativeCheckboxItem NewCheckboxItem(string itemName)
        {
            NativeCheckboxItem item;

            Add(item = new NativeCheckboxItem(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName)));

            return item;
        }

        public NativeCheckboxItem NewCheckboxItem(string itemName, bool isChecked)
        {
            NativeCheckboxItem item;

            Add(item = new NativeCheckboxItem(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName), isChecked));

            return item;
        }

        public NativeListItem<T> NewListItem<T>(string itemName, params T[] itemValues)
        {
            NativeListItem<T> item;

            Add(item = new NativeListItem<T>(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName), itemValues));

            return item;
        }

        public NativeListItem<T> NewListItem<T>(string itemName)
        {
            NativeListItem<T> item;

            Add(item = new NativeListItem<T>(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName)));

            return item;
        }

        public NativeSliderItem NewSliderItem(string itemName, int max, int value)
        {
            NativeSliderItem item;

            Add(item = new NativeSliderItem(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName), max, value));

            return item;
        }

        public NativeItem NewItem(string itemName)
        {
            NativeItem item;

            Add(item = new NativeItem(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName)));

            return item;
        }

        public NativeItem NewItem(int pos, string itemName)
        {
            NativeItem item;

            Add(pos, item = new NativeItem(GetLocalizedItemTitle(itemName), GetLocalizedItemDescription(itemName)));

            return item;
        }

        public override void Tick()
        {

        }
    }
}
