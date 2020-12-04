﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Micron_AGV_WebServices.Model;

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
            if ((EquipmentPlace == "Tower Stock" || EquipmentPlace == "APK"))
            {
                var UpdatePlace = _db.EquipmentStatuss.Where(x => x.Place == EquipmentPlace).FirstOrDefault();

                UpdatePlace.Status = EquipmentStatus;

                _db.SaveChanges();

                return "設備狀態更新完成!";
            }
            else
            {
                return "設備名稱輸入錯誤!!";
            }
        }
    }
}