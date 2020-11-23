using Micron_AGV_WebServices.DAL;
using Micron_AGV_WebServices.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;


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
        ConnectAPI ConnAPI = new ConnectAPI();
        ConnectWinform ConnWinform = new ConnectWinform();
        ArrayList buffer = null;

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
        public void E84WinformTest()
        {
            ConnWinform.E84WinformTest();
        }

        [WebMethod]
        public string AGVTestAPI(string Content)
        {
            string responseStr = "X";

            if (Content.Contains("叫車"))
            {
                responseStr = ConnAPI.AGVTestAPI();
                return responseStr;
            }
            else
            {
                var jsonStr = DispatchFunc.CombineJsonStr(Content , "1"); //還沒做防呆!!!!! 

                responseStr = ConnAPI.AddCarMission(jsonStr);
                return responseStr;
            }
        }

        [WebMethod]
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
    }
}
