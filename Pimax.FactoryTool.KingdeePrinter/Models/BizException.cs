using System;

namespace Pimax.FactoryTool.KingdeePrinter.Models
{
    public class BizException : Exception
    {
        public BizException(string message) : base(message) { }
    }
}
