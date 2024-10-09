using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;

namespace Pimax.FactoryTool.KingdeePrinter.Services
{
    public static class BartenderService
    {
        private static readonly Engine engine;

        static BartenderService()
        {
            try
            {
                engine = new Engine(true);
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("BartenderService.Constructor error:" + e.Message);
                throw e;
            }
        }

        public static bool Print(string labelFile, Dictionary<string, string> variables)
        {
            try
            {
                lock (engine)
                {
                    var format = engine.Documents.Open(labelFile);
                    format.PrintSetup.IdenticalCopiesOfLabel = 1;
                    foreach (var variable in variables)
                    {
                        format.SubStrings[variable.Key].Value = variable.Value;
                    }
                    var res = format.Print();
                    return res == Result.Success;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error("BartenderService.Print error:" + e.Message);
                return false;
            }
        }

        public static void Dispose()
        {
            engine.Stop(SaveOptions.DoNotSaveChanges);
        }
    }
}
