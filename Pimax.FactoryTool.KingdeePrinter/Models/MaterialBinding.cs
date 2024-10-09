using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pimax.FactoryTool.KingdeePrinter.Models
{
    [Table("MaterialBind")]
    public class MaterialBinding
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Column("SerialNo")]
        public string SerialNumber { get; set; }

        [Column("ItemName")]
        public string BindingType { get; set; }

        [Column("SCYH")]
        public string BindingNumber { get; set; }

        [Column("ProductStage")]
        public string ProcessStage { get; set; }

        [Column("WorkStation")]
        public string Workstation { get; set; }

        [Column("StepName")]
        public string ProcessStep { get; set; }

        [Column("CreateTime")]
        public DateTime? OperationTime { get; set; }
    }
}
