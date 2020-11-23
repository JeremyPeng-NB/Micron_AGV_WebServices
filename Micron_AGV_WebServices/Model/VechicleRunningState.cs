using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Micron_AGV_WebServices.Model
{
    public class VechicleRunningState
    {
        public int code { get; set; }
        public string message { get; set; }
        public Runningstatelist runningStateList { get; set; }
    }

    public class Runningstatelist
    {
        public string agvName { get; set; }
        public int entityId { get; set; }
        public float speed { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public int battery { get; set; }
        public Error error { get; set; }
        public int agvState { get; set; }
        public int enabled { get; set; }
    }

    public class Error
    {
        public int id { get; set; }
        public int errorCode { get; set; }
    }
}