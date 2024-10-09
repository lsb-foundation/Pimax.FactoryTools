using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pimax.FactoryTool.KingdeePrinter.Models;
using Pimax.FactoryTool.KingdeePrinter.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pimax.FactoryTool.KingdeePrinter.ViewModels
{
    public class QueryPageViewModel : ObservableObject
    {
        public QueryPageViewModel()
        {
            ProductCodeList = new ObservableCollection<string>();
            QueryResult = new ObservableCollection<BindingInfo>();
            QueryCommand = new RelayCommand(Query);
            ExportCommand = new RelayCommand(Export);
            DetachCommand = new RelayCommand(Detach);
            LoadProductCodes();

            BindingInfo.SelectedChanged += () =>
            {
                OnPropertyChanged(nameof(HasAnySelectedItem));
                OnPropertyChanged(nameof(IsAllSelected));
            };

            FromDate = DateTime.Today;
            ToDate = DateTime.Today.AddDays(1);
        }

        public ObservableCollection<string> ProductCodeList { get; }

        private string selectedProductCode;
        public string SelectedProductCode
        {
            get => selectedProductCode;
            set => SetProperty(ref selectedProductCode, value);
        }

        private string serialNumber;
        public string SerialNumber
        {
            get => serialNumber;
            set => SetProperty(ref serialNumber, value);
        }

        private string orderNumber;
        public string OrderNumber
        {
            get => orderNumber;
            set => SetProperty(ref orderNumber, value);
        }

        private string kingdeeNumber;
        public string KingdeeNumber
        {
            get => kingdeeNumber;
            set => SetProperty(ref kingdeeNumber, value);
        }

        private DateTime fromDate;
        public DateTime FromDate
        {
            get => fromDate;
            set => SetProperty(ref fromDate, value);
        }

        private DateTime toDate;
        public DateTime ToDate
        {
            get => toDate;
            set => SetProperty(ref toDate, value);
        }

        public ObservableCollection<BindingInfo> QueryResult { get; }

        public bool IsAllSelected
        {
            get
            {
                if (QueryResult.Count == 0) return false;
                return QueryResult.All(r => r.Selected);
            }
            set
            {
                var results = QueryResult.Where(r => r.Selected != value);
                foreach (var r in results)
                {
                    r.Selected = value;
                }
            }
        }

        public bool HasAnySelectedItem => QueryResult.Any(r => r.Selected);

        public RelayCommand QueryCommand { get; }
        public RelayCommand ExportCommand { get; }
        public RelayCommand DetachCommand { get; }

        private void LoadProductCodes()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Run(async () =>
            {
                try
                {
                    using (var context = new DbRepository())
                    {
                        var products = await context.GetAllProductCodeAsync();
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var code in products)
                            {
                                ProductCodeList.Add(code);
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    Serilog.Log.Logger.Error("LoadProductCode error: " + e.Message);
                }
            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }

        private async void Query()
        {
            QueryResult.Clear();

            using (var context = new DbRepository())
            {
                var query = from m in context.MaterialBindings
                            join p in context.ProductInfos on m.SerialNumber equals p.SerialNumber
                            where m.BindingType == AppConfig.BindingType
                            select new BindingInfo
                            {
                                Binding = m,
                                Info = p
                            };

                if (!string.IsNullOrEmpty(SelectedProductCode)) query = query.Where(i => i.Info.ProductCode == selectedProductCode);
                if (!string.IsNullOrEmpty(OrderNumber)) query = query.Where(i => i.Info.OrderCode == OrderNumber);
                if (!string.IsNullOrEmpty(SerialNumber)) query = query.Where(i => i.Binding.SerialNumber == SerialNumber);
                if (!string.IsNullOrEmpty(KingdeeNumber)) query = query.Where(i => i.Binding.BindingNumber == KingdeeNumber);

                //输入SN或者KN时不按照日期查询
                if (string.IsNullOrEmpty(SerialNumber) && string.IsNullOrEmpty(KingdeeNumber))
                {
                    query = query.Where(i => i.Binding.OperationTime >= FromDate && i.Binding.OperationTime < ToDate);
                }
                query = query.OrderByDescending(i => i.Binding.OperationTime);

                foreach (var item in await query.ToListAsync())
                {
                    QueryResult.Add(item);
                }
            }
        }

        private void Export()
        {
            if (QueryResult.Count == 0) return;

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.AddWorksheet("Sheet 1");
                sheet.Cell("A1").Value = "整机SN";
                sheet.Cell("B1").Value = "金蝶KN";
                sheet.Cell("C1").Value = "产品料号";
                sheet.Cell("D1").Value = "订单号";
                //sheet.Cell("E1").Value = "是否上传白名单";
                sheet.Cell("E1").Value = "操作时间";

                for (int index = 0; index < QueryResult.Count; index++)
                {
                    sheet.Cell(index + 2, 1).Value = QueryResult[index].Binding.SerialNumber;
                    sheet.Cell(index + 2, 2).Value = QueryResult[index].Binding.BindingNumber;
                    sheet.Cell(index + 2, 3).Value = QueryResult[index].Info.ProductCode;
                    sheet.Cell(index + 2, 4).Value = QueryResult[index].Info.OrderCode;
                    //sheet.Cell(index + 2, 5).Value = QueryResult[index].Info.IsAddToWhiteList ? "是" : "否";
                    sheet.Cell(index + 2, 5).Value = QueryResult[index].Binding.OperationTime;
                }

                var exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Exports\");
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                var filename = exportPath + $"KN码绑定数据-{DateTime.Now: yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(filename);
                Process.Start("explorer.exe", filename);
            }
        }

        private async void Detach()
        {
            if (!HasAnySelectedItem) return;

            var selectedResults = QueryResult.Where(r => r.Selected);
            using (var context = new DbRepository())
            {
                await context.DeleteBindingsAsync(selectedResults.Select(r => r.Binding));
                await context.DeleteProductInfoAsync(selectedResults.Select(r => r.Info));
                await context.SaveChangesAsync();
            }

            while (QueryResult.Any(r => r.Selected))
            {
                QueryResult.Remove(QueryResult.FirstOrDefault(r => r.Selected));
            }

            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(HasAnySelectedItem));
        }
    }
}
