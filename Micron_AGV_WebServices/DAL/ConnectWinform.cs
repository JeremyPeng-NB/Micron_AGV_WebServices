using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Micron_AGV_WebServices.DAL
{
    public class ConnectWinform
    {
        public void E84WinformTest()
        {
            string EXEPath = @"D:\自學\WindowsFormsApplication1\WindowsFormsApplication1\bin\Release";
            Process exe = new Process();
            exe.StartInfo.FileName = EXEPath + @"\WindowsFormsApplication1.exe";
            exe.StartInfo.Arguments = "WaterMan GOOD GOOD";
            exe.Start();
            exe.Close();
        }
    }
}