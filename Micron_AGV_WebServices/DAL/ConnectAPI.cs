using Micron_AGV_WebServices.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Micron_AGV_WebServices.DAL
{
    public class ConnectAPI
    {
        Micron_AGV_DB _db = new Micron_AGV_DB();

        ReturnMissionResult MissionResult = new ReturnMissionResult();

        public string AddCarTask(string NextAction, string ActionType, string AGVID, string targetEntity)
        {
            HttpWebRequest requestt = WebRequest.Create(ConfigurationManager.AppSettings["ServerIP"]) as HttpWebRequest;

            try
            {
                if (requestt != null)
                {
                    requestt.Method = "POST";
                    requestt.KeepAlive = true;
                    requestt.ContentType = "application/json";

                    var jsonStr = AddTaskJson(ActionType, AGVID, targetEntity);
                    byte[] bs = Encoding.UTF8.GetBytes(jsonStr);

                    using (Stream reqStream = requestt.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                        reqStream.Flush();
                    }

                    using (WebResponse response = requestt.GetResponse())
                    {
                        StreamReader sr = new StreamReader(response.GetResponseStream());
                        string resultt = sr.ReadToEnd();
                        MissionResult = JsonConvert.DeserializeObject<ReturnMissionResult>(resultt);
                        MissionResult.message += "! NextAction:" + NextAction;
                        return MissionResult.ToString();
                    }
                }
                else
                {
                    MissionResult.code = -1;
                    MissionResult.message += "operation failed";
                    return MissionResult.ToString();
                }
            }
            catch (Exception ex)
            {
                MissionResult.code = -1;
                MissionResult.message += ex.ToString();
                return MissionResult.ToString();
            }
        }

        private string AddTaskJson(string ActionType, string AGVID, string targetEntity)
        {
            int ActionID = 0;

            string json = null;

            switch (ActionType)
            {
                case "移動":
                    ActionID = 1; 
                    break;
                case "取貨":
                    ActionID = 2;
                    break;
                case "放貨":
                    ActionID = 3;
                    break;
                default:
                    break;
            }

            if (ActionID != 0)
            {
                var targetInfo = _db.NodeMappings.Where(x => x.StorageBin == targetEntity).FirstOrDefault();

                CarMission CarMission = new CarMission()
                {
                    userId = 1,
                    executeAgv = AGVID,
                    orderType = 1,
                    priority = 0,
                    tasks = new Tasks[]
                    {
                        new Tasks()
                        { 
                            seqNum = 1,
                            targetNodeId = targetInfo.Node,
                            action = ActionID,
                            value = targetInfo.Height
                        }
                    }
                };
                json = JsonConvert.SerializeObject(CarMission);
            }

            //顯示
            //Response.Write(json);

            return json;
        }

        public string MissionCompleteJson(string CompleteStr)
        {
            MissionResult.code = 0;
            MissionResult.message += CompleteStr;
            return MissionResult.ToString();
        }

        public string FailStatusJson(string FailStr)
        {
            MissionResult.code = -1;
            MissionResult.message += FailStr;
            return MissionResult.ToString();
        }
    }
}