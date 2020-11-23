using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Micron_AGV_WebServices.Model
{
    public class CarMission
    {
        public int userId { get; set; }
        public string executeAgv { get; set; }
        public int orderType { get; set; }
        public int priority { get; set; }
        public Tasks tasks { get; set; }
    }

    public class Tasks
    {
        public int seqNum { get; set; }
        public int targetEntity { get; set; }
        public int action { get; set; }
    }
}