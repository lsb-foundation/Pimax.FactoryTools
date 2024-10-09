using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Pimax.FactoryTool.KingdeePrinter.Services
{
    public class MesProxy
    {
        private const string ProxyExeFileName = @".\MesProxy\PimaxMesProxy.exe";

        private static Task<string> CallPimaxMesProxyAsync(string arguments)
        {
            var pStartInfo = new ProcessStartInfo(ProxyExeFileName)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (!string.IsNullOrWhiteSpace(arguments)) pStartInfo.Arguments = arguments;

            using (var process = Process.Start(pStartInfo))
            {
                var builder = new StringBuilder();
                process.OutputDataReceived += (s, e) => builder.Append(e.Data);
                process.ErrorDataReceived += (s, e) => builder.Append(e.Data);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                process.CancelOutputRead();
                process.CancelErrorRead();

                return Task.FromResult(builder.ToString());
            }
        }

        public static async Task<(bool CheckOK, string FlowNo, string Message)>
            CheckSNAsync(string serialNumber, string productModel, string workStation)
        {
            var arguments = $"-FunctionName CheckSN -serialNumber {serialNumber} -productModel {productModel} -workstation {workStation}";
            var checkResult = await CallPimaxMesProxyAsync(arguments);
            Serilog.Log.Logger.Information($"MesProxy.CheckSNAsync: SerialNumber={serialNumber},Result={checkResult}");
            if (checkResult.StartsWith("ERROR"))
            {
                return (false, string.Empty, checkResult);
            }
            return (true, checkResult, string.Empty);
        }

        public static async Task<(bool SetOk, string Message)>
            SetTestResultAsync(string serialNumber, string flowNo, string failItem = "")
        {
            var arguments = $"-FunctionName SetTestResult -serialNumber {serialNumber} -flowNo {flowNo}";
            if (!string.IsNullOrEmpty(failItem))
            {
                arguments += $" -failItem {failItem}";
            }
            var setResult = await CallPimaxMesProxyAsync(arguments);
            Serilog.Log.Logger.Information($"MesProxy.SetTestResultAsync: SerialNumber={serialNumber},FlowNo={flowNo},FailItem={failItem},Result={setResult}");
            if (setResult.StartsWith("ERROR"))
            {
                return (false, setResult);
            }
            return (true, setResult);
        }
    }
}
