using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MobarchSipEkran
{
	public static class Db
	{
        public static string ResolveConnStr()
        {
            var ctx = System.Web.HttpContext.Current;
            var fromSession = ctx?.Session?["ConnStr"] as string;
            if (!string.IsNullOrWhiteSpace(fromSession))
                return fromSession;

            return ConfigurationManager.ConnectionStrings["DefaultConn"]?.ConnectionString
                   ?? throw new InvalidOperationException("Bağlantı dizesi bulunamadı.");
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var con = new SqlConnection(ResolveConnStr()))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var con = new SqlConnection(ResolveConnStr()))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteDataTable(string sql, params SqlParameter[] parameters)
        {
            using (var con = new SqlConnection(ResolveConnStr()))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static object DbNull(object value) => value ?? DBNull.Value;

        public static DataRow ExecuteRow(String sql, params SqlParameter[] prms)
        {
            using (var con = new SqlConnection(ResolveConnStr()))
            using (var da = new SqlDataAdapter(sql, con))
            {
                if(prms != null && prms.Length > 0)
                
                    da.SelectCommand.Parameters.AddRange(prms);
                var dt = new DataTable();
                da.Fill(dt);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                
            }
        }
    }
}