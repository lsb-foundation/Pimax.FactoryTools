using Pimax.FactoryTool.KingdeePrinter.ViewModels;
using System.Windows.Controls;

namespace Pimax.FactoryTool.KingdeePrinter.Views
{
    /// <summary>
    /// KingdeePrintPage.xaml 的交互逻辑
    /// </summary>
    public partial class KingdeePrintPage : Page
    {
        KingdeePrintPageViewModel viewModel;

        public KingdeePrintPage()
        {
            InitializeComponent();

            viewModel = DataContext as KingdeePrintPageViewModel;

            viewModel.PrintFinished += () =>
            {
                SerialNumberTextBox.Focus();
                SerialNumberTextBox.SelectAll();
            };
        }
    }
}
