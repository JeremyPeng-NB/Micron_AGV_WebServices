namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskProcess")]
    public partial class TaskProcess
    {
        [Key]
        [Column(Order = 0)]
        public int ID { get; set; }

        public int? TaskID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ActionID { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Action { get; set; }

        [Key]
        [Column(Order = 3)]
        public string NextAction { get; set; }

        public string Destination { get; set; }

        public string ActionType { get; set; }
    }
}
