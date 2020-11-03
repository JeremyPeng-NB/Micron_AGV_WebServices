namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class View_TaskType
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TaskID { get; set; }

        [Key]
        [Column(Order = 1)]
        public string TransferTask { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Task { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Priority { get; set; }

        [Key]
        [Column(Order = 4)]
        public string Purpose { get; set; }

        [Key]
        [Column(Order = 5)]
        public string Car { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ActionID { get; set; }

        [Key]
        [Column(Order = 7)]
        public string Action { get; set; }

        [Key]
        [Column(Order = 8)]
        public string NextAction { get; set; }

        public string Destination { get; set; }

        public string ActionType { get; set; }
    }
}
