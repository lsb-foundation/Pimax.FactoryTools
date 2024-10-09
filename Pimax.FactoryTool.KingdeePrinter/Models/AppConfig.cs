using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;

namespace Pimax.FactoryTool.KingdeePrinter.Models
{
    public static class AppConfig
    {
        public static string ProductModel { get; private set; }
        public static string ProductType { get; private set; }
        public static string ProcessStep { get; private set; }
        public static string ProcessStage { get; private set; }
        public static string Workstation { get; private set; }
        public static string BindingType { get; private set; }
        public static string PimaxClientId { get; private set; }
        public static string PimaxClientSecret { get; private set; }

        [IgnoreConfig]
        public static List<BartenderLabel> BartenderLabelList { get; } = new List<BartenderLabel>();

        [IgnoreConfig]
        public static Dictionary<string, string> BindingCheckDict { get; } = new Dictionary<string, string>();

        [IgnoreConfig]
        public static KNGeneratorConfig KNGenerator { get; private set; }

        public static void Initialize()
        {
            var staticProperties = typeof(AppConfig).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var p in staticProperties)
            {
                var ignoreAttr = p.GetCustomAttribute<IgnoreConfigAttribute>();
                if (ignoreAttr != null) continue;

                var value = ConfigurationManager.AppSettings[p.Name];
                if (value is null)
                {
                    throw new BizException($"App.config: appSettings {p.Name} not exist");
                }

                p.SetValue(null, value);
            }

            IntializeBartenderLabel();
            InitializeBindingCheck();
            KNGenerator = KNGeneratorConfig.Get();
        }

        private static void IntializeBartenderLabel()
        {
            var section = ConfigurationManager.GetSection("Bartender") as BartenderSection;
            foreach (BartenderLabel label in section.Labels)
            {
                BartenderLabelList.Add(label);
            }
        }

        private static void InitializeBindingCheck()
        {
            var section = ConfigurationManager.GetSection("BindingCheck") as NameValueCollection;
            foreach (var key in section.AllKeys)
            {
                BindingCheckDict.Add(key, section[key]);
            }
        }
    }

    public class IgnoreConfigAttribute : Attribute { }

    public class BartenderSection : ConfigurationSection
    {
        [ConfigurationProperty("labels")]
        [ConfigurationCollection(typeof(BartenderLabelCollection), AddItemName = "label")]
        public BartenderLabelCollection Labels => (BartenderLabelCollection)this["labels"];
    }

    public class BartenderLabel : ConfigurationElement
    {
        [ConfigurationProperty("productCode", IsRequired = true)]
        public string ProductCode
        {
            get => (string)base["productCode"];
            set => base["productCode"] = value;
        }

        [ConfigurationProperty("labelName", IsRequired = true)]
        public string LabelName
        {
            get => (string)base["labelName"];
            set => base["labelName"] = value;
        }

        //[ConfigurationProperty("whiteList", IsRequired = true, DefaultValue = false)]
        //public bool WhiteList
        //{
        //    get => (bool)base["whiteList"];
        //    set => base["whiteList"] = value;
        //}
    }

    public class BartenderLabelCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BartenderLabel();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BartenderLabel)element).ProductCode;
        }

        public ConfigurationElement this[int index]
        {
            get => BaseGet(index);
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(value);
            }
        }

        public new ConfigurationElement this[string key]
        {
            get => BaseGet(key);
            set
            {
                if (BaseGet(key) is ConfigurationElement element)
                {
                    BaseRemoveAt(BaseIndexOf(element));
                }
                BaseAdd(value);
            }
        }
    }
}
