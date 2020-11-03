using Micron_AGV_WebService.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Dispatch_MissionComplete(string AGVID, string RFID)
        {
            return DispatchFunc.MissionComplete(AGVID, RFID);
        }
    }
}
