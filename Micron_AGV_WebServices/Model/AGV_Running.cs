namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AGV_Running
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string AGVID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string Status { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ActionID { get; set; }

        [Key]
        [Column(Order = 3)]
        public Guid TaskListID { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime InsertTime { get; set; }
    }
}
