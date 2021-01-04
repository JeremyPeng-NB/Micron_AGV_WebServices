using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Micron_AGV_WebServices.Model;
using System.Threading;

namespace Micron_AGV_WebServices.DAL
{
    public class EquipmentFunction
    {
        //開啟資料庫連結
        public Micron_AGV_DB _db = new Micron_AGV_DB();

        //關閉資料庫連結
        public void Dispose()
        {
            ((IDisposable)_db).Dispose();
        }

        public string StatusUpdate(string EquipmentPlace, bool EquipmentStatus)
        {
            if ((EquipmentPlace == "TowerStocker" || EquipmentPlace == "APK" || EquipmentPlace == "UPK" || EquipmentPlace == "CPUOUT" || EquipmentPlace == "CPUIN"))
            {
                var UpdatePlace = _db.EquipmentStatuss.Where(x => x.Place == EquipmentPlace).FirstOrDefault();

                UpdatePlace.Status = EquipmentStatus;

                _db.SaveChanges();

                return "Y";
            }
            else
            {
                return "X";
            }
        }

        public void StatusCheck(string DispatchRecordStorage)
        {
            switch (DispatchRecordStorage)
            {
                case "CPU裝箱口":
                    DispatchRecordStorage = "CPUOUT";
                    break;
                case "CPU拆箱口":
                    DispatchRecordStorage = "CPUIN";
                    break;
                default:
                    break;
            }

            bool EquiStatusDet = true;

            while (EquiStatusDet)
            {
                var IsCanPurchase = _db.EquipmentStatuss.Where(x => x.Place == DispatchRecordStorage).Select(x => x.Status).FirstOrDefault();

                if (IsCanPurchase)         //設備可以放貨 
                {
                    EquiStatusDet = false; //修改Status
                }
                else                       //設備不可以放貨
                {
                    //寫Log? + 睡覺
                    //using (SqlConnection ConnStr = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
                    //{
                    //    string InsertLogStr = "INSERT INTO [DispatchErrorLog] ([Time],[Message],[FunctionName]) VALUES (@Time,@Message,@FunctionName)";
                    //    int AffectedRows = ConnStr.Execute(InsertLogStr, new { Time = DateTime.Now, Message = "設備無法收貨!", FunctionName = "KMR_Purchase_Test" });
                    //}
                    Thread.Sleep(5000);
                }

                continue;
            }
        }
    }
}