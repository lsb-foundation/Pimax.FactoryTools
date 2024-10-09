using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pimax.FactoryTool.KingdeePrinter.Models
{
    [Table("ProductInfo")]
    public class ProductInfo
    {
        [Key]
        public string SerialNumber { get; set; }
        public string ProductCode { get; set; }
        public string OrderCode { get; set; }
        public bool IsAddToWhiteList { get; set; }
        public DateTime CreateTime { get; set; }

        /*
create table ProductInfo
(
	SerialNumber varchar(200) primary key,
	ProductCode varchar(100) not null,
	OrderCode varchar(200) not null,
	IsAddToWhiteList bit not null,
	CreateTime datetime2 not null
)
        */
    }
}
