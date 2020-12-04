﻿using Dapper;
using Micron_AGV_WebServices.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace Micron_AGV_WebServices.DAL
{
    public class DispatchFunction
    {
        //開啟資料庫連結
        public Micron_AGV_DB _db = new Micron_AGV_DB();

        //連結AGV Server的WebAPI Function
        ConnectAPI ConnAPI = new ConnectAPI();

        //New For找尋新儲位
        ManagemetFunction ManagementFunc = new ManagemetFunction();

        //KMR Buffeer
        ArrayList buffer = null;

        //關閉資料庫
        public void Dispose()
        {
            ((IDisposable)_db).Dispose();
        }

        /// <summary>
        /// 新增任務
        /// </summary>
        /// <param name="TaskID"></param>
        /// <returns></returns>
        public string AddTask(string TransferTask)
        {
            //回應字串
            string responseStr = "任務新增成功!";
            try
            {
                //新增任務
                TaskType Task = _db.TaskTypes.Where(x => x.TransferTask == TransferTask).FirstOrDefault();

                //設定GUID
                var NewTaskListID = Guid.NewGuid();

                //設定任務清單的TaskID & TaskListID
                TaskList AddTask = new TaskList();
                AddTask.TaskListID = NewTaskListID;
                AddTask.TaskID = Task.TaskID;

                //設定派車時間
                AddTask.AcceptanceTime = DateTime.Now;

                //查詢是否有待機中車輛
                CarStatus Car = _db.CarStatuss.Where(x => x.Status == "待機中" & x.CarType == Task.Car).OrderBy(x => x.Power).FirstOrDefault();

                //如果沒車
                if (Car == null)
                {
                    //任務狀態為等待中
                    AddTask.TaskAcceptance = "等待中";
                }
                //如果有車
                else
                {
                    //任務狀態為執行中
                    AddTask.TaskAcceptance = "執行中";

                    //任務開始時間
                    AddTask.StartTime = DateTime.Now;

                    //修改車輛狀態 + 執行任務ID + 更新時間
                    Car.Status = "任務中";
                    Car.TaskListID = NewTaskListID;
                    Car.UpdataTime = DateTime.Now;

                    //任務放進派車資料(任務種類/派車任務ID/車輛資訊)
                    AddDispatch(Task.TaskID.ToString(), NewTaskListID, Car);
                }

                _db.TaskLists.Add(AddTask);
                _db.SaveChanges();

                //查詢是否派車
                var WithCarDispatch = _db.View_DispatchRecords.Where(x => x.EndTime == null && x.TaskStatus == "執行中" && x.TaskListID == NewTaskListID && x.ActionID == 1).FirstOrDefault();

                //如果有派車資料
                if (WithCarDispatch != null)
                {
                    if (WithCarDispatch.AGVID.Contains("AGV"))
                    {
                        //組json字串 + ConnAPI(AGV Server)派車
                        var jsonStr = CombineJsonStr(WithCarDispatch.ActionType, WithCarDispatch.AGVID);
                        if (!jsonStr.Contains("X"))
                        {
                            var resultStr = ConnAPI.AddCarMission(jsonStr);
                            responseStr += "派車狀態:" + resultStr;
                            if (resultStr != "success")
                            {
                                RecordDispatchErrorLog(DateTime.Now, responseStr, "Add_Task");
                            }
                        }
                        else
                        {
                            responseStr += "派車狀態:傳輸字串格式錯誤!";
                            RecordDispatchErrorLog(DateTime.Now, responseStr, "Add_Task");
                        }
                    }
                    else
                    {
                        //Call_KMR();
                    }
                }
            }
            catch (Exception ex)
            {
                responseStr = "任務新增失敗!" + ex.ToString();
                RecordDispatchErrorLog(DateTime.Now, responseStr, "Add_Task");
            }
            _db.SaveChanges();
            return responseStr;
        }

        /// <summary>
        /// 任務放進派車資料
        /// </summary>
        /// <param name="TaskID"></param>
        /// <param name="NewTaskListID"></param>
        /// <param name="car"></param>
        private void AddDispatch(string TaskID, Guid NewTaskListID, CarStatus Car)
        {
            int Task = int.Parse(TaskID);

            //查詢任務流程
            IQueryable<View_TaskType> TaskProcess = _db.View_TaskTypes.Where(x => x.TaskID == Task);

            foreach (var item in TaskProcess)
            {
                string Location = item.Destination;

                DispatchRecord Dispatch = new DispatchRecord();

                if (item.ActionID == 1)
                {
                    Dispatch.TaskStatus = "執行中";
                    Dispatch.StartTime = DateTime.Now;
                }
                else
                {
                    Dispatch.TaskStatus = "等待中";
                }

                if (!string.IsNullOrWhiteSpace(item.Destination))
                {
                    Location = StorageCheck(item.TaskID, item.ActionID, item.Destination, Car.AGVID, item.Purpose);
                }

                Dispatch.AGVID = Car.AGVID;

                Dispatch.TaskID = Task;

                Dispatch.ActionID = item.ActionID;

                Dispatch.TaskListID = NewTaskListID;

                Dispatch.Storage = Location;

                _db.DispatchRecords.Add(Dispatch);
            }

            _db.SaveChanges();
        }

        /// <summary>
        /// 儲位檢查&指定儲位
        /// </summary>
        /// <param name="TaskID"></param>
        /// <param name="ActionID"></param>
        /// <param name="Destination"></param>
        /// <returns></returns>
        private string StorageCheck(int TaskID, int ActionID, string Destination, string AGVID, string Purpose)
        {
            string Location;
            string status = "預定";
            switch (TaskID)
            {
                //CPU取實箱_碼頭放貨
                case 3:
                    if (ActionID == 3)
                    {
                        Location = _db.ShelfManagements.Where(x => x.Area == "碼頭" & x.Status == "正常" & x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
                        if (Location == null)
                        {
                            Location = _db.ShelfManagements.Where(x => x.Area == "暫存區" & x.Status == "正常" & x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
                        }
                    }
                    else
                    {
                        Location = Destination;
                    }
                    break;
                //放空箱_空箱暫存區取空箱
                case 4:
                    if (ActionID == 1)
                    {
                        Location = _db.ShelfManagements.Where(x => x.Area == "空箱暫存" & x.Status == "正常" & x.WithPackage == "有").Select(x => x.Storage).FirstOrDefault();
                    }
                    else
                    {
                        Location = Destination;
                    }
                    break;
                //取空箱_空箱暫存區放空箱
                case 5:
                    if (ActionID == 3)
                    {
                        Location = _db.ShelfManagements.Where(x => x.Area == "空箱暫存" & x.Status == "正常" & x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
                    }
                    else
                    {
                        Location = Destination;
                    }
                    break;
                //暫存區取貨(出貨)
                case 6:
                    switch (ActionID)
                    { //暫存區取貨(出貨)_暫存區取貨
                        case 1:
                            Location = _db.ShelfManagements.Where(x => x.Area == "暫存區" & x.Status == "正常" & x.WithPackage == "有" & x.Purpose == "出貨").Select(x => x.Storage).FirstOrDefault();
                            break;
                        //暫存區取貨(出貨)_碼頭放貨
                        case 3:
                            Location = _db.ShelfManagements.Where(x => x.Area == "碼頭" & x.Status == "正常" & x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
                            break;
                        default:
                            Location = Destination;
                            break;
                    }
                    break;
                //碼頭取貨(拆箱)_前往碼頭取貨
                case 7:
                    if (ActionID == 1)
                    {
                        Location = _db.ShelfManagements.Where(x => x.Area == "碼頭" & x.Status == "正常" & x.WithPackage == "有" & x.Purpose == "進貨").Select(x => x.Storage).FirstOrDefault();
                    }
                    else
                    {
                        Location = Destination;
                    }
                    break;
                //暫存區取貨(拆箱)_前往暫存區取貨
                case 8:
                    if (ActionID == 1)
                    {
                        Location = _db.ShelfManagements.Where(x => x.Area == "暫存區" & x.Status == "正常" & x.WithPackage == "有" & x.Purpose == "進貨").Select(x => x.Storage).FirstOrDefault();
                    }
                    else
                    {
                        Location = Destination;
                    }
                    break;
                //碼頭取貨(進貨)_暫存區放貨
                case 11:
                    switch (ActionID)
                    {  //碼頭取貨(進貨)_碼頭取貨
                        case 1:
                            Location = _db.ShelfManagements.Where(x => x.Area == "碼頭" & x.Status == "正常" & x.WithPackage == "有" & x.Purpose == "進貨").Select(x => x.Storage).FirstOrDefault();
                            break;
                        //碼頭取貨(進貨)_暫存區放貨
                        case 3:
                            Location = _db.ShelfManagements.Where(x => x.Area == "暫存區" & x.Status == "正常" & x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
                            break;
                        default:
                            Location = Destination;
                            break;
                    }
                    break;
                default:
                    Location = Destination;
                    break;
            }
            StorageEdit(Location, "", Purpose, "", status, AGVID);
            return Location;
        }

        /// <summary>
        /// 修改儲位狀態
        /// </summary>
        /// <param name="Storage"></param>
        /// <param name="RFID"></param>
        /// <param name="Purpose"></param>
        /// <param name="WithPackage"></param>
        private void StorageEdit(string Storage, string RFID, string Purpose, string WithPackage, string Status, string AGVID)
        {
            var StorageEdit = _db.ShelfManagements.Where(x => x.Storage == Storage).FirstOrDefault();

            if (StorageEdit != null)
            {
                if (!string.IsNullOrWhiteSpace(Status))
                {

                    StorageEdit.Status = Status;
                }

                if (!string.IsNullOrWhiteSpace(AGVID))
                {

                    StorageEdit.AGVID = AGVID;
                }

                if (!string.IsNullOrWhiteSpace(RFID))
                {

                    StorageEdit.RFID = RFID;
                }

                if (!string.IsNullOrWhiteSpace(Purpose))
                {

                    StorageEdit.Purpose = Purpose;
                }

                if (!string.IsNullOrWhiteSpace(WithPackage))
                {

                    StorageEdit.WithPackage = WithPackage;
                }

                StorageEdit.UpDataTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 任務完成車傳資訊
        /// </summary>
        /// <param name="AGVID"></param>
        /// <param name="RFID"></param>
        public string MissionComplete(string AGVID, string RFID)
        {
            //查詢此車的狀態
            CarStatus Car = _db.CarStatuss.Where(x => x.AGVID == AGVID).FirstOrDefault();

            //回應字串
            string responseStr = "下一個任務:";

            //查詢派車資料中此車目前所執行任務列表
            var TaskTotal = _db.View_DispatchRecords.Where(x => x.TaskListID == Car.TaskListID);

            //查詢車輛ID執行中的任務
            var DispatchRecord = TaskTotal.Where(x => x.TaskStatus == "執行中").FirstOrDefault();

            if (DispatchRecord != null)
            {
                //修改當前任務完成時間&任務狀態
                DispatchRecord.EndTime = DateTime.Now;
                DispatchRecord.TaskStatus = "完成";

                if (!string.IsNullOrWhiteSpace(RFID) && DispatchRecord.ActionType == "取貨")
                {
                    DispatchRecord.RFID = RFID;
                    InsertPackageLog(RFID, "", AGVID, "取貨成功");
                }
                if (!string.IsNullOrWhiteSpace(RFID) && !string.IsNullOrWhiteSpace(DispatchRecord.Storage))
                {
                    StorageEdit(DispatchRecord.Storage, RFID, "", "", "", Car.AGVID);
                    InsertPackageLog(RFID, RFID, AGVID, "抵達" + DispatchRecord.Storage);
                }

                //如果不是最後一個任務
                if (DispatchRecord.ActionID != TaskTotal.Count())
                {
                    //查詢下一任務
                    var NextAction = TaskTotal.Where(x => x.ActionID == DispatchRecord.ActionID + 1).FirstOrDefault();

                    //修改開始時間&狀態
                    NextAction.StartTime = DateTime.Now;
                    NextAction.TaskStatus = "執行中";

                    //AGV
                    if (Car.CarType == "AGV")
                    {
                        //放貨前檢查
                        if (NextAction.ActionType == "放貨" && !(_db.ShelfManagements.Where(x => x.RFID == RFID && x.Storage == DispatchRecord.Storage && x.Status == "預定" && x.AGVID == DispatchRecord.AGVID).Any()))
                        {
                            //再找新儲位
                            var NewStorageBin = ManagementFunc.NoticeNewStorageBin(NextAction.RFID);
                            DispatchRecord.TaskStatus = "執行中";
                            DispatchRecord.EndTime = null;
                            NextAction.TaskStatus = "等待中";
                            NextAction.StartTime = null;

                            //叫車去下一個位置(待續)
                        }
                        else
                        {
                            //組json字串 + ConnAPI(AGV Server)派車
                            var jsonStr = CombineJsonStr(NextAction.ActionType, NextAction.AGVID);
                            if (!jsonStr.Contains("X"))
                            {
                                var resultStr = ConnAPI.AddCarMission(jsonStr);
                                responseStr += NextAction.Action + "\n派車狀態:" + resultStr;
                                if (resultStr != "success")
                                {
                                    RecordDispatchErrorLog(DateTime.Now, responseStr, "MissionComplete");
                                }
                            }
                            else
                            {
                                responseStr += NextAction.Action + "\n派車狀態:傳輸字串格式錯誤!";
                                RecordDispatchErrorLog(DateTime.Now, responseStr, "MissionComplete");
                            }
                        }
                    }
                    //KMR
                    else
                    {
                        if (NextAction.ActionType == "放貨")
                        {
                            bool Status = true;
                            while (Status)
                            {
                                //詢問設備是否可以放貨 Y／Ｎ
                                var IsCanPurchase = "Y";

                                if (IsCanPurchase == "Y") //設備可以放貨 
                                {
                                    Status = false;       //修改Status
                                }
                                else                      //設備不可以放貨
                                {
                                    //寫Log + 睡覺
                                    using (SqlConnection ConnStr = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
                                    {
                                        string InsertLogStr = "INSERT INTO [DispatchErrorLog] ([Time],[Message],[FunctionName]) VALUES (@Time,@Message,@FunctionName)";
                                        int AffectedRows = ConnStr.Execute(InsertLogStr, new { Time = DateTime.Now, Message = "設備無法收貨!" + Name, FunctionName = "KMR_Purchase_Test" });
                                    }
                                    Thread.Sleep(5000);
                                }
                                continue;
                            }
                        }
                       
                        //放貨or取貨-都要CALL
                        //Call_KMR();
                        return "這個還沒有做好拉!先不要玩拉!";
                    }
                }
                //最後一個動作
                else
                {
                    TaskList TaskCompleted = _db.TaskLists.Where(x => x.TaskListID == Car.TaskListID).FirstOrDefault();

                    TaskCompleted.TaskAcceptance = "任務完成";
                    TaskCompleted.EndTime = DateTime.Now;

                    responseStr = "任務完成!";

                    _db.SaveChanges();

                    var TaskListID = Car.TaskListID;

                    Car.Status = "待機中";
                    Car.TaskListID = null;

                    // Stored Procedure
                    // TaskList 資料移到 TaskHistory 
                    // DispatchRecord 資料移到 DispatchHistory 
                    using (SqlConnection Conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
                    {
                        // 準備參數
                        var parameters = new DynamicParameters();
                        parameters.Add("@TasklistID", TaskListID, DbType.Guid, ParameterDirection.Input);
                        Conn.Execute("SP_DispatchHistory", parameters, commandType: CommandType.StoredProcedure);
                        Conn.Execute("SP_TaskHistory", parameters, commandType: CommandType.StoredProcedure);
                    }

                    var WaitingTask = _db.View_TaskLists.Where(x => x.Car == Car.CarType && x.TaskAcceptance == "等待中").OrderBy(x => new { x.Priority, x.AcceptanceTime }).FirstOrDefault();

                    //有待處理任務的時候....
                    if (WaitingTask != null)
                    {
                        //新增任務
                        AddDispatch(WaitingTask.TaskID.ToString(), WaitingTask.TaskListID, Car);

                        TaskList NewTask = _db.TaskLists.Where(x => x.TaskListID == WaitingTask.TaskListID).FirstOrDefault();

                        NewTask.TaskAcceptance = "執行中";

                        Car.Status = "任務中";
                        Car.TaskListID = WaitingTask.TaskListID;

                    }
                    else
                    {
                        //0.主動執行任務
                        //1.查詢碼頭有沒有空位&暫存區有東西要出貨
                        //2.當碼頭有進貨&CPU拆箱有空&派車資料沒有前往CPU拆箱口不然就送暫存區
                        //3.當暫存區有進貨&CPU拆箱有空&派車資料沒有前往CPU拆箱口
                        ActiveTask();
                    }
                }
            }

            _db.SaveChanges();
            return responseStr;
        }

        /// <summary>
        /// 當任務完成，又沒有新指派任務的時候
        /// </summary>
        /// <returns></returns>
        private void ActiveTask()
        {
            //派車資料
            IQueryable<DispatchRecord> DispatchRecord = _db.DispatchRecords;
            //碼頭狀態
            IQueryable<ShelfManagement> Cargoterminal = _db.ShelfManagements.Where(x => x.Status == "正常" & x.Area == "碼頭");
            //暫存區狀態
            IQueryable<ShelfManagement> StorageCache = _db.ShelfManagements.Where(x => x.Status == "正常" & x.Area == "暫存區").OrderBy(x => x.UpDataTime);

            //1.查詢碼頭有沒有空位&暫存區有東西要出貨

            //1.1 查詢碼頭有沒有空位
            string TerminalStatus = Cargoterminal.Where(x => x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
            //1.2 暫存區有東西要出貨
            string StagingStatus = StorageCache.Where(x => x.WithPackage == "有" & x.Purpose == "出貨").OrderBy(x => x.UpDataTime).Select(x => x.Storage).FirstOrDefault();

            if (TerminalStatus != null && StagingStatus != null)
            {
                AddTask("Storage_1");
            }
            else
            {
                //2.當碼頭有進貨&CPU拆箱有空&派車資料沒有前往CPU拆箱口不然就送暫存區
                TerminalStatus = null;
                StagingStatus = null;

                //2.1 碼頭有進貨的貨物
                TerminalStatus = Cargoterminal.Where(x => x.WithPackage == "有" & x.Purpose == "進貨").Select(x => x.Storage).FirstOrDefault();

                if (TerminalStatus != null)
                {
                    //2.2 沒有車輛前往CPU裝箱口的任務
                    IQueryable<DispatchRecord> DispatchStatus = DispatchRecord.Where(x => x.Storage == "CPU裝箱口");

                    //2.3 CPU拆箱口有沒有空(這段要等)

                    //如果沒派車去拆箱口and拆箱口有空
                    if (!DispatchStatus.Any() && 1 == 1)
                    {
                        //送貨去CPU
                        AddTask("Terminal_1");
                    }
                    else
                    {
                        //2.4 尋找暫存區儲位放貨
                        StagingStatus = StorageCache.Where(x => x.WithPackage == "無").Select(x => x.Storage).FirstOrDefault();
                        if (StagingStatus != null)
                        {
                            AddTask("Terminal_2");
                        }
                        else
                        {
                            Storage_2(StorageCache, DispatchRecord);
                        }
                    }
                }
                else
                {
                    //3.當暫存區有進貨&CPU拆箱有空&派車資料沒有前往CPU拆箱口
                    Storage_2(StorageCache, DispatchRecord);
                }
            }
        }

        /// <summary>
        /// 暫存區取貨(拆箱)
        /// </summary>
        /// <param name="StorageCache"></param>
        /// <param name="DispatchRecord"></param>
        private void Storage_2(IQueryable<ShelfManagement> StorageCache, IQueryable<DispatchRecord> DispatchRecord)
        {
            string StagingStatus = StorageCache.Where(x => x.WithPackage == "有" & x.Purpose == "進貨").OrderBy(x => x.UpDataTime).Select(x => x.Storage).FirstOrDefault();

            if (StagingStatus != null)
            {
                //3.2 沒有車輛前往CPU裝箱口的任務
                IQueryable<DispatchRecord> DispatchStatus = DispatchRecord.Where(x => x.Storage == "CPU裝箱口");

                //3.3 CPU拆箱口有沒有空(這段要等QQ)

                //如果有派車去拆箱口or拆箱口沒空
                if (!DispatchStatus.Any() && 1 == 1)
                {
                    //派車前往暫存區拿貨
                    AddTask("Storage_2");
                }
            }
        }

        private void InsertPackageLog(string RFID, string StorageBin, string AGVID, string Purpose)
        {
            using (SqlConnection ConnStr = new SqlConnection(WebConfigurationManager.ConnectionStrings["Micron_AGV_DB"].ConnectionString))
            {
                string InsertLogStr = "INSERT INTO [PurchaseAndShipmentLog] ([RFID],[Storage],[UpdateTime],[Cargostatus],[AGVID]) VALUES (@RFID,@Storage,@UpdateTime,@Cargostatus,@AGVID)";
                int AffectedRows = ConnStr.Execute(InsertLogStr, new { RFID = RFID, Storage = StorageBin, UpdateTime = DateTime.Now, Cargostatus = Purpose, AGVID = AGVID });
            }
        }

        private void RecordDispatchErrorLog(DateTime Time, string Message, string FunctionName)
        {
            var InsertErrorLog = new DispatchErrorLog();
            InsertErrorLog.Time = Time;
            InsertErrorLog.Message = Message;
            InsertErrorLog.FunctionName = FunctionName;
            _db.DispatchErrorLogs.Add(InsertErrorLog);
        }

        public string CombineJsonStr(string ActionType, string AGVID)
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

        public string Call_KMR()
        {
            TcpClient tcpClient = new TcpClient("192.168.12.36", 5678);
            Encoding encode = Encoding.ASCII;
            Char splitchar = '-';

            // Uses the GetStream public method to return the NetworkStream.
            NetworkStream netStream = tcpClient.GetStream();
            buffer = new ArrayList(encode.GetBytes("FabIn"));
            string HEADER = "msgStart-";
            string FOOTER = "-msgEnd";
            Byte[] HEADERBytes = Encoding.UTF8.GetBytes(HEADER);
            Byte[] FOOTERBytes = Encoding.UTF8.GetBytes(FOOTER);

            if (netStream.CanWrite)
            {
                Byte[] sendBytes = Encoding.UTF8.GetBytes("FabIn");
                netStream.Write(HEADERBytes, 0, HEADERBytes.Length);
                netStream.Write(sendBytes, 0, sendBytes.Length);
                netStream.Write(FOOTERBytes, 0, FOOTERBytes.Length);
                netStream.WriteByte(2);
            }
            else
            {
                tcpClient.Close();
                netStream.Close();
            }

            if (netStream.CanRead)
            {
                try
                {
                    int i = netStream.ReadByte();
                    while (i > -1)
                    {
                        if (i == 1)
                            buffer.Clear();
                        else if (i == 2)
                            break;
                        else
                            buffer.Add((byte)i);
                        i = netStream.ReadByte();
                    }
                }
                catch (Exception ex)
                {
                    if (netStream != null)
                        netStream.Close();
                    if (tcpClient != null)
                        tcpClient.Close();
                    return "E";
                }
                finally
                {
                    if (netStream != null)
                        netStream.Close();
                    if (tcpClient != null)
                        tcpClient.Close();
                }
                if (buffer != null && buffer.Count > 0)
                {
                    byte[] rbyte = (byte[])buffer.ToArray(typeof(System.Byte));
                    string result = encode.GetString(rbyte, 0, rbyte.Length);
                    if (String.IsNullOrEmpty(result) || result.IndexOf(splitchar) < 0)
                    {
                        return "收到錯誤的資訊內容!";
                    }
                    else
                    {
                        string length = result.Substring(0, result.IndexOf(splitchar));
                        int datalength = 0;
                        try
                        {
                            datalength = int.Parse(length);
                        }
                        catch
                        {
                            return "收到錯誤的資訊內容!";
                            //throw new InvalidOperationException("LEN must be \"Number\"!!");
                        }
                        if (datalength == result.Length - length.Length - 1)
                        {
                            if (result.Substring(length.Length + 1) == "Y")
                            {
                                return "goodjob";
                            }
                        }
                        else
                        {
                            return "收到錯誤的資訊內容!";
                        }
                    }
                }
            }
            else
            {
                tcpClient.Close();
                netStream.Close();
                return "E";
            }
            tcpClient.Close();
            netStream.Close();
            return "Y2";
        }
        //以下別動
    }
}
