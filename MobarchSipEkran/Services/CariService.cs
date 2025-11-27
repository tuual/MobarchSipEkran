using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MobarchSipEkran.Services
{
    public static class CariService
    {
        private static readonly CultureInfo tr = new CultureInfo("tr-TR");
        private static decimal ToDec(object v) => (v == null || v == DBNull.Value) ? 0m : Convert.ToDecimal(v, tr);
        private static string ToStr(object v) => v == null || v == DBNull.Value ? "" : Convert.ToString(v);

        public static DbHelper.CariBakiye GetByCariKod(string cariKod)
        {
            var row = Db.ExecuteRow(
                @"SELECT CARI_KOD,
                         CARISK,
                         CM_BORCT - CM_ALACT AS BAKIYE
                  FROM dbo.tCariMaster WITH (NOLOCK)
                  WHERE CARI_KOD = @K",
                new SqlParameter("@K", cariKod));

            if (row == null) return null;

            return new DbHelper.CariBakiye
            {
                CariKod = ToStr(row["CARI_KOD"]),
                RiskLimiti = ToDec(row["CARISK"]),
                Bakiye = ToDec(row["BAKIYE"])
            };
        }

        // Session’daki alt cari ile kısayol
        public static DbHelper.CariBakiye GetFromSession()
        {
            DbHelper.Sessions ss = HttpContext.Current.Session["MobarchUser"] as DbHelper.Sessions;

            if (ss == null)
                return null;

            if (string.IsNullOrWhiteSpace(ss.Altcarikod))
                return null;

            return GetByCariKod(ss.Altcarikod);
        }
    
    }
}