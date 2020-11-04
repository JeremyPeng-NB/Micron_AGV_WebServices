using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Dapper;
using Micron_AGV_WebServices.Model;

namespace Micron_AGV_WebService.DAL
{
    public class ManagemetFunction
    {
        //開啟資料庫連結
        public Micron_AGV_DB _db = new Micron_AGV_DB();

        //關閉資料庫
        public void Dispose()
        {
            ((IDisposable)_db).Dispose();
        }

        private void RecordErrorLog(DateTime Time, string StorageBin, string Message, string FunctionName)
        {
            var InsertErrorLog = new ErrorLog();
            InsertErrorLog.Time = Time;
            InsertErrorLog.StorageBin = StorageBin;
            InsertErrorLog.Message = Message;
            InsertErrorLog.FunctionName = FunctionName;
            _db.ErrorLogs.Add(InsertErrorLog);
        }

        private void InsertPackageLog(ShelfManagement StorageBinData, string StorageBin, DateTime Time)
        {
            using (SqlConnection ConnStr = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
            {
                string InsertLogStr = "INSERT INTO [PurchaseAndShipmentLog] ([RFID],[Storage],[UpdateTime],[Cargostatus],[AGVID]) VALUES (@RFID,@Storage,@UpdateTime,@Cargostatus,@AGVID)";
                int AffectedRows = ConnStr.Execute(InsertLogStr, new { RFID = StorageBinData.RFID, Storage = StorageBin, UpdateTime = Time, Cargostatus = StorageBinData.Purpose, AGVID = StorageBinData.AGVID });
            }
        }

        private void UpdatePackageErrorStatus(ShelfManagement StorageBinData, string PurchaseStorageBin, DateTime PurchaseTime)
        {
            StorageBinData.Status = "異常";
            StorageBinData.UpDataTime = PurchaseTime;
        }

        private void UpdatePackageStatus(ShelfManagement StorageBinData, DateTime ShipmentTime)
        {
            StorageBinData.Status = "正常";
            StorageBinData.WithPackage = "無";
            StorageBinData.Purpose = string.Empty;
            StorageBinData.UpDataTime = ShipmentTime;
            StorageBinData.AGVID = string.Empty;
        }

        public string Purchase_Complete_HaveRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus)
        {
            //var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault();

            //// 是碼頭儲位
            //if (StorageBinData.Status.Contains("碼頭"))
            //{ 
            //    // 回傳RFID
            //}
            //// 不是碼頭儲位
            //else
            //{
            //    if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID) && !PurchaseStatus.Contains("failure"))
            //    {

            //    }
            //    else
            //    {
            //        UpdateStatusAndLog(StorageBinData, PurchaseStorageBin, PurchaseTime);

            //        if (PurchaseStatus.Contains("failure"))
            //        {

            //            RecordErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常", "Purchase_Complete_HaveRFID");
            //        }
            //        else
            //        {
            //            RecordErrorLog(PurchaseTime, PurchaseStorageBin, "人為偷放", "Purchase_Complete_HaveRFID");
            //        }
            //    }
            //}
            return "Y";
        }

        /// <summary>
        /// 放貨完成 - 無RFID
        /// </summary>
        /// <param name="PurchaseTime"></param>
        /// <param name="PurchaseStorageBin"></param>
        /// <param name="PurchaseStatus"></param>
        /// <returns></returns>
        public string[] Purchase_Complete_NoRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus)
        {
            string[] ResponseStr = { "Y", "正常" };

            try
            {
                // (儲位管理)
                var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault();

                // 取得儲位上的RFID
                if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID))
                {
                    ResponseStr[1] = StorageBinData.AGVID;

                    // 放貨狀態 == 正常
                    if (!PurchaseStatus.Contains("failure"))
                    {
                        // 使用RFID + 儲位修改儲位狀態 & 新增Log & 叫車子離開
                        UpdatePackageStatus(StorageBinData, PurchaseTime);
                        InsertPackageLog(StorageBinData, PurchaseStorageBin, PurchaseTime);
                    }
                    // 放貨狀態 == 異常 
                    else
                    {
                        ResponseStr[0] = "X";
                        UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                        RecordErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常","Purchase_Complete_NoRFID");
                    }                  
                }
                // 沒收到RFID (人偷放的!)
                else
                {
                    ResponseStr[0] = "X";
                    ResponseStr[1] = "異常";
                    UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                    RecordErrorLog(PurchaseTime, PurchaseStorageBin, "人為偷放", "Purchase_Complete_NoRFID");
                }
            }
            catch (Exception ex)
            {
                ResponseStr[0] = "X";
                ResponseStr[1] = ex.ToString();
                RecordErrorLog(PurchaseTime, PurchaseStorageBin, "例外狀況", "Purchase_Complete_NoRFID");
            }

            _db.SaveChanges();
            return ResponseStr;
        }

        /// <summary>
        /// 出貨完成
        /// </summary>
        /// <param name="ShipmentTime"></param>
        /// <param name="ShipmentStorageBin"></param>
        /// <returns></returns>
        public string[] Shipment_Complete(DateTime ShipmentTime, string ShipmentStorageBin)
        {
            string[] ResponseStr = { "Y", "正常" };

            try
            {
                // (儲位管理)
                var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == ShipmentStorageBin).FirstOrDefault();
               
                // RFID + 儲位查詢 (是否有派車)
                var DispatchRecordCount = _db.DispatchRecords.Where(x => x.Storage == ShipmentStorageBin && x.RFID == StorageBinData.RFID).Count();

                // 取得儲位上的RFID
                if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID))
                {
                    // 暫存區
                    if (StorageBinData.Area.Contains("暫存區"))
                    {
                        // 是否有派車
                        if (DispatchRecordCount <= 0)
                        {
                            ResponseStr[0] = "X";
                            ResponseStr[1] = "異常";
                            RecordErrorLog(ShipmentTime, ShipmentStorageBin, "找不到派車資料", "Shipment_Complete");
                        }
                    }
                    // 碼頭
                    else
                    {
                        // 通知電子看板出貨時間
                        if (StorageBinData.Purpose.Contains("出貨準備"))
                        {
                            ResponseStr[1] = ShipmentTime.ToString();
                        }
                    }
                }
                // 修改貨物儲位狀態 + 新增貨物Log
                UpdatePackageStatus(StorageBinData, ShipmentTime);
                InsertPackageLog(StorageBinData, ShipmentStorageBin, ShipmentTime);
            }
            catch (Exception ex)
            {
                ResponseStr[0] = "X";
                ResponseStr[1] = ex.ToString();
                RecordErrorLog(ShipmentTime, ShipmentStorageBin, "例外狀況", "Shipment_Complete");
            }

            _db.SaveChanges();
            return ResponseStr;
        }
    }
}