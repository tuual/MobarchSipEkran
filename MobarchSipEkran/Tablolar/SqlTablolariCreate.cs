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

            string tablo1 = "  CREATE TABLE tWebKullaniciGiris(REFKEY INT NOT NULL ,SISTEMCARIKOD NVARCHAR(50) DEFAULT '' NOT NULL,ALTCARIKOD NVARCHAR(50) DEFAULT '' NOT NULL, txtVKN NVARCHAR(50) DEFAULT '' NOT NULL,txtKadi NVARCHAR(50) DEFAULT '' NOT NULL,txtSifre NVARCHAR(50) DEFAULT '' NOT NULL    ); ";



                string tablo2 = "CREATE TABLE tWebBilgiler(SISTEMCARIKOD NVARCHAR(50) NOT NULL DEFAULT '',SISTEMBAGLANTI NVARCHAR(50) DEFAULT '' NOT NULL,SISTEMKADI NVARCHAR(50) DEFAULT '' NOT NULL,SISTEMSIFRE NVARCHAR(50) DEFAULT '' NOT NULL";
            string tablo3 = "CREATE TABLE tWebSiparisDetayTemp(SessionID nvarchar(50),StokKodu NVARCHAR(50),Miktar DECIMAL(18,2),KayitTarihi datetime,Fiyat Decimal(18,2))";
            string tablo4 = "CREATE TABLE [dbo].[tWebSiparis]([SessionId] [nvarchar](100) NULL ,[StokKodu] [nvarchar](50) NULL,[Miktar] [decimal](18, 2) NULL,[KayitTarihi] [datetime] NULL,[Fiyat] [decimal](18, 2) NULL) ON [PRIMARY]";
        } 
    }
}