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

        private void InsertPackageLog(ShelfManagement StorageBinData, string StorageBin, DateTime Time, string IsPepoleFetch)
        {
            using (SqlConnection ConnStr = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
            {
                string InsertLogStr = "INSERT INTO [PurchaseAndShipmentLog] ([RFID],[Storage],[UpdateTime],[CargoStatus],[AGVID]) VALUES (@RFID,@Storage,@UpdateTime,@Cargostatus,@AGVID)";
                int AffectedRows = ConnStr.Execute(InsertLogStr, new { RFID = StorageBinData.RFID, Storage = StorageBin, UpdateTime = Time, CargoStatus = StorageBinData.Purpose + IsPepoleFetch, AGVID = StorageBinData.AGVID });
            }
        }

        private void UpdatePackageErrorStatus(ShelfManagement StorageBinData, string PurchaseStorageBin, DateTime PurchaseTime)
        {
            StorageBinData.Status = "異常";
            StorageBinData.UpDataTime = PurchaseTime;
        }

        private void UpdatePurchasePackageStatus(ShelfManagement StorageBinData, DateTime PurchaseTime)
        {
            StorageBinData.Status = "正常";
            StorageBinData.WithPackage = "有";
            StorageBinData.UpDataTime = PurchaseTime;
            StorageBinData.AGVID = string.Empty;
        }

        private void UpdateShipmentPackageStatus(ShelfManagement StorageBinData, DateTime ShipmentTime)
        {
            StorageBinData.RFID = string.Empty;
            StorageBinData.Status = "正常";
            StorageBinData.WithPackage = "無";
            StorageBinData.Purpose = string.Empty;
            StorageBinData.UpDataTime = ShipmentTime;
            StorageBinData.AGVID = string.Empty;
        }

        public string[] Purchase_Complete_HaveRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus, string RFID)
        {
            string[] ResponseStr = { "Y", "正常" };

            // (儲位管理)
            var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault();

            // (儲位管理) RFID搜尋儲位
            var RFIDIsOrder = _db.ShelfManagements.Where(x => x.RFID == RFID).Any();

            // RFID是否符合規格
            var RFIDIsMatch = true;

            // 是
            if (RFIDIsMatch)
            {
                if (!PurchaseStatus.Contains("failure"))
                {
                    // 正常
                    // 否
                    if (!RFIDIsOrder)
                    {
                        StorageBinData.RFID = RFID;
                        StorageBinData.Purpose = "進貨準備";
                    }
                    InsertPackageLog(StorageBinData, PurchaseStorageBin, PurchaseTime, "");
                    UpdatePurchasePackageStatus(StorageBinData, PurchaseTime);
                    ResponseStr[1] = StorageBinData.AGVID;
                }
                else
                {
                    // 異常
                    // (儲位管理) 修改儲位狀態為異常 + 異常Log
                    UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                    RecordErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常", "Purchase_Complete_HaveRFID");
                    ResponseStr[0] = "X";
                    ResponseStr[0] = "放貨異常";
                }
            }
            // 否
            else
            {
                // (儲位管理) 修改儲位狀態為異常 + 異常Log
                UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                RecordErrorLog(PurchaseTime, PurchaseStorageBin, "RFID不符合規格", "Purchase_Complete_HaveRFID");
                ResponseStr[0] = "X";
                ResponseStr[0] = "RFID不符合規格";
            }

            _db.SaveChanges();
            return ResponseStr;
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
                // 儲位管理
                var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault();

                // 取得儲位上的RFID
                if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID))
                {
                    // 放貨狀態 == 正常
                    if (!PurchaseStatus.Contains("failure"))
                    {
                        // 使用RFID + 儲位修改儲位狀態 & 新增Log 
                        InsertPackageLog(StorageBinData, PurchaseStorageBin, PurchaseTime,"");
                        UpdatePurchasePackageStatus(StorageBinData, PurchaseTime);                       
                    }
                    // 放貨狀態 == 異常 
                    else
                    {
                        // 修改為異常狀態 & ErrorLog
                        ResponseStr[0] = "X";
                        UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                        RecordErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常","Purchase_Complete_NoRFID");
                    }

                    // 叫車子離開
                    ResponseStr[1] = StorageBinData.AGVID;
                }
                // 沒收到RFID (人偷放的!)
                else
                {
                    ResponseStr[0] = "X";
                    ResponseStr[1] = "人為放貨";
                    UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                    RecordErrorLog(PurchaseTime, PurchaseStorageBin, "人為放貨", "Purchase_Complete_NoRFID");
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
        /// 取貨完成
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
                var IsDispatchRecord = _db.DispatchRecords.Where(x => x.Storage == ShipmentStorageBin && x.RFID == StorageBinData.RFID).Any();

                // 是否人為取貨
                var IsPeopleFetch = string.Empty;

                // 取得儲位上的RFID
                if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID))
                {
                    // 暫存區
                    if (StorageBinData.Area.Contains("暫存區"))
                    {
                        // 是否有派車
                        if (!IsDispatchRecord)
                        {
                            ResponseStr[0] = "X";
                            ResponseStr[1] = "取貨異常";
                            RecordErrorLog(ShipmentTime, ShipmentStorageBin, "找不到派車資料(暫存區貨物被人為拿取)", "Shipment_Complete");
                            IsPeopleFetch = "(人為拿取)";
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
                InsertPackageLog(StorageBinData, ShipmentStorageBin, ShipmentTime, IsPeopleFetch);
                UpdateShipmentPackageStatus(StorageBinData, ShipmentTime);
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