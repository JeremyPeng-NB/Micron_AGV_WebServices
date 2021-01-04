using Micron_AGV_WebServices.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Micron_AGV_WebServices.DAL
{
    public class ConnectAPI
    {
        public string AGVServerTest()
        {
            HttpWebRequest requestt = WebRequest.Create("http://192.168.12.65:1111/AGVTestAPI") as HttpWebRequest;

            if (requestt != null)
            {
                /*傳遞方式 + 格式*/
                requestt.Method = "GET";
                requestt.KeepAlive = true;
                requestt.ContentType = "application/json";                

                /*接訊息回來*/
                using (WebResponse response = requestt.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string resultt = sr.ReadToEnd();
                    VechicleRunningState VechicleRunningState = JsonConvert.DeserializeObject<VechicleRunningState>(resultt);
                    return "叫車成功!";
                }
            }
            else
            {
                return "叫車失敗!";
            }
        }

        public string AddCarMission(string ActionType, string AGVID, string StorageBin)
        {
            var jsonStr = CombineJsonStr(ActionType, AGVID, StorageBin);

            HttpWebRequest requestt = WebRequest.Create("http://192.168.12.65:1111/AddCarMission") as HttpWebRequest;

            if (requestt != null)
            {
                requestt.Method = "POST";
                requestt.KeepAlive = true;
                requestt.ContentType = "application/json";

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
                    ReturnCarMissionResult CarMissionResult = JsonConvert.DeserializeObject<ReturnCarMissionResult>(resultt);
                    return CarMissionResult.message;
                }
            }
            else
            {
                return "operation failed";
            }            
        }

        private string CombineJsonStr(string ActionType, string AGVID, string StorageBin)
        {
            int ActionID = 0;
            string json = "X";

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

            if (ActionID != 0 && !string.IsNullOrWhiteSpace(AGVID) && !string.IsNullOrEmpty(AGVID))
            {
                CarMission CarMission = new CarMission()
                {
                    userId = 1,
                    executeAgv = AGVID,
                    orderType = 1,
                    priority = 0,
                    tasks = new Tasks()
                    {
                        seqNum = 1,
                        targetEntity = 5,
                        action = ActionID,
                        //value = 1 之後放貨 要新增抬升高度的參數
                    }
                };
                //轉成JSON格式
                json = JsonConvert.SerializeObject(CarMission);
            }

            //顯示
            //Response.Write(json);

            return json;
        }
    }
}