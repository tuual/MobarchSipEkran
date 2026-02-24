using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using MobarchSipEkran.Class;
using MobarchSipEkran.DbHelper;

namespace MobarchSipEkran
{
    public partial class mainSiparis : System.Web.UI.Page
    {
        private readonly CultureInfo tr = new CultureInfo("tr-TR");

        Alert alert = new Alert();
        // kdv hesaplama gpt den aldım
        private static decimal NormalizeKdv(object val, CultureInfo culture)
        {
            if (val == null || val == DBNull.Value) return 0m;
            var k = Convert.ToDecimal(val, culture); // 1  veya 0.01 gelebilir
            if (k >= 1m) k /= 100m;                   // 1 -> 0.01
            if (k < 0m) k = 0m;
            return Math.Round(k, 4);
        }

        private decimal ParseDec(string s)
        {
            return decimal.TryParse(s ?? "0", NumberStyles.Any, tr, out var d) ? d : 0m;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            txtBelgeNo.Enabled = false;
            
            var conn = Db.ResolveConnStr();
            if (string.IsNullOrEmpty(conn))
            {
                ClientScript.RegisterStartupScript(GetType(), "yok", "alert('Bağlantı bulunamadı.');", true);
                Response.Redirect("/mainLogin.aspx");
            }
            if (!IsPostBack)
            {
               

                try
                {
                    txtBelgeNo.Text = GetNextFatirsNoFromSeri();
                }
                catch (Exception ex)
                {
                    alert.AlertMsg("Belge numarası üretilemedi: " + ex.Message, this,"belgeNoUyari");
                    txtBelgeNo.Text = "";
                }
                txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
                BindGrid();
                RecalcTotals();
              
                CariBilgileri();


             
            }
        }

        private DataTable CreateStoklarTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("StokKodu");
            dt.Columns.Add("StokAdi");
            dt.Columns.Add("Miktar", typeof(decimal));
            dt.Columns.Add("Fiyat", typeof(decimal));
            dt.Columns.Add("Tutar", typeof(decimal));
            dt.Columns.Add("KdvOran", typeof(decimal));
            dt.Columns.Add("KdvTutar", typeof(decimal));
            dt.Columns.Add("KdvDahilTutar", typeof(decimal));
            return dt;
        }
        private DataTable Stoklar
        {
            get
            {
                if (Session["Stoklar"] == null)
                    Session["Stoklar"] = CreateStoklarTable();

                return (DataTable)Session["Stoklar"];
            }
            set
            {
                Session["Stoklar"] = value;
            }
        }

        private void BindGrid()
        {
           
                if (Session["ConnStr"] == null)
                {
                    return;
                }
                else
                {
                    string sessionID = Session["SID"].ToString();
                    string sql = @"SELECT
	                    T.StokKodu
                       ,S.STOK_ADI AS StokAdi
                       ,T.Miktar
                       ,S.SATIS_FIAT1 AS Fiyat
                       ,(T.Miktar * S.SATIS_FIAT1) AS Tutar
                       ,(S.KDV_ORANI / 100.0) AS KdvOran
                       ,CASE S.SatisKdvDahil 
                        WHEN 1 THEN 
                            CAST((ISNULL(T.Miktar, 0) * S.SATIS_FIAT1) AS DECIMAL(18,4)) - 
                            CAST(((ISNULL(T.Miktar, 0) * S.SATIS_FIAT1) / (1.0 + (CAST(ISNULL(S.KDV_ORANI, 0) AS DECIMAL(18,4)) / 100.0))) AS DECIMAL(18,4))
                        ELSE 
                            CAST((ISNULL(T.Miktar, 0) * S.SATIS_FIAT1 * CAST(ISNULL(S.KDV_ORANI, 0) AS DECIMAL(18,4)) / 100.0) AS DECIMAL(18,4))
                    END AS 'KdvTutar'
                       , CAST((((T.Miktar * S.SATIS_FIAT1) * S.KDV_ORANI) / 100.0) + (T.Miktar * S.SATIS_FIAT1) AS DECIMAL(18,4)) AS 'KdvDahilTutar'
                    FROM tWebSiparis T
                    INNER JOIN tStokMaster S
	                    ON T.StokKodu = S.STOK_KODU
                    WHERE T.SessionID =  @SID";
                    DataTable dt = Db.ExecuteDataTable(sql, new SqlParameter("@SID", sessionID));
                    Session["Stoklar"] = dt;
                    gvStoklar.DataSource = dt;
                    gvStoklar.DataBind();
                RecalcTotals();
                }
            }
           

            
            
             
        

        protected void stokSecModal_StokSecildi(object sender, StokSec.StokSecEventArgs e)
        {
          

            try
            {
                BindGrid();
                RefreshTotalsPanel();
                upGrid.Update();
                upTotals.Update();


            }
            catch (Exception ex)
            {

                ScriptManager.RegisterStartupScript(this, GetType(), "hata", "alert('Hata oluştu: " + ex.Message.Replace("'", "\\'") + "');", true);
            }

        }

        protected void btnEkle_Click(object sender, EventArgs e)
        {
        }



        protected void gvStoklar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out var rowIndex)) return;
            if (rowIndex < 0 || rowIndex >= gvStoklar.Rows.Count) return;

            var kod = Convert.ToString(gvStoklar.DataKeys[rowIndex].Value);

            if (e.CommandName == "Sil")
            {
                var dt = Stoklar;
                var dr = dt.AsEnumerable().FirstOrDefault(r => r.Field<string>("StokKodu") == kod);
                if (dr != null)
                {
                    var varmi = Db.ExecuteNonQuery("SELECT COUNT(*) FROM tWebSiparis WHERE StokKodu = @SK AND SessionId = @SID", new SqlParameter("@SK", kod),
                        new SqlParameter("@SID", Session.SessionID));
                    if (varmi == 0)
                    {
                        BildirimHelper.MesajGoster(this, "Ürün tWebSiparis tablosunda SessionId ve StokKodu bulunamadı", false);
                    }
                    Db.ExecuteNonQuery("Delete from tWebSiparis WHERE StokKodu = @SK and SessionId = @SID", new SqlParameter("@SK", kod),
                        new SqlParameter("@SID", Session.SessionID));
                    Db.ExecuteNonQuery("Delete from tWebSiparisDetayTemp WHERE StokKodu = @SK and SessionId = @SID", new SqlParameter("@SK", kod),
                        new SqlParameter("@SID", Session.SessionID));
                    
                    dt.Rows.Remove(dr);   
                }

                Stoklar = dt;
                BindGrid();
                RefreshTotalsPanel();    
            }
        }
        /*  protected void RowMiktar_TextChanged(object sender, EventArgs e)
          {
              var txt = (TextBox)sender;
              var row = (GridViewRow)txt.NamingContainer;
              int rowIndex = row.RowIndex;
              if (rowIndex < 0) return;

              var dt = Stoklar;
              var kod = Convert.ToString(gvStoklar.DataKeys[rowIndex].Value);
              var dr = dt.AsEnumerable().First(r => r.Field<string>("StokKodu") == kod);

              decimal yeniMiktar = ParseDec(txt.Text);
              decimal fiyat = dr["Fiyat"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["Fiyat"]);
              decimal kdvOran = dr["KdvOran"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["KdvOran"]);
              Db.ExecuteNonQuery("UPDATE tWebSiparisDetayTemp SET Miktar = @Miktar WHERE StokKodu = @SK and SessionId = @SID ",
                  new SqlParameter("@Miktar", yeniMiktar), new SqlParameter("@SK", kod),
                  new SqlParameter("@SID", Session.SessionID.ToString()));
              decimal net = Math.Round(yeniMiktar * fiyat, 6);
              decimal kdv = Math.Round(net * kdvOran, 6);
              decimal dahil = net + kdv;



              Stoklar = dt;
              BindGrid();
              RefreshTotalsPanel();
          }

        */
        private void RecalcTotals()
        {
            var dt = (DataTable)Session["Stoklar"];
            if (dt == null || dt.Rows.Count == 0)
            {
                txtBrutTutar.Text = "0,00";
                txtGenelToplam.Text = "0,00";
                txtIskonto.Text = "0.00";
                txtAraToplam.Text = "0.00";
                txtKdvToplam.Text = "0.00";
                return;
            }

            decimal brutTutar = dt.AsEnumerable().Sum(r => r.Field<decimal>("Tutar"));
            decimal toplamKdv = dt.AsEnumerable().Sum(r => r.Field<decimal>("KdvTutar"));

            decimal kdvDahilToplam = dt.AsEnumerable().Sum(r => r.Field<decimal>("KdvDahilTutar"));

            decimal iskonto = ParseDec(txtIskonto.Text);

            decimal araToplam = brutTutar - iskonto;

            decimal factor = brutTutar > 0m ? araToplam / brutTutar : 1m;
            decimal sonKdv = Math.Round(toplamKdv * factor, 2);

            decimal genelToplam = araToplam + sonKdv;

            txtBrutTutar.Text = brutTutar.ToString("N2", tr);
            txtAraToplam.Text = araToplam.ToString("N2", tr);
            txtKdvToplam.Text = sonKdv.ToString("N2", tr);
            txtGenelToplam.Text = genelToplam.ToString("N2", tr);
        }


        protected void btnKaydet_Click(object sender, EventArgs e)
        {

            List<string> qList = new List<string>();


        

            var sipOnayliDuserQuery = @"SELECT SIPARISONAYLISEVK from tSistemParam";
            var sipOnayliDuser = Db.ExecuteDataTable(sipOnayliDuserQuery);
            int deger = 0;
            if (sipOnayliDuser.Rows.Count > 0)
            {
                deger = Convert.ToInt16(sipOnayliDuser.Rows[0]["SIPARISONAYLISEVK"]);
            }

            DateTime dateTime = DateTime.Now;
            string sqlFormattedDate = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var zarfID = Guid.NewGuid().ToString().Trim(); 
            var sessionID = Session["SID"].ToString();
            var cariKodu = Session["ALTCARIKOD"] != null ? Session["ALTCARIKOD"].ToString() : "";

            var brutttutar = ParseDec(txtBrutTutar.Text.Trim());
            var belgeNo = txtBelgeNo.Text.Trim();
            var tarih = Convert.ToDateTime(txtTarih.Text).ToString("yyyy-MM-dd HH:mm:ss.fff");
            var caKod = cariKodu;
            var kdvDahilMi = 'Y';
            var odemeTarihi = sqlFormattedDate;

            var fatKalemAdedi = (Session["Stoklar"] as DataTable)?.Rows.Count ?? 0;

            var siparisTest = sqlFormattedDate;
            var genelToplam = ParseDec(txtGenelToplam.Text);
            var kdvToplam = ParseDec(txtKdvToplam.Text);
            var fiYatTarihi = sqlFormattedDate;
            var dYedek10 = sqlFormattedDate;
            var aciklama = txtAciklama.Text.Trim();
            var eFaturano = txtBelgeNo.Text.Trim();

           
            char onayDurumuVal = (deger == 0) ? 'Y' : 'H';
            char onayTipiVal = 'A';
            string onayTarihiVal = sqlFormattedDate;
          

            qList.Add(@"INSERT INTO tSiparis (SUBE_KODU, FTIRSIP, FATIRS_NO, CARI_KODU, TARIH, TIPI, BRUTTUTAR, SAT_ISKT, MFAZ_ISKT, 
                        GEN_ISK1T, GEN_ISK2T, GEN_ISK3T, GEN_ISK1O, GEN_ISK2O, GEN_ISK3O, KDV, FAT_ALTM1, FAT_ALTM2, ODEMEGUNU, ODEMETARIHI,
                        KDV_DAHILMI, FATKALEM_ADEDI, SIPARIS_TEST, TOPLAM_MIK, TOPDEPO, SIRANO, KDV_DAHIL_BRUT_TOP, KDV_TENZIL, MALFAZLASIKDVSI,
                        GENELTOPLAM, YUVARLAMA, PLA_KODU, DOVIZTIP, DOVIZTUT, KS_KODU, BAG_TUTAR, F_Yedek3, F_Yedek4, F_Yedek5, C_Yedek6, B_Yedek7, 
                        I_Yedek8, L_Yedek9, D_Yedek10, FIYATTARIHI, GENISK1TIP, GENISK2TIP, GENISK3TIP, EXPORTTYPE, KayItYapanKul, KayItTarIhI, DuzeltmeYapanKul,
                        DuzeltmeTarIhI, GELSUBE_KODU, GITSUBE_KODU, OnayTIpI, OnayNum, ISLETME_KODU, BRMALIYET, KOSVADEGUNU, CARI_KOD2, ACIKLAMA, YEDEK22, SAP_SPRSN,
                        SAP_TSLMTN, SAP_FTRN, SAP_AKTZMN, SAP_AKTKL, BIZ_SPRSN, BIZ_TSLMTN, BIZ_FTRN, E_FATURANO, FATIRS_SERI, YAZDIRILDI, KOSULKODU, EXPORTREFNO, ZNO, 
                        FNO, KAPALIFATURA, KASAKODU, ZARFID, E_IRSALIYE, MUHTELIF_MUSTERI, EBELGENOGECERLI, EBELGETASLAKBASILDI, ONAYDURUMU, ONAYKULLANICI, ONAYTARIHI,
                        KONAKLAMA, KONAKLAMAVORAN, KONAKLAMAVTUTAR, KOD1, EXGUMRUKNO, FARKLITESLIMID, IHRACATMI, IHRBRUTKILO, IHRNETKILO, IHRISTISNAKOD, OTV, NAKIT, KREDI, 
                        EXTFL_I, EXTFL_C, TESLIMEDILDI, KAMYONID, REVNO)
	                    VALUES ('0', '6', '"+belgeNo+@"', '"+cariKodu+@"', '"+tarih+@"', 2, "+brutttutar.ToString().Replace(',','.')+@", 0, 0, 0, 0, 0, 0, 0, 0, "+kdvToplam.ToString().Replace(',', '.') + @", 0, 0, 0, '"+odemeTarihi+@"','"+kdvDahilMi+@"', "+fatKalemAdedi+@", '"+siparisTest+@"',
	                    0, 100, 0, 0, 0, 0, "+genelToplam.ToString().Replace(',', '.') + @", 0, '100', '0', '0', 0, 0, 0, 0, 0, 0, 0, 0, 0, '"+dYedek10+@"', '"+fiYatTarihi+@"', 0, 0, 0, 0, 'Web Sipariş Portalı', '"+dYedek10+@"'
	                    ,'', '2000-01-01 00:00:00', 0, 0, '"+onayTipiVal+@"', 0, 1, 0.0000000, 0, '', '"+aciklama+@"', 'SF', '', '', '', '2001-01-01 00:00:00', '', '', '', '', '', '',
	                    '0', '0', '', '', '', '0', '', '"+zarfID+@"', '0', '0', '0', '0', '"+onayDurumuVal+@"', '', '"+onayTarihiVal+@"', '0', '0', '0', '', '', 
	                    '0', '0', '0', '0', '', '0', '0', '0', '', '', 0, 0, 0)");

            qList.Add(@"
INSERT INTO dbo.tSiparisDetay(STOK_KODU, FISNO, STHAR_GCMIK, STHAR_GCMIK2, CEVRIM, STHAR_GCKOD, STHAR_TARIH, STHAR_NF, STHAR_BF, STHAR_IAF, STHAR_KDV, DEPO_KODU, 
STHAR_ACIKLAMA, STHAR_SATISK, STHAR_MALFISK, STHAR_FTIRSIP, STHAR_SATISK2, LISTE_FIAT, STHAR_HTUR, STHAR_DOVTIP, PROMASYON_KODU, STHAR_DOVFIAT, STHAR_ODEGUN, 
STRA_SATISK3, STRA_SATISK4, STRA_SATISK5, STRA_SATISK6, STHAR_BGTIP, STHAR_KOD1, STHAR_KOD2, STHAR_SIPNUM, STHAR_CARIKOD, STHAR_SIP_TURU, PLASIYER_KODU, EKALAN_NEDEN,
EKALAN, EKALAN1, REDMIK, REDNEDEN, SIRA, STRA_SIPKONT, AMBAR_KABULNO, FIRMA_DOVTIP, FIRMA_DOVTUT, FIRMA_DOVMAL, UPDATE_KODU, IRSALIYE_NO, IRSALIYE_TARIH, KOSULKODU,
ECZA_FAT_TIP, STHAR_TESTAR, OLCUBR, VADE_TARIHI, LISTE_NO, BAGLANTI_NO, SUBE_KODU, MUH_KODU, S_YEDEK1, S_YEDEK2, F_YEDEK3, F_YEDEK4, F_YEDEK5, C_YEDEK6, B_YEDEK7, I_YEDEK8, 
L_YEDEK9, D_YEDEK10, PROJE_KODU, FIYATTARIHI, KOSULTARIHI, SATISK1TIP, SATISK2TIP, SATISK3TIP, SATISK4TIP, SATISK5TIP, SATISK6TIP, EXPORTTYPE, EXPORTMIK, ONAYTIPI, ONAYNUM,
KKMALF, STRA_IRSKONT, YAPKOD, MAMYAPKOD, GIRIS_DEPO, CIKIS_DEPO, STHAR_ZFA1, STHAR_ZLSF, STHAR_DOVKUR, STHAR_DOVTARIH, STHAR_DOVKOD, STHAR_DOVAD, SKTACIK, SATIRACIKLAMA, 
MAS_ZARFID, PALET_ID, GSTHAR_BF, GSTHAR_NF, MAS_FISNO, SATINALMATALEPID, STHARGUID, ALT_BT, BELGE_DOVKUR, IHRTESLIMSARTI, IHRGONDERIMSEKLI, IHRKABMARKA, IHRGTNO, IHRKABCINSI,
IHRKABNO, IHRKABADET, KAR_MALIYET, KAR_BIRIM, KAR_TUTAR, KAR_ORAN, KAR_TARIH, KAR_MALIYETTIPI, OTVORAN, OTVTIPID, ZANAKOSULTIPI, SORID, BIN_MALIYET, IND_MALIYET, BIN_DEG_MALIYET,
IND_DEG_MALIYET, SIPKAPATILDI, DUZELTMETARIHI,
SOZID, ORJ_ISLEM_MIK, TEVKIFATID)
SELECT tws.StokKodu, -- STOK_KODU - nvarchar(35)
'"+belgeNo+@"', -- FISNO - nvarchar(15)
    tws.Miktar2, -- STHAR_GCMIK - float
    tws.Miktar, -- STHAR_GCMIK2 - float
    1.00000000000000, -- CEVRIM - decimal(28, 14)
    'C', -- STHAR_GCKOD - char(1)
    '"+tarih+ @"', -- STHAR_TARIH - datetime
    tws.NetFiyat, -- STHAR_NF - float
    tws.BrutFiyat, -- STHAR_BF - float
    0, -- STHAR_IAF - float
    ST.KDV_ORANI, -- STHAR_KDV - int
    100, -- DEPO_KODU - int
    NULL, -- STHAR_ACIKLAMA - varchar(400)
    0, -- STHAR_SATISK - float
    0.00000, -- STHAR_MALFISK - decimal(15, 5)
    '6', -- STHAR_FTIRSIP - char(1)
    0, -- STHAR_SATISK2 - float
    1, -- LISTE_FIAT - int
    'H', -- STHAR_HTUR - char(1)
    0, -- STHAR_DOVTIP - int
    0, -- PROMASYON_KODU - int
    0.000000000000000, -- STHAR_DOVFIAT - decimal(28, 15)
    0, -- STHAR_ODEGUN - int
    0, -- STRA_SATISK3 - float
    0, -- STRA_SATISK4 - float
    0, -- STRA_SATISK5 - float
    0, -- STRA_SATISK6 - float
    'I', -- STHAR_BGTIP - char(1)
    NULL, -- STHAR_KOD1 - char(1)
    NULL, -- STHAR_KOD2 - char(1)
    NULL, -- STHAR_SIPNUM - nvarchar(15)
    '"+cariKodu+@"', -- STHAR_CARIKOD - nvarchar(400)
    NULL, -- STHAR_SIP_TURU - char(1)
    100, -- PLASIYER_KODU - nvarchar(25)
    NULL, -- EKALAN_NEDEN - char(1)
    NULL, -- EKALAN - nvarchar(450)
    NULL, -- EKALAN1 - varchar(35)
    0.00000, -- REDMIK - decimal(15, 5)
    0, -- REDNEDEN - int
    1, -- SIRA - int
    1, -- STRA_SIPKONT - int
    NULL, -- AMBAR_KABULNO - varchar(15)
    0, -- FIRMA_DOVTIP - int
    0, -- FIRMA_DOVTUT - float
    0, -- FIRMA_DOVMAL - float
    NULL, -- UPDATE_KODU - char(1)
    '', -- IRSALIYE_NO - nvarchar(15)
    '2000-01-01', -- IRSALIYE_TARIH - datetime
    NULL, -- KOSULKODU - varchar(8)
    0, -- ECZA_FAT_TIP - int
    GETDATE(), -- STHAR_TESTAR - datetime
    1, -- OLCUBR - int
    '"+tarih+@"', -- VADE_TARIHI - datetime
    NULL, -- LISTE_NO - varchar(8)
    0, -- BAGLANTI_NO - int
    0, -- SUBE_KODU - int
    NULL, -- MUH_KODU - nvarchar(35)
    NULL, -- S_YEDEK1 - nvarchar(35)
    tws.BirimMetin, -- S_YEDEK2 - varchar(8)
    0, -- F_YEDEK3 - float
    0, -- F_YEDEK4 - float
    0, -- F_YEDEK5 - float
    NULL, -- C_YEDEK6 - char(1)
    0, -- B_YEDEK7 - int
    0, -- I_YEDEK8 - int
    0, -- L_YEDEK9 - int
    GETDATE(), -- D_YEDEK10 - datetime
    NULL, -- PROJE_KODU - nvarchar(25)
    GETDATE(), -- FIYATTARIHI - datetime
    NULL, -- KOSULTARIHI - datetime
    0, -- SATISK1TIP - int
    0, -- SATISK2TIP - int
    0, -- SATISK3TIP - int
    0, -- SATISK4TIP - int
    0, -- SATISK5TIP - int
    0, -- SATISK6TIP - int
    0, -- EXPORTTYPE - int
    0, -- EXPORTMIK - float
    'A', -- ONAYTIPI - char(1)
    0, -- ONAYNUM - int
    0, -- KKMALF - float
    1, -- STRA_IRSKONT - int
    NULL, -- YAPKOD - varchar(15)
    '', -- MAMYAPKOD - varchar(15)
    NULL, -- GIRIS_DEPO - nvarchar(15)
    NULL, -- CIKIS_DEPO - nvarchar(15)
    0.00, -- STHAR_ZFA1 - decimal(18, 2)
    0.00, -- STHAR_ZLSF - decimal(18, 2)
    1.00000, -- STHAR_DOVKUR - decimal(18, 5)
    GETDATE(), -- STHAR_DOVTARIH - datetime
    0, -- STHAR_DOVKOD - int
    '', -- STHAR_DOVAD - nvarchar(10)
    '', -- SKTACIK - nvarchar(25)
    NULL, -- SATIRACIKLAMA - nvarchar(150)
    '"+zarfID+@"', -- MAS_ZARFID - uniqueidentifier
    NULL, -- PALET_ID - uniqueidentifier
    0.00000, -- GSTHAR_BF - decimal(18, 5)
    0.00000, -- GSTHAR_NF - decimal(18, 5)
    NULL, -- MAS_FISNO - nvarchar(15)
    NULL, -- SATINALMATALEPID - int
    NULL, -- STHARGUID - uniqueidentifier
    NULL, -- ALT_BT - nvarchar(10)
    NULL, -- BELGE_DOVKUR - decimal(18, 5)
    0, -- IHRTESLIMSARTI - int
    0, -- IHRGONDERIMSEKLI - int
    '', -- IHRKABMARKA - nvarchar(150)
    '', -- IHRGTNO - nvarchar(35)
    0, -- IHRKABCINSI - int
    '', -- IHRKABNO - nvarchar(100)
    0, -- IHRKABADET - int
    NULL, -- KAR_MALIYET - decimal(18, 5)
    NULL, -- KAR_BIRIM - decimal(18, 5)
    NULL, -- KAR_TUTAR - decimal(18, 5)
    NULL, -- KAR_ORAN - decimal(18, 2)
    NULL, -- KAR_TARIH - datetime
    NULL, -- KAR_MALIYETTIPI - nvarchar(10)
    0.00, -- OTVORAN - decimal(18, 2)
    0, -- OTVTIPID - int
    'Z000', -- ZANAKOSULTIPI - nvarchar(7)
    0, -- SORID - int
    0.00000, -- BIN_MALIYET - decimal(18, 5)
    0.00000, -- IND_MALIYET - decimal(18, 5)
    0.00000, -- BIN_DEG_MALIYET - decimal(18, 5)
    0.00000, -- IND_DEG_MALIYET - decimal(18, 5)
    0, -- SIPKAPATILDI - int
    NULL, -- DUZELTMETARIHI - datetime
    0, -- SOZID - int
    0, -- ORJ_ISLEM_MIK - float
    0 -- TEVKIFATID - int
FROM dbo.tWebSiparis AS tws 
Left Join tStokMaster ST ON ST.STOK_KODU = tws.StokKodu 
WHERE tws.SessionId = '"+sessionID+@"'");

            qList.Add(@"DELETE FROM tWebSiparisDetayTemp WHERE SessionId = '" + sessionID + @"'");

            try
            {
                Db.ExecuteTransaction(cmd =>
                {
                    foreach (var q in qList)
                    {
                        cmd.CommandText = q;
                        cmd.ExecuteNonQuery();
                    }
                });
                /*    Db.ExecuteTransaction(cmd =>
                    {
                        cmd.CommandText = kayitQuery;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@BELGENO", belgeNo);
                        cmd.Parameters.AddWithValue("@CAKOD", caKod);
                        cmd.Parameters.AddWithValue("@TARIH", tarih);
                        cmd.Parameters.AddWithValue("@BRUTTUTAR", brutttutar);
                        cmd.Parameters.AddWithValue("@KDV", kdvToplam);
                        cmd.Parameters.AddWithValue("@ODEMETARIHI", odemeTarihi);
                        cmd.Parameters.AddWithValue("@KDVDAHILMI", kdvDahilMi);
                        cmd.Parameters.AddWithValue("@FATKALEMADEDI", fatKalemAdedi);
                        cmd.Parameters.AddWithValue("@SIPARISTEST", siparisTest);
                        cmd.Parameters.AddWithValue("@GENELTOPLAM", genelToplam);
                        cmd.Parameters.AddWithValue("@DYEDEK10", dYedek10);
                        cmd.Parameters.AddWithValue("@FIYATTARIHI", fiYatTarihi);
                        cmd.Parameters.AddWithValue("@KAYITTARIHI", dYedek10);
                        cmd.Parameters.AddWithValue("@ONAYTIPI", onayTipiVal);
                        cmd.Parameters.AddWithValue("@EFATURANO", eFaturano);
                        cmd.Parameters.AddWithValue("@ZARFID", zarfID);
                        cmd.Parameters.AddWithValue("@ACIKLAMA", aciklama);
                        cmd.Parameters.AddWithValue("@ONAYDURUMU", onayDurumuVal);
                        cmd.Parameters.AddWithValue("@ONAYTARIHI", onayTarihiVal);

                        cmd.ExecuteNonQuery();


                        /* cmd.CommandText = @"INSERT INTO tStokMasterHareket 
                                    (ZARFID, STOK_KODU, MIKTAR, FIYAT, KDV_ORANI, TARIH)
                                    SELECT @ZARFID, StokKodu, Miktar, Fiyat, 0, GETDATE() 
                                    FROM tWebSiparisDetayTemp 
                                    WHERE SessionID = @SID";

                         cmd.Parameters.Clear();
                         cmd.Parameters.AddWithValue("@ZARFID", zarfID);
                         cmd.Parameters.AddWithValue("@SID", sessionID);
                         cmd.ExecuteNonQuery();
                        */
              
                
                BildirimHelper.MesajGoster(this, "Sipariş Başarıyla Oluşturuldu.", false);  

                Session["Stoklar"] = null;
                textleriSil();
                BindGrid();
                Response.Redirect(Request.RawUrl);
            }

            catch (SqlException ex)
            {

                BildirimHelper.MesajGoster(this, "Sipariş kaydedilirken bir hata oluştu: " + ex.Message, true);
            }
        }
        protected void gvStoklar_PreRender(object sender, EventArgs e)
        {
            if (gvStoklar.HeaderRow != null)
            {
                gvStoklar.UseAccessibleHeader = true;
                gvStoklar.HeaderRow.TableSection = TableRowSection.TableHeader;
            }
        }

        protected void txtIskonto_TextChanged(object sender, EventArgs e)
        {

        }

        private void RefreshTotalsPanel()
        {
            RecalcTotals();
            CariBilgileri();
            upTotals.Update();
        }


        private void CariBilgileri()
        {
            var info = Services.CariService.GetFromSession();

            if (info == null)
            {
                lblCariBakiye.Text = "0,00";
                lblRiskLimiti.Text = "0,00";
                lblKalanLimit.Text = "0,00";
                return;
            }

            decimal bakiye = info.Bakiye;
            decimal riskLimiti = info.RiskLimiti;
            decimal siparisToplami = ParseDec(txtGenelToplam.Text);

            lblCariBakiye.Text = bakiye.ToString("N2", tr);
            lblRiskLimiti.Text = riskLimiti.ToString("N2", tr);

            decimal kalanLimit = riskLimiti - bakiye - siparisToplami;

            lblKalanLimit.Text = kalanLimit.ToString("N2", tr);

            if (kalanLimit < 0)
            {
                lblKalanLimit.ForeColor = Color.Red;
            }
            else
            {
                lblKalanLimit.ForeColor = Color.Black;
            }
        }

             
        
        private string GetNextFatirsNoFromSeri()
        {
            var seriRow = Db.ExecuteRow(
                "SELECT IMPSERI FROM dbo.tMainEvrakSerileri WHERE KULID = 0");

            if (seriRow == null || seriRow["IMPSERI"] == DBNull.Value)
                throw new Exception("IMPSERI bulunamadı (tMainEvrakSerileri.KULID=0).");

            string imps = Convert.ToString(seriRow["IMPSERI"]).Trim();
            if (string.IsNullOrEmpty(imps))
                throw new Exception("IMPSERI boş geldi.");

            string prefix = imps.Length >= 3 ? imps.Substring(0, 3) : imps.PadRight(3, '0');

            const string sql = @"
        SELECT MAX(FATIRS_NO) AS SONNO
        FROM dbo.tSiparis
        WHERE FATIRS_NO LIKE @P + '%';
    ";

            var row = Db.ExecuteRow(sql, new SqlParameter("@P", prefix));

            string last = (row == null || row["SONNO"] == DBNull.Value)
                ? null
                : Convert.ToString(row["SONNO"]);

            int totalLen = 15;                       
            int numericLen = totalLen - prefix.Length;

            if (numericLen <= 0)
                throw new Exception("Belge numarası uzunluğu hatalı (prefix çok uzun).");

            if (string.IsNullOrWhiteSpace(last))
            {
                string firstNumber = 1.ToString().PadLeft(numericLen, '0');
                return prefix + firstNumber;
            }

            if (!last.StartsWith(prefix))
                throw new Exception("Son FATIRS_NO beklenen seri ile başlamıyor.");

            string numericPart = last.Substring(prefix.Length);
            if (!numericPart.All(char.IsDigit))
                throw new Exception("FATIRS_NO sayısal olmayan karakter içeriyor.");

            long n = long.Parse(numericPart);
            n++; 

            string nextNumeric = n.ToString().PadLeft(numericPart.Length, '0');

            // 5) Yeni belge numarası
            return prefix + nextNumeric;
        }
        private void textleriSil()
        {
            txtBelgeNo.Text = GetNextFatirsNoFromSeri();
            txtTarih.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtIskonto.Text = "0";
                txtBrutTutar.Text = "0,00";
                txtAraToplam.Text = "0,00";
               txtGenelToplam.Text = "0,00";
                txtKdvToplam.Text = "0,00"; 
            txtAciklama.Text = "";
            CariBilgileri();
        }
       


    }
}
