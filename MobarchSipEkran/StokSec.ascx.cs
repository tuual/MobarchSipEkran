using Microsoft.Ajax.Utilities;
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
                lbKullLimit.Text = kullanilabilirLimit().ToString("N2");

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

            string sql = @"SELECT
	                        S.STOK_KODU
                           ,S.STOK_ADI, 
	                        ISNULL(T.Miktar, 0) AS KayitliMiktar
                           ,CASE S.SatisKdvDahil
		                        WHEN 1 THEN 'Dahil'
		                        ELSE 'Hariç'
	                        END AS 'SatisKdvDahil'
                           ,CASE
		                        WHEN S.SatisKdvDahil = 1 THEN S.SATIS_FIAT1 
		                        ELSE S.SATIS_FIAT1 * (1 + (ISNULL(S.KDV_ORANI, 0) / 100)) 
	                        END AS SATIS_FIAT1,
                            S.OLCU_BR1, 
                            S.OLCU_BR2, 
                            S.OLCU_BR3,
                            T.BirimMetin,
                            S.PAY_1,
                            S.PAYDA_1,
                            S.PAY2,
                            S.PAYDA2
                        FROM tStokMaster S WITH (NOLOCK)
                        LEFT JOIN tWebSiparisDetayTemp T
	                        ON S.STOK_KODU = T.StokKodu
		                        AND T.SessionID = @SID
                        WHERE (@T = ''
                        OR S.STOK_KODU LIKE @KOD
                        OR S.STOK_ADI LIKE @AD) AND MUH_DETAYKODU = 0
                        ORDER BY T.Miktar DESC, S.STOK_ADI ASC";




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
                
                DropDownList ddl = (DropDownList)e.Row.FindControl("ddlBirimler");

                if (ddl != null)
                {
                    ddl.Items.Clear();
                    ddl.Items.Add(new ListItem(BirimIsimlendirme.BirimDuzeltme(dataRow["OLCU_BR1"].ToString()), "1/1"));

                    if (!string.IsNullOrEmpty(dataRow["OLCU_BR2"].ToString()))                   
                    {
                        string katsayi = dataRow["PAY_1"].ToString() + "/" + dataRow["PAYDA_1"].ToString();
                        ddl.Items.Add(new ListItem(BirimIsimlendirme.BirimDuzeltme(dataRow["OLCU_BR2"].ToString()), katsayi));
                    }

                    if (!string.IsNullOrEmpty(dataRow["OLCU_BR3"].ToString())) 
                    {
                        string katsayi = dataRow["PAY2"].ToString() + "/" + dataRow["PAYDA2"].ToString();
                        ddl.Items.Add(new ListItem(BirimIsimlendirme.BirimDuzeltme(dataRow["OLCU_BR3"].ToString()), katsayi));
                    }
                    string seciliBirim = dataRow["BirimMetin"].ToString();
                    ListItem secilecekItem = ddl.Items.FindByText(seciliBirim);
                    if (secilecekItem != null)
                    {
                        ddl.ClearSelection();
                        secilecekItem.Selected = true;
                    }
                }
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
                DropDownList ddlBirimler = row.FindControl("ddlBirimler") as DropDownList;
                var secilenBirim = ddlBirimler.SelectedIndex;
                string[] katsayi = ddlBirimler.SelectedValue.Split('/');
                decimal pay = Convert.ToDecimal(katsayi[0]);
                decimal payda = Convert.ToDecimal(katsayi[1]);
                var secilenBirimMetin = ddlBirimler.SelectedItem.Text.ToString();
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
                  
                  TempKaydet(stokKodu, miktar, false,secilenBirim,pay,payda,secilenBirimMetin);
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

        private void TempKaydet(string stokKodu, decimal miktar,bool T,int birim , decimal pay, decimal payda,string secilenBirimMetin)
        {
            string stokFiyat = "SELECT SATIS_FIAT1,KDV_ORANI,SatisKdvDahil from tStokMaster WHERE STOK_KODU = @STOK";
           var kayit = Db.ExecuteDataTable(stokFiyat, new SqlParameter("@STOK", stokKodu));
            if (kayit.Rows.Count > 0)
            {
                DataRow datarow = kayit.Rows[0];
                decimal hamFiyat = Convert.ToDecimal(datarow["SATIS_FIAT1"]); 
                decimal kdvOran = Convert.ToDecimal(datarow["KDV_ORANI"]);
                bool isKdvDahil = Convert.ToBoolean(datarow["SatisKdvDahil"]);
                decimal brutfiyat = 0;
                decimal netFiyat = 0;

                if (isKdvDahil)
                {
                    decimal birimFiyat = Convert.ToDecimal(hamFiyat);
                     brutfiyat = Convert.ToDecimal(birimFiyat / (1 + (kdvOran / 100)));
                }
                else
                {
                    brutfiyat = hamFiyat;

                }
                if (isKdvDahil)
                {
                    netFiyat = brutfiyat;
                }
                else
                {
                     netFiyat = hamFiyat + (hamFiyat * (kdvOran / 100));
                }
                if (hamFiyat == 0)
                {

                    BildirimHelper.MesajGoster(upStok, "Fiyatı Sıfır Olan Ürün Seçilemez", true);
                    T = false;
                    return;
                }
                decimal hesaplanmisBirimFiyat = (hamFiyat * payda) / pay;
                decimal anaBirimMiktar = miktar/ (pay / payda) ;

                var sonuc = SiparisHesaplayici.Hesapla(hesaplanmisBirimFiyat, miktar, kdvOran, isKdvDahil);
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
            UPDATE tWebSiparisDetayTemp SET Miktar = @M, KayitTarihi = GETDATE(), Fiyat = @FIYAT, Birim = @BR, BirimMetin = @BIRIMMETIN, Miktar2 = @ANABIRIMMIKTAR, BrutFiyat = @BRUTFIYAT, NetFiyat = @NETFIYAT
            WHERE SessionID = @SID AND StokKodu = @SK
        END
        ELSE
        BEGIN
            INSERT INTO tWebSiparisDetayTemp(SessionID, StokKodu, Miktar, KayitTarihi, Fiyat, Birim,Miktar2,BrutFiyat,NetFiyat,BirimMetin)
            VALUES(@SID, @SK, @M, GETDATE(), @FIYAT, @BR,@ANABIRIMMIKTAR,@BRUTFIYAT,@NETFIYAT,@BIRIMMETIN)
        END";

                genelToplamlar.sipGenelToplam += Convert.ToDecimal(sonuc.SatirToplam); // Satir toplam belirticez
                    Db.ExecuteNonQuery(sql, new SqlParameter("@SID", sessionID),
                       new SqlParameter("@SK", stokKodu),
                       new SqlParameter("@M", miktar),
                       new SqlParameter("@BR", birim),
                       new SqlParameter("@BRUTFIYAT",brutfiyat),
                       new SqlParameter("@NETFIYAT",netFiyat),
                       new SqlParameter("@BIRIMMETIN", secilenBirimMetin),
                       new SqlParameter("@ANABIRIMMIKTAR", anaBirimMiktar),
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
            var info = Services.CariService.GetFromSession();
            if (info == null) return;

            decimal toplamRisk = info.Bakiye + genelToplamlar.sipGenelToplam;

            if (info.RiskLimiti > 0 && toplamRisk > info.RiskLimiti)
            {
                decimal fark = toplamRisk - info.RiskLimiti;
                BildirimHelper.MesajGoster(upStok, $"Risk limiti aşıldı! Sipariş giremezsiniz. Aşım: {fark:N2} TL", true);
                return;
            }

            if (onStokSecildi != null)
            {
                onStokSecildi(this, new StokSecEventArgs());
                var sql = @"delete from tWebSiparis WHERE SessionId = @SID;
                    INSERT INTO tWebSiparis SELECT * FROM tWebSiparisDetayTemp WHERE SessionId = @SID";
                Db.ExecuteNonQuery(sql, new SqlParameter("@SID", Session["SID"])); 
                
            }
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "refresh", "kayitBasarili()",true);
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "closeModal", "$('#stokModal').modal('hide');", true);
        }
        public decimal kullanilabilirLimit()
        {
            var info = Services.CariService.GetFromSession();
            if (info == null) return 0;

            string sql = "SELECT SUM(Miktar * Fiyat) FROM tWebSiparisDetayTemp WHERE SessionID = @SID";
            object result = Db.ExecuteScalar(sql, new SqlParameter("@SID", Session["SID"]));

            decimal suAnkiSepetToplami = (result == DBNull.Value || result == null) ? 0 : Convert.ToDecimal(result);

            genelToplamlar.sipGenelToplam = suAnkiSepetToplami;

            var risk = info.RiskLimiti;
            var bakiye = info.Bakiye;

            var limit = risk - (bakiye + suAnkiSepetToplami);

            if (lbKullLimit != null)
            {
                lbKullLimit.ForeColor = limit < 0 ? Color.Red : Color.Blue;
                lbKullLimit.Text = limit.ToString("N2");
            }

            return limit;
        }
    }
    }
