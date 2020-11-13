using Micron_AGV_WebService.DAL;
using Micron_AGV_WebServices.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;

namespace Micron_AGV_WebServices
{
    /// <summary>
    ///DispatchAndStorage 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class DispatchAndStorage : System.Web.Services.WebService
    {
        ManagemetFunction ManagementFunc = new ManagemetFunction();
        DispatchFunction DispatchFunc = new DispatchFunction();

        [WebMethod]
        public string[] Purchase_Complete_HaveRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus, string RFID)
        {
            return ManagementFunc.Purchase_Complete_HaveRFID(PurchaseTime, PurchaseStorageBin, PurchaseStatus, RFID);
        }

        [WebMethod]
        public string[] Purchase_Complete_NoRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus)
        {
            return ManagementFunc.Purchase_Complete_NoRFID(PurchaseTime, PurchaseStorageBin, PurchaseStatus);
        }

        [WebMethod]
        public string[] Shipment_Complete(DateTime ShipmentTime, string ShipmentStorageBin)
        {
            return ManagementFunc.Shipment_Complete(ShipmentTime, ShipmentStorageBin);
        }

        [WebMethod]
        public void Dispatch_AddTask(string TransferTask)
        {
            DispatchFunc.AddTask(TransferTask);
        }

        [WebMethod]
        public void Dispatch_MissionComplete(string AGVID, string RFID)
        {
            DispatchFunc.MissionComplete(AGVID, RFID);
        }

        [WebMethod]
        public void WinformTest()
        {
            string EXEPath = @"D:\自學\WindowsFormsApplication1\WindowsFormsApplication1\bin\Release";
            Process exe = new Process();
            exe.StartInfo.FileName = EXEPath + @"\WindowsFormsApplication1.exe";
            exe.StartInfo.Arguments = "WaterMan GOOD GOOD";
            exe.Start();
            exe.Close();
        }

        [WebMethod]
        public void APITest()
        {
            HttpWebRequest requestt = WebRequest.Create("http://192.168.12.65:1111/TestAPI") as HttpWebRequest;
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
                    AllVechicleRunningState VechicleRunningState = JsonConvert.DeserializeObject<AllVechicleRunningState>(resultt);
                }

                //using (WebResponse response = requestt.GetResponse())
                //{
                //string resultt = "{'agvName':'127.0.0.6','entityId':1,'speed':30,'x':10,'y':20,'battery':90,'agvState':0,'enabled':1}";
                //AllVechicleRunningState VechicleRunningState = JsonConvert.DeserializeObject<AllVechicleRunningState>(resultt);
                //}
            }
        }
    }
}
