using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pimax.FactoryTool.KingdeePrinter.Models;
using Pimax.FactoryTool.KingdeePrinter.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Pimax.FactoryTool.KingdeePrinter.ViewModels
{
    public class KingdeePrintPageViewModel : ObservableObject
    {
        public KingdeePrintPageViewModel()
        {
            OperationLogs = new ObservableCollection<BindingInfo>();
            AttachPrintCommand = new RelayCommand(AttachPrint);
            if (LabelList.Count > 0)
            {
                SelectedLabel = LabelList[0];
            }

            SetNextKingdeeNumber();
        }

        private readonly DbRepository _repository = new DbRepository();

        public ObservableCollection<BindingInfo> OperationLogs { get; }

        public List<BartenderLabel> LabelList => AppConfig.BartenderLabelList;

        public event Action PrintFinished;

        private BartenderLabel selectedLabel;
        public BartenderLabel SelectedLabel
        {
            get => selectedLabel;
            set
            {
                SetProperty(ref selectedLabel, value);
                OnPropertyChanged(nameof(CanPrint));
            }
        }

        private string orderNumber;
        public string OrderNumber
        {
            get => orderNumber;
            set
            {
                SetProperty(ref orderNumber, value);
                OnPropertyChanged(nameof(CanPrint));
            }
        }

        private string serialNumber;
        public string SerialNumber
        {
            get => serialNumber;
            set
            {
                SetProperty(ref serialNumber, value);
                OnPropertyChanged(nameof(CanPrint));
            }
        }

        private string kingdeeNumber;
        public string KingdeeNumber
        {
            get => kingdeeNumber;
            set
            {
                SetProperty(ref kingdeeNumber, value);
                OnPropertyChanged(nameof(CanPrint));
            }
        }

        public bool CanPrint =>
            IsReady && SelectedLabel != null
            && !string.IsNullOrWhiteSpace(OrderNumber)
            && !string.IsNullOrWhiteSpace(SerialNumber)
            && !string.IsNullOrWhiteSpace(KingdeeNumber);

        private bool isReady = true;
        public bool IsReady
        {
            get => isReady;
            set
            {
                SetProperty(ref isReady, value);
                OnPropertyChanged(nameof(CanPrint));
            }
        }

        public RelayCommand AttachPrintCommand { get; }

        private async void AttachPrint()
        {
            if (!CanPrint) return;

            try
            {
                IsReady = false;
                AppMessage.Clear();

                var check = await MesProxy.CheckSNAsync(SerialNumber, AppConfig.ProductModel, AppConfig.Workstation);
                if (!check.CheckOK)
                {
                    AppMessage.Show(AppMessageType.Error, check.Message);
                    return;
                }

                var bind = await _repository.GetMaterialBindingBySNAsync(SerialNumber);
                if (bind != null)
                {
                    AppMessage.Show(AppMessageType.Error, "此整机SN已绑定金蝶KN");
                    return;
                }

                bind = await _repository.GetMaterialBindingByKNAsync(KingdeeNumber);
                if (bind != null)
                {
                    AppMessage.Show(AppMessageType.Error, "此金蝶KN已绑定整机SN");
                    return;
                }

                //取消白名单上传
                //if (SelectedLabel.WhiteList)
                //{
                //    await PimaxApiService.AddWhiteList(SerialNumber);   //添加白名单
                //}
                var bindInfo = await AttachAsync();     //绑定KN
                await PrintAsync();     //打印条码
                OperationLogs.Insert(0, bindInfo);

                var setResult = await MesProxy.SetTestResultAsync(SerialNumber, check.FlowNo);  //更新过站
                if (!setResult.SetOk)
                {
                    AppMessage.Show(AppMessageType.Error, setResult.Message);
                    return;
                }

                AppMessage.Show(AppMessageType.Succeed, "操作成功");
                AppConfig.KNGenerator.PrintFinish();
                SetNextKingdeeNumber();
            }
            catch (Exception e)
            {
                AppMessage.Show(AppMessageType.Error, "Error: " + e.Message);
                throw e;
            }
            finally
            {
                IsReady = true;
                PrintFinished?.Invoke();
            }
        }

        private void SetNextKingdeeNumber()
        {
            try
            {
                KingdeeNumber = AppConfig.KNGenerator.Next();
            }
            catch (BizException e)
            {
                AppMessage.Show(AppMessageType.Error, e.Message);
            }
        }

        private async Task<BindingInfo> AttachAsync()
        {
            var binding = new MaterialBinding
            {
                SerialNumber = SerialNumber,
                BindingType = AppConfig.BindingType,
                BindingNumber = KingdeeNumber,
                ProcessStage = AppConfig.ProcessStage,
                Workstation = AppConfig.Workstation,
                ProcessStep = AppConfig.ProcessStep,
                OperationTime = DateTime.Now
            };
            await _repository.InsertBindingAsync(binding);

            var info = new ProductInfo
            {
                SerialNumber = SerialNumber,
                ProductCode = SelectedLabel.ProductCode,
                OrderCode = OrderNumber,
                //IsAddToWhiteList = SelectedLabel.WhiteList,
                IsAddToWhiteList = false,
                CreateTime = DateTime.Now
            };
            _repository.InsertProductInfo(info);

            await _repository.SaveChangesAsync();

            return new BindingInfo
            {
                Binding = binding,
                Info = info
            };
        }

        private async Task PrintAsync()
        {
            var labelFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Labels\", SelectedLabel.LabelName);
            if (!File.Exists(labelFile))
            {
                AppMessage.Show(AppMessageType.Error, $"未找到标签文件{SelectedLabel.LabelName}");
                return;
            }

            try
            {
                var printComplete = await Task.Run(() => BartenderService.Print(labelFile, new Dictionary<string, string>
                {
                    { "SN", SerialNumber },
                    { "KN", KingdeeNumber }
                }));

                if (!printComplete)
                {
                    AppMessage.Show(AppMessageType.Error, "打印失败");
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error($"打印出错: {e.Message}");
                AppMessage.Show(AppMessageType.Error, $"打印出错: {e.Message}");
                return;
            }
        }
    }
}
