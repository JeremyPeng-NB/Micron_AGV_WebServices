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

        private void UpdateStatusAndLog(ShelfManagement StorageBinData, string PurchaseStorageBin, DateTime PurchaseTime)
        {
            StorageBinData.Status = "異常";
            StorageBinData.UpDataTime = PurchaseTime;
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
                    if (StorageBinData.Status != "failure")
                    {
                        // 使用RFID + 儲位修改放貨時間 & 叫車子離開
                        var StorageBinLog = _db.PurchaseAndShipmentLogs.Where(x => x.RFID == StorageBinData.RFID && x.Storage == PurchaseStorageBin).FirstOrDefault();
                        StorageBinLog.UpdateTime = PurchaseTime;
                    }
                    // 放貨狀態 == 異常 
                    else
                    {
                        ResponseStr[0] = "X"; 
                        UpdateStatusAndLog(StorageBinData, PurchaseStorageBin, PurchaseTime);
                        RecordErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常","Purchase_Complete_NoRFID");
                    }                  
                }
                // 沒收到RFID (人偷放的!)
                else
                {
                    ResponseStr[0] = "X";
                    ResponseStr[1] = "異常";
                    UpdateStatusAndLog(StorageBinData, PurchaseStorageBin, PurchaseTime);
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

        private void PackageStatusFunc(ShelfManagement StorageBinData, DateTime ShipmentTime)
        {
            StorageBinData.Status = "正常";
            StorageBinData.WithPackage = "無";
            StorageBinData.Purpose = string.Empty;
            StorageBinData.UpDataTime = ShipmentTime;
            StorageBinData.AGVID = string.Empty;
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
                // RFID + 儲位查詢是否有派車
                var DispatchRecordCount = _db.DispatchRecords.Where(x => x.Storage == ShipmentStorageBin && x.RFID == StorageBinData.RFID).Count();

                // 取得儲位上的RFID
                if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID))
                {
                    // 暫存區
                    if (StorageBinData.Area.Contains("暫存區"))
                    {
                        // 修改貨物儲位狀態
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
                PackageStatusFunc(StorageBinData, ShipmentTime);
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