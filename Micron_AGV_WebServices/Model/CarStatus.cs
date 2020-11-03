namespace Micron_AGV_WebServices.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CarStatus
    {
        public int ID { get; set; }

        [Required]
        public string AGVID { get; set; }

        [Required]
        public string Power { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime UpdataTime { get; set; }

        [Required]
        public string CarType { get; set; }

        public Guid? TaskListID { get; set; }
    }
}
