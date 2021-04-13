using Micron_AGV_WebServices.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Threading.Tasks;

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
        EquipmentFunction EquipmentFunc = new EquipmentFunction();

        //[WebMethod]
        //public string[] Shelf_Purchase_Complete_HaveRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus, string RFID)
        //{
        //    return ManagementFunc.Purchase_Complete_HaveRFID(PurchaseTime, PurchaseStorageBin, PurchaseStatus, RFID);
        //}

        //[WebMethod]
        //public string[] Shelf_Purchase_Complete_NoRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus)
        //{
        //    return ManagementFunc.Purchase_Complete_NoRFID(PurchaseTime, PurchaseStorageBin, PurchaseStatus);
        //}

        //[WebMethod]
        //public string[] Shelf_Shipment_Complete(DateTime ShipmentTime, string ShipmentStorageBin)
        //{
        //    return ManagementFunc.Shipment_Complete(ShipmentTime, ShipmentStorageBin);
        //}

        [WebMethod]
        public string Equipment_Purchase_Status(string EquipmentPlace, bool EquipmentStatus)
        {
            return EquipmentFunc.StatusUpdate(EquipmentPlace, EquipmentStatus);
        }

        [WebMethod]
        public string DispatchAddTask(string TransferTask)
        {
            return DispatchFunc.AddTask(TransferTask);
        }

        [WebMethod]
        public string MissionComplete(string AGVID)
        {
            return DispatchFunc.MissionComplete(AGVID);
        }
    }
}
