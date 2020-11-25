namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class View_DispatchRecord
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string AGVID { get; set; }

        public string RFID { get; set; }

        public string Storage { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Column(Order = 2)]
        public string TaskStatus { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Action { get; set; }

        [Key]
        [Column(Order = 4)]
        public Guid TaskListID { get; set; }

        public string ActionType { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ActionID { get; set; }
    }
}
