using MobarchSipEkran.Class;
using MobarchSipEkran.DbHelper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Expressions;

namespace MobarchSipEkran
{
    public partial class StokSec : UserControl
    {
        DateTime date;
        private string cariSql;
        private DataRow cariRow;
        private CariBakiye info = Services.CariService.GetFromSession();

        public class StokSecEventArgs : EventArgs
        {
            public string StokKodu { get; set; }
            public string StokAdi { get; set; }
        }

        public event EventHandler<StokSecEventArgs> onStokSecildi;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
          
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Bind("");
            }
            
        }

        private void Bind(string term)
        {
            var carikod = Session["ALTCARIKOD"].ToString();
            
             cariSql = @"select VARSAYILANKOSUL from tCariMaster WHERE CARI_KOD = @CAKOD";
            var dtCari = Db.ExecuteDataTable(cariSql, new SqlParameter("@CAKOD", carikod));
             cariRow = dtCari.Rows[0];
            var varsayilanKosul = cariRow["VARSAYILANKOSUL"];
            var iskontoQuery = "exec [sp_KalemIskontoBul] @STOKKODU,@CAKOD,@TARIH,@VADETIP,100,0";
            var iskontoHesaplama = "";


            string sql = @"
        SELECT 
            S.STOK_KODU, 
            S.STOK_ADI, 
            S.SATIS_FIAT1,
            ISNULL(T.Miktar, 0) AS KayitliMiktar, -- Eğer temp tabloda varsa miktar gelir, yoksa 0 gelir
            CASE S.SatisKdvDahil WHEN 1 THEN 'Dahil'ELSE 'Hariç' END AS 'SatisKdvDahil'
        FROM tStokMaster S WITH (NOLOCK)
        LEFT JOIN tWebSiparisDetayTemp T ON S.STOK_KODU = T.StokKodu AND T.SessionID = @SID
        WHERE (@T='' OR S.STOK_KODU LIKE @KOD OR S.STOK_ADI LIKE @AD)
        ORDER BY T.Miktar DESC";

            var dt = Db.ExecuteDataTable(sql,
                new SqlParameter("@SID", Session.SessionID),
                new SqlParameter("@T", term ?? string.Empty),
                new SqlParameter("@KOD", (term ?? string.Empty) + "%"),
                new SqlParameter("@AD", "%" + (term ?? string.Empty) + "%"));
          
            gv.DataSource = dt;
            gv.DataBind();
            
        }

     

        protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var btn = (LinkButton)e.Row.FindControl("btnSec");
                if (btn != null)
                {
                    ScriptManager.GetCurrent(Page).RegisterPostBackControl(btn);
                }
                var dataRow = e.Row.DataItem as DataRowView;
                decimal kayitliMiktar = Convert.ToDecimal(dataRow["KayitliMiktar"]);
                TextBox txt = e.Row.FindControl("txtMiktar") as TextBox;
                if (txt != null)
                {
                    txt.CssClass += "is-valid";
                }
            }
        }

        protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Ekle")
            {

             
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gv.Rows[rowIndex];
                string stokKodu = gv.DataKeys[rowIndex].Values["STOK_KODU"].ToString();
                string stokAdi = gv.DataKeys[rowIndex].Values["STOK_ADI"].ToString();
                TextBox txtMiktar = (TextBox)row.FindControl("txtMiktar");
                decimal miktar = 0;
                decimal.TryParse(txtMiktar.Text, out miktar);
                if (onStokSecildi != null)
                {
                    onStokSecildi(this, new StokSecEventArgs
                    {
                        StokKodu = stokKodu,
                        StokAdi = stokAdi
                    });
                }

                if (miktar > 0 )
                {

                  
                  TempKaydet(stokKodu, miktar, false);
                        txtMiktar.CssClass = "form-control form-control-sm is-valid";

                    

                }
               
            }

            if (e.CommandName=="Sil")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gv.Rows[rowIndex];
                string stokKoduSil = gv.DataKeys[rowIndex].Values["STOK_KODU"].ToString();
                TextBox txtMiktar = row.FindControl("txtMiktar") as TextBox;

                TempSil(stokKoduSil);
            }

        }

        private void TempKaydet(string stokKodu, decimal miktar,bool T)
        {
            string stokFiyat = "SELECT SATIS_FIAT1,KDV_ORANI,SatisKdvDahil from tStokMaster WHERE STOK_KODU = @STOK";
           var kayit = Db.ExecuteDataTable(stokFiyat, new SqlParameter("@STOK", stokKodu));
            if (kayit.Rows.Count > 0)
            {
                DataRow datarow = kayit.Rows[0];
                decimal hamFiyat = Convert.ToDecimal(datarow["SATIS_FIAT1"]); // Sqlden kolon çekme
                decimal kdvOran = Convert.ToDecimal(datarow["KDV_ORANI"]);
                bool isKdvDahil = Convert.ToBoolean(datarow["SatisKdvDahil"]);
               

                if (hamFiyat == 0)
                {

                    BildirimHelper.MesajGoster(upStok, "Fiyatı Sıfır Olan Ürün Seçilemez", true);
                    T = false;
                    return;
                }

                var sonuc = SiparisHesaplayici.Hesapla(hamFiyat, miktar, kdvOran, isKdvDahil); // KdvHesaplama alanı
                    string sessionID = Session["SID"].ToString();
                
                    
                   string sqlStokKontrol = @"SELECT Miktar,Fiyat FROM tWebSiparisDetayTemp WHERE SessionID = @SID AND StokKodu = @SK";
                var kontrol = Db.ExecuteDataTable(sqlStokKontrol, new SqlParameter("@SID", sessionID), new SqlParameter("@SK", stokKodu));
                if (kontrol.Rows.Count > 0 )
                {
                    decimal eskiFiyat = Convert.ToDecimal(kontrol.Rows[0]["Fiyat"]);
                    decimal eskiMiktar = Convert.ToDecimal(kontrol.Rows[0]["Miktar"]); 
                    decimal eskiSatirToplam = eskiFiyat * eskiMiktar;
                    genelToplamlar.sipGenelToplam -= eskiSatirToplam;

                }

                string sql = @"IF EXISTS(SELECT 1 FROM tWebSiparisDetayTemp WHERE SessionID = @SID AND StokKodu = @SK)
        BEGIN
            UPDATE tWebSiparisDetayTemp SET Miktar = @M, KayitTarihi = GETDATE()
            WHERE SessionID = @SID AND StokKodu = @SK
        END
        ELSE
        BEGIN
            INSERT INTO tWebSiparisDetayTemp(SessionID, StokKodu, Miktar, KayitTarihi,Fiyat)
            VALUES(@SID, @SK, @M, GETDATE(),@FIYAT)
        END";

                genelToplamlar.sipGenelToplam += Convert.ToDecimal(sonuc.SatirToplam); // Satir toplam belirticez
                    Db.ExecuteNonQuery(sql, new SqlParameter("@SID", sessionID),
                       new SqlParameter("@SK", stokKodu),
                       new SqlParameter("@M", miktar),
                       new SqlParameter("@FIYAT", sonuc.BirimFiyat));
                    BildirimHelper.MesajGoster(upStok, "Ürün Eklendi", false);
                    T = true;
                    lbKullLimit.Text = kullanilabilirLimit().ToString("N2");





            }
        
           
            }
           
        

        private void TempSil(string stokKodu)
        {

            string sessionID = Session["SID"].ToString();

            var sorgu = @"Select * from tWebSiparisDetayTemp WHERE SessionId = @SID AND StokKodu = @SK";
            var kayit = Db.ExecuteDataTable(sorgu,new SqlParameter("@SID", sessionID),
                new SqlParameter("@SK",stokKodu));
            
            if (kayit.Rows.Count > 0)
            {
                var fiyatSorgu = "SELECT Fiyat from tWebSiparisDetayTemp WHERE StokKodu = @STOK";
                var fiyatSorguDb = Db.ExecuteDataTable(fiyatSorgu, new SqlParameter("@STOK", stokKodu));
                DataRow data = fiyatSorguDb.Rows[0];
                decimal fiyat = Convert.ToDecimal(data["Fiyat"]);
                
                genelToplamlar.sipGenelToplam -= (fiyat * Convert.ToDecimal(kayit.Rows[0]["Miktar"]));

                string sql = @"Delete FROM tWebSiparisDetayTemp WHERE SessionId = @SID AND StokKodu = @SK";
                string sql2 = @"Delete FROM tWebSiparis WHERE SessionId = @SID AND StokKodu = @SK";

                Db.ExecuteNonQuery(sql, new SqlParameter("@SID", sessionID),
                    new SqlParameter("@SK", stokKodu));
                Db.ExecuteNonQuery(sql2, new SqlParameter("@SID", sessionID),
                    new SqlParameter("@SK", stokKodu));
                Bind("");
                BildirimHelper.MesajGoster(upStok, "Ürün kaldırıldı", true);
                lbKullLimit.Text = kullanilabilirLimit().ToString("N2");


            }
            else
            {

                BildirimHelper.MesajGoster(upStok, "Ürün bulunamadı", true);
            }

        

        }

        protected void txtAra_TextChanged(object sender, EventArgs e)
        {
            Bind(txtAra.Text.Trim());
            txtAra.Focus();
        }

        protected void btnAktar_Click(object sender, EventArgs e)
        {
            var sipToplami = genelToplamlar.sipGenelToplam;
            var info = Services.CariService.GetFromSession();
            var risk = info.RiskLimiti;
            if (sipToplami > risk)
            {
                BildirimHelper.MesajGoster(upStok, "Risk Limitinizi Aşıyorsunuz ! Firma İle İletişime Geçiniz.", true);
              

            }
            else
            {
                if (onStokSecildi != null)
                {
                    onStokSecildi(this, new StokSecEventArgs());
                    var sql = @"delete from tWebSiparis WHERE SessionId = @SID;
                    INSERT INTO tWebSiparis SELECT * FROM tWebSiparisDetayTemp WHERE SessionId = @SID";
                    Db.ExecuteNonQuery(sql, new SqlParameter("@SID", Session.SessionID));
                }
                ScriptManager.RegisterStartupScript(this.Page, GetType(), "closeModal", "$('#stokModal').modal('hide');", true);
            }
          

        }
        public decimal kullanilabilirLimit()
        {
            var risk = info?.RiskLimiti ?? 0;
            var limit = risk - genelToplamlar.sipGenelToplam;

            if (lbKullLimit != null)
            {
                // Eğer limit negatifse kırmızı, değilse mavi olsun
                lbKullLimit.ForeColor = limit < 0 ? Color.Red : Color.Blue;
            }

            return limit;
        }
    }
    }
