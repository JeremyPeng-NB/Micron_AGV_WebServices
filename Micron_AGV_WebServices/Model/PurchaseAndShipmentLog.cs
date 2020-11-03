namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PurchaseAndShipmentLog")]
    public partial class PurchaseAndShipmentLog
    {
        [Key]
        [Column(Order = 0)]
        public int ID { get; set; }

        public string RFID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Storage { get; set; }

        public DateTime? UpdateTime { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Cargostatus { get; set; }

        public string AGVID { get; set; }
    }
}
