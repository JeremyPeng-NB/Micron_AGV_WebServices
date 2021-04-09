namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NodeMapping")]
    public partial class NodeMapping
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string StorageBin { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Node { get; set; }

        [Key]
        [Column(Order = 2)]
        public double Height { get; set; }
    }
}
