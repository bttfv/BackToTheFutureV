using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemonUI;
using LemonUI.Elements;
using LemonUI.Menus;

namespace BackToTheFutureV.Menu
{
    public delegate void OnItemSelected(NativeItem sender, SelectedEventArgs e);
    public delegate void OnItemActivated(NativeItem sender, EventArgs e);
    public delegate void OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked);
    public delegate void OnItemValueChanged(NativeSliderItem sender, EventArgs e);

    public abstract class CustomNativeMenu : NativeMenu
    {
        public event OnItemActivated OnItemActivated;
        public event OnItemSelected OnItemSelected;
        public event OnItemCheckboxChanged OnItemCheckboxChanged;
        public event OnItemValueChanged OnItemValueChanged;

        public CustomNativeMenu(string title) : base(title)
        {
        }

        public CustomNativeMenu(string title, string subtitle) : base(title, subtitle)
        {
        }

        public CustomNativeMenu(string title, string subtitle, string description) : base(title, subtitle, description)
        {
        }

        public CustomNativeMenu(string title, string subtitle, string description, I2Dimensional banner) : base(title, subtitle, description, banner)
        {            
        }

        public new void Add(NativeItem nativeItem)
        {
            nativeItem.Activated += NativeItem_Activated;
            nativeItem.Selected += NativeItem_Selected;

            base.Add(nativeItem);
        }

        public void Add(NativeCheckboxItem nativeCheckboxItem)
        {
            nativeCheckboxItem.Activated += NativeItem_Activated;
            nativeCheckboxItem.Selected += NativeItem_Selected;
            nativeCheckboxItem.CheckboxChanged += NativeCheckboxItem_CheckboxChanged;

            base.Add(nativeCheckboxItem);
        }

        public void Add<T>(NativeListItem<T> nativeListItem)
        {
            nativeListItem.Activated += NativeItem_Activated;
            nativeListItem.Selected += NativeItem_Selected;

            base.Add(nativeListItem);
        }

        public void Add(NativeSliderItem nativeSliderItem)
        {
            nativeSliderItem.Activated += NativeItem_Activated;
            nativeSliderItem.Selected += NativeItem_Selected;
            nativeSliderItem.ValueChanged += NativeSliderItem_ValueChanged;

            base.Add(nativeSliderItem);
        }

        public void Add(NativeSlidableItem nativeSlidableItem)
        {
            nativeSlidableItem.Activated += NativeItem_Activated;
            nativeSlidableItem.Selected += NativeItem_Selected;
            
            base.Add(nativeSlidableItem);
        }

        private void NativeSliderItem_ValueChanged(object sender, EventArgs e)
        {
            OnItemValueChanged?.Invoke((NativeSliderItem)((NativeMenu)sender).SelectedItem, e);
        }

        private void NativeCheckboxItem_CheckboxChanged(object sender, EventArgs e)
        {
            OnItemCheckboxChanged?.Invoke((NativeCheckboxItem)sender, e, ((NativeCheckboxItem)sender).Checked);
        }

        private void NativeItem_Selected(object sender, SelectedEventArgs e)
        {
            OnItemSelected?.Invoke(((NativeMenu)sender).SelectedItem, e);
        }

        private void NativeItem_Activated(object sender, EventArgs e)
        {
            OnItemActivated?.Invoke(((NativeMenu)sender).SelectedItem, e);
        }

        public abstract void Tick();
    }
}