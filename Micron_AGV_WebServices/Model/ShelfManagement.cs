namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ShelfManagement")]
    public partial class ShelfManagement
    {
        [Key]
        [Column(Order = 0)]
        public Guid ID { get; set; }

        public string RFID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string Storage { get; set; }

        [Column(Order = 2)]
        public string Status { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Area { get; set; }

        [Key]
        [Column(Order = 4)]
        public string WithPackage { get; set; }

        public string Purpose { get; set; }

        public DateTime? UpDataTime { get; set; }

        public string AGVID { get; set; }
    }
}
