using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobarchSipEkran.Tablolar
{
    public class SqlTablolariCreate
    {
        public SqlTablolariCreate()
        {
        
            string tablo1 =
                "CREATE TABLE tWebKullaniciGiris" +
                "(" +
                "    REFKEY INT NOT NULL ," +
                "    SISTEMCARIKOD NVARCHAR(50) DEFAULT '' NOT NULL," +
                "    ALTCARIKOD NVARCHAR(50) DEFAULT '' NOT NULL," +
                "    txtVKN NVARCHAR(50) DEFAULT '' NOT NULL," +
                "    txtKadi NVARCHAR(50) DEFAULT '' NOT NULL," +
                "    txtSifre NVARCHAR(50) DEFAULT '' NOT NULL," +
                ");";

            string tablo2 = "CREATE TABLE tWebBilgiler(SISTEMCARIKOD NVARCHAR(50) NOT NULL DEFAULT '',SISTEMBAGLANTI NVARCHAR(50) DEFAULT '' NOT NULL,SISTEMKADI NVARCHAR(50) DEFAULT '' NOT NULL,SISTEMSIFRE NVARCHAR(50) DEFAULT '' NOT NULL";
        } 
    }
}