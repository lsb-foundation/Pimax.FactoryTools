using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pimax.FactoryTool.KingdeePrinter.Models;
using Pimax.FactoryTool.KingdeePrinter.Services;
using System.Collections.Generic;
using System.Linq;

namespace Pimax.FactoryTool.KingdeePrinter.ViewModels
{
    public class BindingCheckPageViewModel : ObservableObject
    {
        public BindingCheckPageViewModel()
        {
            CheckCommand = new RelayCommand<CheckItem>(Check);
            CheckItems = new List<CheckItem>();
            CheckItems.Add(new CheckItem("整机SN", string.Empty));
            CheckItems.AddRange(AppConfig.BindingCheckDict.Select(c => new CheckItem(c.Key, c.Value)).ToList());
        }

        private string checkResult;
        public string CheckResult
        {
            get => checkResult;
            set => SetProperty(ref checkResult, value);
        }

        private bool isCorrect;
        public bool IsCorrect
        {
            get => isCorrect;
            set => SetProperty(ref isCorrect, value);
        }

        public List<CheckItem> CheckItems { get; private set; }

        public RelayCommand<CheckItem> CheckCommand { get; }

        private void ClearResult()
        {
            CheckResult = string.Empty;
            CheckItems.ForEach(c => c.Result = null);
        }

        private void SetCheckResult(bool isCorrect, string res)
        {
            IsCorrect = isCorrect;
            CheckResult = res;
        }

        private async void Check(CheckItem currentCheckItem)
        {
            if (!CheckInput(currentCheckItem))
            {
                return;
            }

            ClearResult();

            List<MaterialBinding> bindingList = null;
            var serialNumber = CheckItems.FirstOrDefault().Input;

            var check = await MesProxy.CheckSNAsync(serialNumber, AppConfig.ProductModel, AppConfig.Workstation);
            if (!check.CheckOK)
            {
                AppMessage.Show(AppMessageType.Error, check.Message);
                return;
            }

            using (var context = new DbRepository())
            {
                bindingList = await context.GetAllBindingsAsync(serialNumber);
            }

            if (bindingList == null || bindingList.Count == 0)
            {
                SetCheckResult(false, "未检索到此整机SN的绑定记录");
                return;
            }

            foreach (var checkItem in CheckItems.Skip(1))
            {
                var binding = bindingList.FirstOrDefault(b => b.BindingType == checkItem.Type);
                checkItem.Result = binding != null && binding.BindingNumber == checkItem.Input;
            }

            if (CheckItems.Where(i => i.Name != "整机SN").All(i => i.Result.HasValue && i.Result.Value))
            {
                var setResult = await MesProxy.SetTestResultAsync(serialNumber, check.FlowNo);  //更新过站
                if (!setResult.SetOk)
                {
                    AppMessage.Show(AppMessageType.Error, setResult.Message);
                }
                else
                {
                    SetCheckResult(true, "绑定记录校验OK");
                }
            }
            else
            {
                SetCheckResult(false, "绑定记录校验NG");
            }
        }

        private bool CheckInput(CheckItem currentInput)
        {
            currentInput.IsFocus = false;

            //输入整机SN后清空所有其他项目
            if (currentInput.Name == "整机SN")
            {
                CheckItems.ForEach(i =>
                {
                    if (i.Name != "整机SN")
                    {
                        i.Input = string.Empty;
                        i.Result = null;
                    }
                    CheckResult = string.Empty;
                });
            }

            var lastCheckItem = CheckItems.LastOrDefault();
            if (currentInput != lastCheckItem)
            {
                var nextCheckItem = CheckItems[CheckItems.IndexOf(currentInput) + 1];
                nextCheckItem.IsFocus = true;    //选中下一项
                return false;       //只有最后一项输入完成才开始检查
            }
            else
            {
                CheckItems[0].IsFocus = true;   //已经是最后一项，则将第一项选中

                var emptyCheckItem = CheckItems.FirstOrDefault(i => string.IsNullOrEmpty(i.Input));
                if (emptyCheckItem != null)
                {
                    emptyCheckItem.IsFocus = true;  //选中空白项
                    return false;
                }
                return true;
            }
        }
    }

    public class CheckItem : ObservableObject
    {
        private string input;
        public string Input
        {
            get => input;
            set => SetProperty(ref input, value);
        }

        private bool isFocus;
        public bool IsFocus
        {
            get => isFocus;
            set => SetProperty(ref isFocus, value);
        }

        public string Type { get; }

        public string Name { get; }

        private bool? result;
        public bool? Result
        {
            get => result;
            set => SetProperty(ref result, value);
        }

        public CheckItem(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
