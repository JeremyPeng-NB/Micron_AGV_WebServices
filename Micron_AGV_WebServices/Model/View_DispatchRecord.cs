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
        [Column(Order = 8)]
        public Guid TaskListID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string AGVID { get; set; }



        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Action { get; set; }
        [Column(Order = 2)]
        public string RFID { get; set; }
        [Column(Order = 3)]
        public string Storage { get; set; }
        [Column(Order = 4)]
        public DateTime? StartTime { get; set; }
        [Column(Order = 5)]
        public DateTime? EndTime { get; set; }

        [Key]
        [Column(Order = 6)]
        public string TaskStatus { get; set; }

    }
}
