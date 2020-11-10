namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ShelfManagementTEST")]
    public partial class ShelfManagementTEST
    {
        public Guid ID { get; set; }

        public string RFID { get; set; }

        [Required]
        public string Storage { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string Area { get; set; }

        [Required]
        public string WithPackage { get; set; }

        public string Purpose { get; set; }

        public DateTime? UpDataTime { get; set; }

        public string AGVID { get; set; }

        public string Previous_RFID { get; set; }

        public string Previous_Purpose { get; set; }
    }
}
