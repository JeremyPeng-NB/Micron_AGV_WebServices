namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskType")]
    public partial class TaskType
    {
        [Key]
        public int TaskID { get; set; }

        [Required]
        public string TransferTask { get; set; }

        [Required]
        public string Task { get; set; }

        public int Priority { get; set; }

        [Required]
        public string Purpose { get; set; }

        [Required]
        public string Car { get; set; }
    }
}
