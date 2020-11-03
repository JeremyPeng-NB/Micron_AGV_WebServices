namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DispatchRecord")]
    public partial class DispatchRecord
    {
        public int ID { get; set; }

        public Guid TaskListID { get; set; }

        [Required]
        public string AGVID { get; set; }

        public int TaskID { get; set; }

        public int ActionID { get; set; }

        public string RFID { get; set; }

        public string Storage { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        public string TaskStatus { get; set; }
    }
}
