namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskList")]
    public partial class TaskList
    {
        public Guid TaskListID { get; set; }

        public int TaskID { get; set; }

        [Required]
        [StringLength(50)]
        public string TaskAcceptance { get; set; }

        public DateTime AcceptanceTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}