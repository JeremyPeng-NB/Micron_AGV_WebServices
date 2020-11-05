namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class View_TaskList
    {
        [Key]
        [Column(Order = 0)]
        public Guid TaskListID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TaskID { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Task { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string TaskAcceptance { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime AcceptanceTime { get; set; }
        [Key]
        [Column(Order = 5)]
        public DateTime? StartTime { get; set; }
        [Key]
        [Column(Order = 6)]
        public DateTime? EndTime { get; set; }



        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Priority { get; set; }

        [Key]
        [Column(Order = 8)]
        public string Purpose { get; set; }

        [Key]
        [Column(Order = 9)]
        public string Car { get; set; }
    }
}
