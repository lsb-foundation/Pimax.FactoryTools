using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Pimax.FactoryTool.KingdeePrinter.Models
{
    public class BindingInfo : ObservableObject
    {
        public static event Action SelectedChanged;

        public MaterialBinding Binding { get; set; }
        public ProductInfo Info { get; set; }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                SetProperty(ref selected, value);
                SelectedChanged?.Invoke();
            }
        }
    }
}
