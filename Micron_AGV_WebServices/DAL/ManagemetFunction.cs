using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Dapper;
using Micron_AGV_WebServices.Model;

namespace Micron_AGV_WebServices.DAL
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

        private void RecordShelfErrorLog(DateTime Time, string StorageBin, string Message, string FunctionName)
        {
            var InsertErrorLog = new ShelfErrorLog();
            InsertErrorLog.Time = Time;
            InsertErrorLog.StorageBin = StorageBin;
            InsertErrorLog.Message = Message;
            InsertErrorLog.FunctionName = FunctionName;
            _db.ShelfErrorLogs.Add(InsertErrorLog);
        }

        private void InsertPackageLog(ShelfManagementTEST StorageBinData, string StorageBin, DateTime Time, string IsPepoleFetch)
        {
            using (SqlConnection ConnStr = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
            {
                string InsertLogStr = "INSERT INTO [PurchaseAndShipmentLog] ([RFID],[Storage],[UpdateTime],[CargoStatus],[AGVID]) VALUES (@RFID,@Storage,@UpdateTime,@Cargostatus,@AGVID)";
                int AffectedRows = ConnStr.Execute(InsertLogStr, new { RFID = StorageBinData.RFID, Storage = StorageBin, UpdateTime = Time, CargoStatus = StorageBinData.Purpose + IsPepoleFetch, AGVID = StorageBinData.AGVID });
            }
        }

        private void UpdatePackageErrorStatus(ShelfManagementTEST StorageBinData, string PurchaseStorageBin, DateTime PurchaseTime)
        {
            StorageBinData.Status = "異常";
            StorageBinData.UpDataTime = PurchaseTime;
        }

        private void UpdatePurchasePackageStatus(ShelfManagementTEST StorageBinData, DateTime PurchaseTime)
        {
            StorageBinData.Status = "正常";
            StorageBinData.WithPackage = "有";
            StorageBinData.UpDataTime = PurchaseTime;
            StorageBinData.AGVID = string.Empty;
        }

        private void UpdateShipmentPackageStatus(ShelfManagementTEST StorageBinData, DateTime ShipmentTime)
        {
            StorageBinData.RFID = null;
            StorageBinData.Status = "正常";
            StorageBinData.WithPackage = "無";
            StorageBinData.Purpose = null;
            StorageBinData.UpDataTime = ShipmentTime;
            StorageBinData.AGVID = string.Empty;
        }

        /// <summary>
        /// 找尋新儲位
        /// </summary>
        /// <param name="RFID"></param>
        /// <returns></returns>
        public string[] NoticeNewStorageBin(string RFID)
        {
            string[] ResponseStr = { "Y", "正常" };
            try
            {
                // 確認收到RFID
                if (!string.IsNullOrEmpty(RFID) && !string.IsNullOrWhiteSpace(RFID))
                {
                    // 找空儲位 
                    var IsWithSpace = _db.ShelfManagementTESTs.Where(x => x.Status == "正常");

                    // 碼頭有位置
                    if (IsWithSpace.Where(x => x.Area == "碼頭").Any())
                    {
                        // 排序之後丟第一筆資料出來
                        var PierData = IsWithSpace.OrderBy(x => x.Storage).FirstOrDefault();

                        // 修改儲位狀態 + RFID
                        PierData.Purpose = "出貨";
                        PierData.RFID = RFID;

                        // 把儲位丟出去
                        ResponseStr[1] = PierData.Storage;
                    }
                    // 碼頭沒位置
                    else
                    {
                        // 排序之後丟第一筆資料出來
                        var TempAreaData = IsWithSpace.OrderBy(x => x.Storage).FirstOrDefault();

                        // 如果有資料
                        if (TempAreaData != null)
                        {
                            // 修改儲位狀態 + RFID
                            TempAreaData.Status = "出貨";
                            TempAreaData.RFID = RFID;

                            // 把儲位丟出去
                            ResponseStr[1] = TempAreaData.Storage;
                        }
                        // 沒有資料
                        else
                        {
                            //代表沒有儲位
                            ResponseStr[0] = "X";
                            ResponseStr[1] = "沒有儲位";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ResponseStr[0] = "X"; 
                ResponseStr[1] = ex.ToString();
                RecordShelfErrorLog(DateTime.Now, string.Empty, "例外狀況:" + ex.ToString(), "NoticeNewStorageBin");
            }
            _db.SaveChanges();
            return ResponseStr;
        }

        /// <summary>
        /// 放貨完成 - 有RFID
        /// </summary>
        /// <param name="PurchaseTime"></param>
        /// <param name="PurchaseStorageBin"></param>
        /// <param name="PurchaseStatus"></param>
        /// <param name="RFID"></param>
        /// <returns></returns>
        public string[] Purchase_Complete_HaveRFID(DateTime PurchaseTime, string PurchaseStorageBin, string PurchaseStatus, string RFID)
        {
            string[] ResponseStr = { "Y", "正常" };
            try
            {
                // (儲位管理) 儲位資料
                // var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault(); 
                var StorageBinData = _db.ShelfManagementTESTs.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault();

                // (儲位管理) RFID搜尋儲位
                // var RFIDIsOrder = _db.ShelfManagements.Where(x => x.RFID == RFID).Any();
                var RFIDIsOrder = _db.ShelfManagementTESTs.Where(x => x.RFID == RFID).Any();

                // (儲位管理) 判斷是否重複放貨
                // var DuplicatePackage = _db.ShelfManagements.Where(x => x.RFID == RFID);
                var IsDuplicatePackage = _db.ShelfManagementTESTs.Where(x => x.Previous_RFID == RFID);

                // RFID是否符合規格
                var RFIDIsMatch = true;

                // RFID符合規格
                if (RFIDIsMatch)
                {
                    // 放貨正常
                    if (!PurchaseStatus.Contains("failure"))
                    {
                        if (!RFIDIsOrder)
                        {
                            #region 判斷重複放貨
                            if (IsDuplicatePackage.Any())
                            {
                                var DuplicatePackage = IsDuplicatePackage.FirstOrDefault();
                                StorageBinData.RFID = DuplicatePackage.Previous_RFID;
                                StorageBinData.Purpose = DuplicatePackage.Previous_Purpose;
                                DuplicatePackage.Previous_RFID = null;
                                DuplicatePackage.Previous_Purpose = null;
                                UpdateShipmentPackageStatus(DuplicatePackage, PurchaseTime);
                            }
                            else
                            {
                                StorageBinData.RFID = RFID;
                                StorageBinData.Purpose = "進貨";
                            }
                            #endregion
                        }
                        ResponseStr[1] = StorageBinData.AGVID;
                        UpdatePurchasePackageStatus(StorageBinData, PurchaseTime);
                        InsertPackageLog(StorageBinData, PurchaseStorageBin, PurchaseTime, string.Empty); 
                    }
                    // 放貨異常
                    else
                    {
                        // (儲位管理) 修改儲位狀態為異常 + 異常Log
                        ResponseStr[0] = "X";
                        ResponseStr[1] = "放貨異常";
                        UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                        RecordShelfErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常", "Purchase_Complete_HaveRFID");
                    }
                }
                // RFID不符合規格
                else
                {
                    // (儲位管理) 修改儲位狀態為異常 + 異常Log
                    ResponseStr[0] = "X";
                    ResponseStr[1] = "RFID不符合規格";
                    UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                    RecordShelfErrorLog(PurchaseTime, PurchaseStorageBin, "RFID不符合規格", "Purchase_Complete_HaveRFID");
                }
            }
            catch (Exception ex)
            {
                ResponseStr[0] = "X";
                ResponseStr[1] = ex.ToString();
                RecordShelfErrorLog(PurchaseTime, PurchaseStorageBin, "例外狀況:" + ex.ToString(), "Purchase_Complete_HaveRFID");
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
                // var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault();
                   var StorageBinData = _db.ShelfManagementTESTs.Where(x => x.Storage == PurchaseStorageBin).FirstOrDefault(); 

                // 取得儲位上的RFID
                if (!string.IsNullOrEmpty(StorageBinData.RFID) && !string.IsNullOrWhiteSpace(StorageBinData.RFID))
                {
                    // 放貨狀態 == 正常
                    if (!PurchaseStatus.Contains("failure"))
                    {
                        // 使用RFID + 儲位修改儲位狀態 & 新增Log 
                        UpdatePurchasePackageStatus(StorageBinData, PurchaseTime);
                        InsertPackageLog(StorageBinData, PurchaseStorageBin, PurchaseTime, string.Empty);               
                    }
                    // 放貨狀態 == 異常 
                    else
                    {
                        // 修改為異常狀態 & ErrorLog
                        ResponseStr[0] = "X";
                        UpdatePackageErrorStatus(StorageBinData, PurchaseStorageBin, PurchaseTime);
                        RecordShelfErrorLog(PurchaseTime, PurchaseStorageBin, "放貨異常","Purchase_Complete_NoRFID");
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
                    RecordShelfErrorLog(PurchaseTime, PurchaseStorageBin, "人為放貨", "Purchase_Complete_NoRFID");
                }
            }
            catch (Exception ex)
            {
                ResponseStr[0] = "X";
                ResponseStr[1] = ex.ToString();
                RecordShelfErrorLog(PurchaseTime, PurchaseStorageBin, "例外狀況:" + ex.ToString(), "Purchase_Complete_NoRFID");
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
                // var StorageBinData = _db.ShelfManagements.Where(x => x.Storage == ShipmentStorageBin).FirstOrDefault();
                   var StorageBinData = _db.ShelfManagementTESTs.Where(x => x.Storage == ShipmentStorageBin).FirstOrDefault();  

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
                            ResponseStr[1] = "人為取貨";
                            IsPeopleFetch = "人為取貨";
                            RecordShelfErrorLog(ShipmentTime, ShipmentStorageBin, "找不到派車資料(暫存區貨物被人為拿取)", "Shipment_Complete");
                        }
                    }
                    // 碼頭
                    else
                    {
                        // 通知電子看板出貨時間
                        if (StorageBinData.Purpose.Contains("出貨"))
                        {
                            ResponseStr[1] = ShipmentTime.ToString();
                        }
                    }
                }
                // 修改貨物儲位狀態 + 新增貨物Log
                UpdateShipmentPackageStatus(StorageBinData, ShipmentTime);
                InsertPackageLog(StorageBinData, ShipmentStorageBin, ShipmentTime, IsPeopleFetch);
            }
            catch (Exception ex)
            {
                ResponseStr[0] = "X";
                ResponseStr[1] = ex.ToString();
                RecordShelfErrorLog(ShipmentTime, ShipmentStorageBin, "例外狀況:" + ex.ToString(), "Shipment_Complete");
            }
            _db.SaveChanges();
            return ResponseStr;
        }
    }
}