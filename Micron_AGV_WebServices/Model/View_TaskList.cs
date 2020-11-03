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
        [StringLength(50)]
        public string TaskAcceptance { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime AcceptanceTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Key]
        [Column(Order = 4)]
        public string Task { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Priority { get; set; }

        [Key]
        [Column(Order = 6)]
        public string Purpose { get; set; }

        [Key]
        [Column(Order = 7)]
        public string Car { get; set; }
    }
}
