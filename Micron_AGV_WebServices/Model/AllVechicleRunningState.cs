using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Micron_AGV_WebServices.Model
{
    //public class AllVechicleRunningState
    //{
    //    public VechicleRunningState[] result { get; set; }
    //}

    public class AllVechicleRunningState
    {
        public string agvName { get; set; }

        public int entityId { get; set; }

        public int speed { get; set; }

        public int x { get; set; }

        public int y { get; set; }

        public int battery { get; set; }

        //public string error { get; set; }

        public int agvState { get; set; }

        public int enabled { get; set; }
    }
}