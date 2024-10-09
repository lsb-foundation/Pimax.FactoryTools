using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Pimax.FactoryTool.MesProxy
{
    /// <summary>
    /// 项目编译为x86架构，用于包装调用Delphi dll (DBServer.dll)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var arguments = GetArguments(args);
                var functionName = arguments["functionname"];
                if (functionName == "CheckSN")
                {
                    var serialNumber = arguments["serialnumber"];
                    var productModel = arguments["productmodel"];
                    var workstation = arguments["workstation"];
                    var res = MesDllProxy.CheckSN(serialNumber, productModel, workstation, "0001", string.Empty, 1);
                    Console.WriteLine(Marshal.PtrToStringAnsi(res));
                }
                else if (functionName == "SetTestResult")
                {
                    var serialNumber = arguments["serialnumber"];
                    var flowNo = arguments["flowno"];

                    var (result, failItem) = arguments.ContainsKey("failitem") ?
                        (0, arguments["failitem"]) :
                        (1, string.Empty);

                    var res = MesDllProxy.SetTestResult(serialNumber, flowNo, result, failItem, 1);
                    Console.WriteLine(Marshal.PtrToStringAnsi(res));
                }
                else
                {
                    Console.WriteLine("ERROR,Incorrect function name");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR," + e.Message);
            }
        }

        static Dictionary<string, string> GetArguments(string[] args)
        {
            var dict = new Dictionary<string, string>();
            for (int index = 0; index < args.Length; index++)
            {
                var argument = args[index];
                if (argument.StartsWith("-"))
                {
                    var key = argument.TrimStart('-').ToLower();
                    dict.Add(key, null);
                }
                else
                {
                    var last = dict.LastOrDefault();
                    if (last.Key != null && last.Value == null)
                    {
                        dict[last.Key] = argument;
                    }
                }
            }
            return dict;
        }
    }

    public static class MesDllProxy
    {
        //函数原型：function CheckSN(Sern: pchar; Model: pchar; Station: pchar; Client: pchar; OrderNO: pchar;connectiontype:integer): pchar stdcall;
        [DllImport("DBServer.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr CheckSN(
            [MarshalAs(UnmanagedType.LPStr)]
            string Sern,
            [MarshalAs(UnmanagedType.LPStr)]
            string Model,
            [MarshalAs(UnmanagedType.LPStr)]
            string Station,
            [MarshalAs(UnmanagedType.LPStr)]
            string Client,
            [MarshalAs(UnmanagedType.LPStr)]
            string OrderNO,
            int connectiontype);

        //函数原型：function SetTestResult(Sern: pchar; FlowNO: pchar; aResult: integer; FailItem: pchar; connectiontype: integer): pchar stdcall;
        [DllImport("DBServer.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr SetTestResult(
            [MarshalAs(UnmanagedType.LPStr)]
            string Sern,
            [MarshalAs(UnmanagedType.LPStr)]
            string FlowNO,
            int aResult,
            [MarshalAs(UnmanagedType.LPStr)]
            string FailItem,
            int connectiontype);
    }
}
