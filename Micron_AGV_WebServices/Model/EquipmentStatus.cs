namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EquipmentStatus
    {
        [Key]
        [Column(Order = 0)]
        public string Place { get; set; }

        [Column(Order = 1)]
        public bool Status { get; set; }
    }
}
