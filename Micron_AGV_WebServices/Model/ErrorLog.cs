namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ErrorLog")]
    public partial class ErrorLog
    {
        [Key]
        [Column(Order = 0)]
        public DateTime Time { get; set; }

        [Key]
        [Column(Order = 1)]
        public string StorageBin { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Message { get; set; }

        [Key]
        [Column(Order = 3)]
        public string FunctionName { get; set; }
    }
}
