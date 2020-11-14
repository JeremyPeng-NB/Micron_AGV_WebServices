using Micron_AGV_WebServices.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Micron_AGV_WebServices.DAL
{
    public class ConnectAPI
    {
        public AllVechicleRunningState APITest()
        {
            HttpWebRequest requestt = WebRequest.Create("http://192.168.12.65:1111/TestAPI") as HttpWebRequest;
            AllVechicleRunningState VechicleRunningState;
            //string mapId = "1";
            //string sectionId = "1";

            if (requestt != null)
            {
                /*傳遞訊息*/
                //StringBuilder sb = new StringBuilder();
                //sb.AppendLine("{");
                //sb.AppendLine("\"mapId\":" + mapId + ",");
                //sb.AppendLine("\"sectionId\":" + sectionId);
                //sb.AppendLine("}");

                /*傳遞方式 + 格式*/
                requestt.Method = "GET";
                requestt.KeepAlive = true;
                requestt.ContentType = "application/json";

                /*轉byte*/
                //byte[] bs = Encoding.UTF8.GetBytes(sb.ToString());

                /*吐訊息過去*/
                //using (Stream reqStream = requestt.GetRequestStream())
                //{
                //    reqStream.Write(bs, 0, bs.Length);
                //}

                /*接訊息回來*/
                using (WebResponse response = requestt.GetResponse())
                {
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string resultt = sr.ReadToEnd();
                    VechicleRunningState = JsonConvert.DeserializeObject<AllVechicleRunningState>(resultt);
                }

                //using (WebResponse response = requestt.GetResponse())
                //{
                //string resultt = "{'agvName':'127.0.0.6','entityId':1,'speed':30,'x':10,'y':20,'battery':90,'agvState':0,'enabled':1}";
                //AllVechicleRunningState VechicleRunningState = JsonConvert.DeserializeObject<AllVechicleRunningState>(resultt);
                //}

                return VechicleRunningState;
            }
            else
            {
                return null;
            }
        }
    }
}