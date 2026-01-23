using MobarchSipEkran.Class;
using System;
using System.Data;
using System.Data.SqlClient;
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
            string sql = @"
        SELECT 
            S.STOK_KODU, 
            S.STOK_ADI, 
            S.SATIS_FIAT1,
            ISNULL(T.Miktar, 0) AS KayitliMiktar -- Eğer temp tabloda varsa miktar gelir, yoksa 0 gelir
        FROM tStokMaster S WITH (NOLOCK)
        LEFT JOIN tSiparisDetayTemp T ON S.STOK_KODU = T.StokKodu AND T.SessionID = @SID
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
                BildirimHelper.MesajGoster(this, "Ürün Kaydı yapıldı",false);

                if (miktar > 0 )
                {
                    TempKaydet(stokKodu, miktar);
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

        private void TempKaydet(string stokKodu, decimal miktar)
        {
            string sessionID = Session["SID"].ToString(); 
            string sql = @"IF EXISTS(SELECT 1 FROM tSiparisDetayTemp WHERE SessionID = @SID AND StokKodu = @SK)
        BEGIN
            UPDATE tSiparisDetayTemp SET Miktar = @M, KayitTarihi = GETDATE()
            WHERE SessionID = @SID AND StokKodu = @SK
        END
        ELSE
        BEGIN
            INSERT INTO tSiparisDetayTemp(SessionID, StokKodu, Miktar, KayitTarihi)
            VALUES(@SID, @SK, @M, GETDATE())
        END";

            Db.ExecuteNonQuery(sql, new SqlParameter("@SID", sessionID),
                new SqlParameter("@SK", stokKodu),
                new SqlParameter("@M", miktar));
            BildirimHelper.MesajGoster(upStok, "Ürün Eklendi", false);
        }

        private void TempSil(string stokKodu)
        {
            string sessionID = Session["SID"].ToString();

            var sorgu = @"Select * from tSiparisDetayTemp WHERE SessionId = @SID AND StokKodu = @SK";
            var kayit = Db.ExecuteDataTable(sorgu,new SqlParameter("@SID", sessionID),
                new SqlParameter("@SK",stokKodu));

            if(kayit.Rows.Count > 0)
            {
                string sql = @"Delete FROM tSiparisDetayTemp WHERE SessionId = @SID AND StokKodu = @SK";

                Db.ExecuteNonQuery(sql, new SqlParameter("@SID", sessionID),
                    new SqlParameter("@SK", stokKodu));
                Bind("");
                BildirimHelper.MesajGoster(upStok, "Ürün kaldırıldı", true);

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
            if (onStokSecildi != null)
            {
                onStokSecildi(this, new StokSecEventArgs());
            }
            ScriptManager.RegisterStartupScript(this.Page, GetType(), "closeModal", "$('#stokModal').modal('hide');", true);
        }
    }
    }
