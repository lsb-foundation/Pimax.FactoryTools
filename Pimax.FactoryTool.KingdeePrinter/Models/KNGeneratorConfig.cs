using Newtonsoft.Json;
using System;
using System.Collections;
using System.Configuration;
using System.IO;

namespace Pimax.FactoryTool.KingdeePrinter.Models
{
    public class KNGeneratorConfig
    {
        public string StartCode { get; set; }
        public string TerminusCode { get; set; }
        public int Increment { get; set; }

        private static string _current;
        private static bool _isPrint;

        public string Next()
        {
            if (_current == null)
            {
                _current = StartCode;
                _isPrint = false;
                return _current;
            }

            if (!_isPrint) return _current;

            if (_current == TerminusCode)
            {
                throw new BizException("金蝶KN码生成失败：超出范围");
            }

            var serial = _current.Substring(_current.Length - 5, 5);
            if (!int.TryParse(serial, out int iSerial))
            {
                throw new BizException("KNGenerator code format is error");
            }

            var newSerial = (iSerial + Increment).ToString().PadLeft(5, '0');
            _current = _current.Substring(0, _current.Length - 5) + newSerial;
            _isPrint = false;

            return _current;
        }

        public void PrintFinish()
        {
            _isPrint = true;
        }

        public async void Save()
        {
            var saveObj = new KNCodeSaver
            {
                StartCode = StartCode,
                CurrentCode = _current,
                IsPrinted = _isPrint
            };
            var json = JsonConvert.SerializeObject(saveObj);

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KNGenerator.json");
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                }
            }
        }

        public static KNGeneratorConfig Get()
        {
            var section = ConfigurationManager.GetSection("KNGenerator") as IDictionary;
            if (section is null) return null;

            var config = new KNGeneratorConfig();
            config.StartCode = section["StartCode"] as string;
            config.TerminusCode = section["TerminusCode"] as string;
            int.TryParse(section["Increment"] as string, out int increment);
            config.Increment = increment;

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "KNGenerator.json");
            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var obj = JsonConvert.DeserializeObject<KNCodeSaver>(json);

                    if (config.StartCode == obj.StartCode)
                    {
                        _current = obj.CurrentCode;
                        _isPrint = obj.IsPrinted;
                    }
                }
                catch { }
            }

            return config;
        }

        class KNCodeSaver
        {
            public string StartCode { get; set; }
            public string CurrentCode { get; set; }

            /// <summary>
            /// 条码是否已打印使用
            /// </summary>
            public bool IsPrinted { get; set; }
        }
    }
}
